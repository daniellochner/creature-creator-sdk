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

		AssetDatabase.CopyAsset("Assets/Creature Creator SDK/Toolkit/Scenes/Template.unity", scenePath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Selection.activeObject = AssetDatabase.LoadAssetAtPath<MapConfig>(mapConfigPath);

		EditorSceneManager.OpenScene(scenePath);
	}

	public static bool BuildMap(MapConfig config)
	{
		if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
		{
			return false;
		}

		var startTime = DateTime.Now;
		Debug.Log("Map build started");

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
            info.OnValidate();
        }
        CustomMapSecurityValidator.SanitizeAnimators(scene);
        EditorSceneManager.SaveOpenScenes();

        string buildPath = GetBuildPath(config);
        string configPath = Path.Combine(buildPath, "config.json");
        MapConfigData prevData = null;

        if (File.Exists(configPath))
        {
            prevData = JsonConvert.DeserializeObject<MapConfigData>(File.ReadAllText(configPath));
        }

        if (Directory.Exists(buildPath))
		{
			Directory.Delete(buildPath, true);
		}

		Directory.CreateDirectory(buildPath);

		string bundleBuildPath = GetBundleBuildPath(config);

		Directory.CreateDirectory(bundleBuildPath);

		if(config.thumbnail != null)
		{
			string thumbnailPath = ConvertLocalPathToGlobalPath(AssetDatabase.GetAssetPath(config.thumbnail));
			string thumbnailBuildPath = Path.Combine(buildPath, "thumb.png");

			File.Copy(thumbnailPath, thumbnailBuildPath);
		}

		AssetBundleBuilder.AssignBundleNames(config);

		AssetBundleBuilder.BuildAssetBundles(config, bundleBuildPath);

		foreach(var file in new DirectoryInfo(bundleBuildPath).GetFiles("*.manifest"))
		{
			File.Delete(file.FullName);
		}

        // the file that gets generated with the same name as the folder is the AssetBundleManifest
        // and is apparently useful to have in some situations. It's small so there's not really
        // a reason to delete it.

        // do this last because hot reloading uses it to determine a completed build

        string nextDataJson = config.GetJSON();
        MapConfigData nextData = JsonConvert.DeserializeObject<MapConfigData>(nextDataJson);
        if (prevData != null)
        {
            nextData.ItemId = prevData.ItemId;
        }
        File.WriteAllText(configPath, JsonConvert.SerializeObject(nextData, Formatting.Indented));

		Debug.Log($"Build completed in {DateTime.Now.Subtract(startTime).TotalSeconds.ToString("0")} seconds: {buildPath}");
		return true;
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