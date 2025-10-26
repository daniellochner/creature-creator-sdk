using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using DanielLochner.Assets.CreatureCreator;
using UnityEditor;

public static class PatternUtils
{
    private static float MAX_PATTERN_SIZE = 1f;

    public static void NewPattern()
    {
        if (ModdingUtils.TryCreateNewItem(out string patternName, out string patternPath, out PatternConfig config))
        {
            string dstPath = Path.Combine(patternPath, $"{patternName}.png");
            AssetDatabase.CopyAsset("Assets/CreatureCreatorSDK/Internal/Templates/Pattern.png", dstPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            config.thumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>(ModdingUtils.ConvertGlobalPathToLocalPath(dstPath));
        }
    }

    public static bool BuildPattern(PatternConfig config, bool buildAll)
    {
        string patternName = config.GetDirectoryName();

        string[] images = Directory.GetFiles(config.GetFullDirectory(), $"{patternName}.png", SearchOption.AllDirectories);
        if (images.Length != 1)
        {
            ModdingUtils.ThrowError($"One image (PNG) must exist with the name '{patternName}' (i.e., it must match the directory's name).");
            return false;
        }

        return ModdingUtils.TryBuildItem<PatternConfig, PatternConfigData>(config, buildAll);
    }

    public static void TestPattern(PatternConfig config)
    {
        var path = ModdingUtils.GetBuildPath(config);
        if (!Directory.Exists(path))
        {
            ModdingUtils.ThrowError("You have not built this pattern yet. You have to build it before testing.");
            return;
        }

        var windowsBundlePath = Path.Combine(path, "Pattern", "Bundles_WindowsPlayer");
        if (ModdingUtils.IsTooLarge(windowsBundlePath, MAX_PATTERN_SIZE))
        {
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "loadpattern");
    }

    public static void UploadPattern(PatternConfig config)
    {
        var path = ModdingUtils.GetBuildPath(config);
        if (!Directory.Exists(path))
        {
            ModdingUtils.ThrowError("You have not built this pattern yet. You have to build it before uploading.");
            return;
        }

        if (ModdingUtils.IsTooLarge(path, MAX_PATTERN_SIZE * 5f))
        {
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "uploadpattern");
    }
}
