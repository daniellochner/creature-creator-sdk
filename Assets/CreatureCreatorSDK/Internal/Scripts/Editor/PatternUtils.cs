using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using DanielLochner.Assets.CreatureCreator;
using UnityEditor;

public static class PatternUtils
{
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
        string path = ModdingUtils.GetBuildPath(config);

        if (IsTooLarge(path))
        {
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "loadpattern");
    }

    public static void UploadPattern(PatternConfig config)
    {
        string path = ModdingUtils.GetBuildPath(config);

        if (IsTooLarge(path))
        {
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "uploadpattern");
    }

    private static bool IsTooLarge(string path)
    {
        var windowsBundlePath = Path.Combine(path, "Pattern", "Bundles_WindowsPlayer");
        var maxFileSizeMB = 1f;
        if (ModdingUtils.CheckFileSize(windowsBundlePath, maxFileSizeMB, out float fileSizeMB))
        {
            ModdingUtils.ThrowError($"Your custom pattern is too large! ({fileSizeMB:0.00}MB > {maxFileSizeMB:0.00}MB)");
            return true;
        }
        return false;
    }
}
