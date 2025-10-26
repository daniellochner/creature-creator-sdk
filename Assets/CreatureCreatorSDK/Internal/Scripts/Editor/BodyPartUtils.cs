using System.IO;
using DanielLochner.Assets.CreatureCreator;
using UnityEditor;
using UnityEngine;

public static class BodyPartUtils
{
    public static void NewBodyPart()
    {
        if (ModdingUtils.TryCreateNewItem(out string bodyPartName, out string bodyPartPath, out BodyPartConfig config))
        {
            string dstPath = Path.Combine(bodyPartPath, $"{bodyPartName}.prefab");
            AssetDatabase.CopyAsset("Assets/CreatureCreatorSDK/Internal/Templates/BodyPart.prefab", dstPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    public static bool BuildBodyPart(BodyPartConfig config, bool buildAll)
    {
        string bodyPartName = config.GetDirectoryName();

        string[] prefabs = Directory.GetFiles(config.GetFullDirectory(), $"{bodyPartName}.prefab", SearchOption.AllDirectories);
        if (prefabs.Length != 1)
        {
            ModdingUtils.ThrowError($"One prefab must exist with the name '{bodyPartName}' (i.e., it must match the directory's name).");
            return false;
        }

        return ModdingUtils.TryBuildItem<BodyPartConfig, BodyPartConfigData>(config, buildAll, delegate
        {
            string excludeDir = Path.Combine(config.GetDirectory(), "Exclude");
            if (Directory.Exists(excludeDir))
            {
                string thumbnailPath = Path.Combine(excludeDir, "thumb.png");
                if (File.Exists(thumbnailPath))
                {
                    Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);
                    config.thumbnail = savedTexture;
                }
            }
        });
    }

    public static void TestBodyPart(BodyPartConfig config)
    {
        string path = ModdingUtils.GetBuildPath(config);

        if (IsTooLarge(path))
        {
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "loadbodypart");
    }

    public static void UploadBodyPart(BodyPartConfig config)
    {
        string path = ModdingUtils.GetBuildPath(config);

        if (IsTooLarge(path))
        {
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "uploadbodypart");
    }

    private static bool IsTooLarge(string path)
    {
        var windowsBundlePath = Path.Combine(path, "Body Part", "Bundles_WindowsPlayer");
        var maxFileSizeMB = 1f;
        if (ModdingUtils.CheckFileSize(windowsBundlePath, maxFileSizeMB, out float fileSizeMB))
        {
            ModdingUtils.ThrowError($"Your custom body part is too large! ({fileSizeMB:0.00}MB > {maxFileSizeMB:0.00}MB)");
            return true;
        }
        return false;
    }
}
