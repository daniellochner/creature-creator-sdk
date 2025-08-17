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
        return ModdingUtils.TryBuildItem<PatternConfig, PatternConfigData>(PatternConfig.GetCurrent(), buildAll);
    }

    public static void TestPattern(PatternConfig config)
    {
        string path = ModdingUtils.GetBuildPath(config);
        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "loadpattern");
    }

    public static void UploadPattern(PatternConfig config)
    {

    }
}
