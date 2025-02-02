using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public static class MappingUtils
{
	public static string SetApplicationPath()
	{
		string path = EditorUtility.OpenFilePanel("Find Creature Creator.exe", EditorSteamManager.GetInstallFolder(), "exe");

		if(string.IsNullOrEmpty(path))
			return null;

		Debug.Log("Set Creature Creator.exe path to " + path);

		PlayerPrefs.SetString(Constants.PlayerPrefsApplicationPathKey, path);
		PlayerPrefs.Save();
		return path;
	}

	public static void CheckForErrors()
	{
        if (CustomMapValidator.IsSceneValid(SceneManager.GetActiveScene(), out string error))
        {
            EditorUtility.DisplayDialog("There are no errors.", "Everything is OK!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", error, "OK");
        }
    }

	public static void NewMap()
	{
		string mapName = EditorInputDialog.Show("New Map", "Create a New Map", "Map Name");

		string mapDirectory = Path.Combine(Application.dataPath, "Maps", mapName);

		if(Directory.Exists(mapDirectory))
		{
			ThrowError($"The map {mapName} already exists at {mapDirectory}.");
		}

		Directory.CreateDirectory(mapDirectory);

		string scenePath = Path.Combine(mapDirectory, $"{mapName}.unity");
		string mapConfigPath = Path.Combine(ConvertGlobalPathToLocalPath(mapDirectory), "config.asset");

		MapConfig mapConfig = ScriptableObject.CreateInstance<MapConfig>();
		mapConfig.bundleName = mapName.ToLower().Replace(' ', '_');
		mapConfig.name = mapName;
		AssetDatabase.CreateAsset(mapConfig, mapConfigPath);

		AssetDatabase.CopyAsset("Assets/CreatureCreatorSDK/Toolkit/Scenes/Template.unity", scenePath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Selection.activeObject = AssetDatabase.LoadAssetAtPath<MapConfig>(mapConfigPath);

		EditorSceneManager.OpenScene(scenePath);
	}

    public static void GenerateThumbnail(MapConfig config)
    {
        if (ImageGenerator.TryGetThumbnail("MapThumbnailCamera", 512, 512, out Texture2D tex))
        {
            string thumbnailDirectory = Path.Combine(config.GetMapDirectory(), "Exclude");

            if (!Directory.Exists(thumbnailDirectory))
            {
                Directory.CreateDirectory(thumbnailDirectory);
            }

            string thumbnailPath = Path.Combine(thumbnailDirectory, "thumb.png");

            byte[] textureData = tex.EncodeToPNG();
            File.WriteAllBytes(thumbnailPath, textureData);

            AssetDatabase.Refresh();

            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);
            config.thumbnail = savedTexture;
            EditorUtility.SetDirty(config);
        }
    }

	public static bool BuildMap(MapConfig config)
	{
		if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
		{
			return false;
		}

		var startTime = DateTime.Now;
		Debug.Log("Map build started");

        // Scene
        string[] scenes = Directory.GetFiles(config.GetFullMapDirectory(), "*.unity", SearchOption.AllDirectories);
		if(scenes.Length > 1)
		{
			ThrowError("More than one scene found in the map folder.");
		}
		Scene scene = SceneManager.GetActiveScene();
        if (!CustomMapValidator.IsSceneValid(scene, out string error))
        {
            ThrowError(error);
        }
        if (scene.TryGetComponent(out MapInfo info))
        {
            info.Setup();
        }
        CustomMapSecurityValidator.SanitizeAnimators(scene);
        EditorSceneManager.SaveOpenScenes();

        // Previous config
        string buildPath = GetBuildPath(config);
        string configPath = Path.Combine(buildPath, "config.json");
        MapConfigData prevConfigData = null;
        if (File.Exists(configPath))
        {
            prevConfigData = JsonConvert.DeserializeObject<MapConfigData>(File.ReadAllText(configPath));
        }

        // Delete existing build
        if (Directory.Exists(buildPath))
		{
			Directory.Delete(buildPath, true);
		}
		Directory.CreateDirectory(buildPath);

        // Build asset bundles
        BuildBundlesForPlatform(config, RuntimePlatform.WindowsPlayer);
        BuildBundlesForPlatform(config, RuntimePlatform.OSXPlayer);
        BuildBundlesForPlatform(config, RuntimePlatform.LinuxPlayer);
        BuildBundlesForPlatform(config, RuntimePlatform.IPhonePlayer);
        BuildBundlesForPlatform(config, RuntimePlatform.Android);

        // Thumbnail
        GenerateThumbnail(config);
        if (config.thumbnail != null)
        {
            string thumbnailPath = ConvertLocalPathToGlobalPath(AssetDatabase.GetAssetPath(config.thumbnail));
            string thumbnailBuildPath = Path.Combine(buildPath, "thumb.png");

            File.Copy(thumbnailPath, thumbnailBuildPath);
        }

        // Config
        string nextDataJson = config.GetJSON();
        MapConfigData nextData = JsonConvert.DeserializeObject<MapConfigData>(nextDataJson);
        if (prevConfigData != null)
        {
            nextData.ItemId = prevConfigData.ItemId;
        }
        File.WriteAllText(configPath, JsonConvert.SerializeObject(nextData, Formatting.Indented));

		Debug.Log($"Build completed in {DateTime.Now.Subtract(startTime).TotalSeconds.ToString("0")} seconds: {buildPath}");
		return true;
	}

    private static void BuildBundlesForPlatform(MapConfig config, RuntimePlatform platform)
    {
        string bundleBuildPath = GetBundleBuildPath(config) + $"_{platform}";

        Directory.CreateDirectory(bundleBuildPath);

        AssetBundleBuilder.AssignBundleNames(config);

        BuildTarget buildTarget = default;
        switch (platform)
        {
            case RuntimePlatform.WindowsPlayer:
                buildTarget = BuildTarget.StandaloneWindows64;
                break;

            case RuntimePlatform.OSXPlayer:
                buildTarget = BuildTarget.StandaloneOSX;
                break;

            case RuntimePlatform.LinuxPlayer:
                buildTarget = BuildTarget.StandaloneLinux64;
                break;

            case RuntimePlatform.IPhonePlayer:
                buildTarget = BuildTarget.iOS;
                break;

            case RuntimePlatform.Android:
                buildTarget = BuildTarget.Android;
                break;
        }

        AssetBundleBuilder.BuildAssetBundles(config, bundleBuildPath, buildTarget);

        foreach (var file in new DirectoryInfo(bundleBuildPath).GetFiles("*.manifest"))
        {
            File.Delete(file.FullName);
        }
    }


	public static void TestMap(MapConfig config)
	{
		string path = GetBuildPath(config);

		if(!Directory.Exists(path))
		{
			ThrowError("You have not built this map yet. You have to build it before testing.");
		}

		StartGame(GetApplicationPath(), path, "loadmap");
	}

	public static void UploadMap(MapConfig config)
	{
		if(config.thumbnail == null)
		{
			ThrowError("Missing thumbnail. Assign a thumbnail in the config file of your map.");
		}

		StartGame(GetApplicationPath(), GetBuildPath(config), "uploadmap");
	}

	public static void BuildAndTestMap(MapConfig config)
	{
		if(BuildMap(config))
		{
			TestMap(config);
		}
	}

	public static string GetApplicationPath()
	{
		string applicationPath;

		if(PlayerPrefs.HasKey(Constants.PlayerPrefsApplicationPathKey))
		{
			applicationPath = PlayerPrefs.GetString(Constants.PlayerPrefsApplicationPathKey);
		}
		else
		{
			applicationPath = EditorSteamManager.GetInstallLocation();
		}

		if(!File.Exists(applicationPath))
		{
			Debug.LogError("The application was not found at the specified path: " + applicationPath);
			applicationPath = SetApplicationPath();
		}

		if(!File.Exists(applicationPath))
		{
			ThrowError("The application was not found at the specified path: " + applicationPath);
		}

		return applicationPath;
	}

	public static string GetBuildPath(MapConfig config)
	{
		return Path.Combine(Application.dataPath, "..", "Built Maps", config.name);
	}

	public static string GetBundleBuildPath(MapConfig config)
	{
        return Path.Combine(GetBuildPath(config), "Map", "Bundles");
	}

	public static void StartGame(string applicationPath, string mapPath, string arg)
	{
		Process process = new Process();
		process.StartInfo.FileName = applicationPath;
		process.StartInfo.Arguments = $"-{arg} \"{mapPath}\"";
		process.Start();
		Debug.Log("Starting game with arguments: " + process.StartInfo.Arguments);
	}

	public static string ConvertLocalPathToGlobalPath(string localPath)
	{
		return Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length), localPath);
	}

	public static string ConvertGlobalPathToLocalPath(string globalPath)
	{
		return Path.Combine(globalPath.Substring(Application.dataPath.Length - "Assets".Length));
	}

	static void ThrowError(string error)
	{
		EditorUtility.DisplayDialog("Error", error, "OK");
		throw new Exception(error);
	}
}