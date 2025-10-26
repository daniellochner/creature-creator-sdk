using System.IO;
using DanielLochner.Assets.CreatureCreator;
using UnityEditor;
using UnityEngine;

public static class BodyPartUtils
{
    private static float MAX_BODYPART_SIZE = 1f;

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
        var path = ModdingUtils.GetBuildPath(config);
        if (!Directory.Exists(path))
        {
            ModdingUtils.ThrowError("You have not built this body part yet. You have to build it before testing.");
            return;
        }

        var windowsBundlePath = Path.Combine(path, "Body Part", "Bundles_WindowsPlayer");
        if (ModdingUtils.IsTooLarge(windowsBundlePath, MAX_BODYPART_SIZE))
        {
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "loadbodypart");
    }

    public static void UploadBodyPart(BodyPartConfig config)
    {
        var path = ModdingUtils.GetBuildPath(config);
        if (!Directory.Exists(path))
        {
            ModdingUtils.ThrowError("You have not built this body part yet. You have to build it before uploading.");
            return;
        }

        if (ModdingUtils.IsTooLarge(path, MAX_BODYPART_SIZE * 5f))
        {
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "uploadbodypart");
    }
}
