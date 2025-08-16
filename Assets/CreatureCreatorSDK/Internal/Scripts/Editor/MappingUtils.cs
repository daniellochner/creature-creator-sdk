using Codice.Utils;
using DanielLochner.Assets.CreatureCreator;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MappingUtils
{
    public static void NewMap()
	{
        if (ModdingUtils.TryCreateNewItem<MapConfig>(out string mapName, out string mapPath))
        {
            string dstPath = Path.Combine(mapPath, $"{mapName}.unity");
            AssetDatabase.CopyAsset("Assets/CreatureCreatorSDK/Internal/Templates/Map.unity", dstPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(dstPath);
        }
    }

    public static bool BuildMap(MapConfig config, bool buildAll)
	{
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return false;
        }

        return ModdingUtils.TryBuildItem<MapConfig, MapConfigData>(config, buildAll, delegate (string buildPath)
        {
            string[] scenes = Directory.GetFiles(config.GetFullDirectory(), "*.unity", SearchOption.AllDirectories);
            if (scenes.Length > 1)
            {
                ModdingUtils.ThrowError("More than one scene found in the map folder.");
            }
            Scene scene = SceneManager.GetActiveScene();
            if (!CustomMapValidator.IsSceneValid(scene, out string error))
            {
                ModdingUtils.ThrowError(error);
            }
            CustomMapSecurityValidator.SanitizeAnimators(scene);
            EditorSceneManager.SaveOpenScenes();
            GenerateThumbnail(config);

            if (config.thumbnail != null)
            {
                string thumbnailPath = ModdingUtils.ConvertLocalPathToGlobalPath(AssetDatabase.GetAssetPath(config.thumbnail));
                string thumbnailBuildPath = Path.Combine(buildPath, "thumb.png");

                File.Copy(thumbnailPath, thumbnailBuildPath);
            }
        });
	}

	public static void TestMap(MapConfig config)
	{
		string path = ModdingUtils.GetBuildPath(config);

		if (!Directory.Exists(path))
		{
			ModdingUtils.ThrowError("You have not built this map yet. You have to build it before testing.");
			return;
		}

        if (ModdingUtils.CheckFileSize(path, out double fileSizeMB, out double maxFileSizeMB))
        {
            ModdingUtils.ThrowError($"Your custom map is too large! ({fileSizeMB:00}MB > {maxFileSizeMB:00}MB)");
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "loadmap");
	}

	public static void UploadMap(MapConfig config)
	{
        string path = ModdingUtils.GetBuildPath(config);

        if (!Directory.Exists(path))
        {
            ModdingUtils.ThrowError("You have not built this map yet. You have to build it before uploading.");
            return;
        }

        if (config.thumbnail == null)
		{
			ModdingUtils.ThrowError("Missing thumbnail. Assign a thumbnail in the config file of your map.");
			return;
		}

        if (ModdingUtils.CheckFileSize(path, out double fileSizeMB, out double maxFileSizeMB))
        {
            ModdingUtils.ThrowError($"Your custom map is too large! ({fileSizeMB:00}MB > {maxFileSizeMB:00}MB)");
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "uploadmap");
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
    public static void GenerateThumbnail(MapConfig config)
    {
        if (ImageGenerator.TryGetThumbnail(512, 512, out Texture2D tex))
        {
            string thumbnailDirectory = Path.Combine(config.GetDirectory(), "Exclude");

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
}