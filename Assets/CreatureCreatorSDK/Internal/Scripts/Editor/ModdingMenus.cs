using System;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ModdingMenus : MonoBehaviour
{
    #region General
    [MenuItem("Creature Creator/Connect to Steam", priority = 101)]
	public static void ConnectToSteam()
	{
		EditorSteamManager.ConnectToSteam();
	}

	[MenuItem("Creature Creator/Locate Creature Creator.exe", priority = 102)]
	public static void LocateExe()
	{
        ModdingUtils.SetApplicationPath();
	}

    [MenuItem("Creature Creator/Build", priority = 51)]
    public static void Build()
    {
        PerformOperation(BuildMap, BuildBodyPart, BuildPattern);
    }
    [MenuItem("Creature Creator/Test", priority = 52)]
    public static void Test()
    {
        PerformOperation(TestMap, TestBodyPart, TestPattern);
    }
    [MenuItem("Creature Creator/Build and Test", priority = 53)]
    public static void BuildAndTest()
    {
        PerformOperation(BuildAndTestMap, BuildAndTestBodyPart, BuildAndTestPattern);
    }
    [MenuItem("Creature Creator/Upload to Workshop", priority = 54)]
    public static void UploadToWorkshop()
    {
        PerformOperation(BuildAndUploadMap, BuildAndUploadBodyPart, BuildAndUploadPattern);
    }

    public static void PerformOperation(Action<MapConfig> onMap, Action<BodyPartConfig> onBodyPart, Action<PatternConfig> onPattern)
    {
        if (MapConfig.GetSelected() is MapConfig mapConfig)
        {
            if (mapConfig != MapConfig.GetCurrent())
            {
                ModdingUtils.ThrowError("The selected map is not loaded.");
            }
            else
            {
                onMap?.Invoke(mapConfig);
            }
        }
        else
        if (BodyPartConfig.GetSelected() is BodyPartConfig bodyPartConfig)
        {
            onBodyPart?.Invoke(bodyPartConfig);
        }
        else
        if (PatternConfig.GetSelected() is PatternConfig patternConfig)
        {
            onPattern?.Invoke(patternConfig);
        }
        else
        {
            ModdingUtils.ThrowError("Please select a config file to perform an operation.");
        }
    }

    [MenuItem("Creature Creator/Build", true)]
    [MenuItem("Creature Creator/Test", true)]
    [MenuItem("Creature Creator/Build and Test", true)]
    [MenuItem("Creature Creator/Upload to Workshop", true)]
    private static bool ValidateConfig()
    {
        if (MapConfig.GetSelected() != null)
        {
            return true;
        }
        else
        if (BodyPartConfig.GetSelected() != null)
        {
            return true;
        }
        else
        if (PatternConfig.GetSelected() != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    #region Map
    [MenuItem("Creature Creator/New/Map", priority = 1)]
    public static void NewMap()
    {
        MappingUtils.NewMap();
    }

    private static bool TryBuildMapDependencies(MapConfig config)
    {
        foreach (var bodyPart in config.linkedBodyParts)
        {
            BuildBodyPart(bodyPart.config as BodyPartConfig);
            SetupLinkedItem(bodyPart);
        }
        foreach (var pattern in config.linkedPatterns)
        {
            BuildPattern(pattern.config as PatternConfig);
            SetupLinkedItem(pattern);
        }
        return true;
    }
    private static void SetupLinkedItem(MapConfig.LinkedItem item)
    {
        string persistentDataPath = Path.Combine(Application.persistentDataPath, "..", "..", "Daniel Lochner", "Creature Creator");

        string modsDir = Path.Combine(persistentDataPath, "mods");
        if (!Directory.Exists(modsDir))
        {
            Directory.CreateDirectory(modsDir);
        }

        string dirName = item.config.Singular.ToLower().Replace(" ", "");
        string dirPath = Path.Combine(modsDir, dirName);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        string src = ModdingUtils.GetBuildPath(item.config);
        string dst = Path.Combine(dirPath, item.itemId);
        if (Directory.Exists(dst))
        {
            Directory.Delete(dst, true);
        }
        Directory.Move(src, dst);
    }

    public static void BuildMap(MapConfig config)
	{
        if (TryBuildMapDependencies(config))
        {
            MappingUtils.BuildMap(config, false);
        }
    }
	public static void TestMap(MapConfig config)
	{
        MappingUtils.TestMap(config);
    }
    public static void BuildAndTestMap(MapConfig config)
	{
        if (TryBuildMapDependencies(config))
        {
            if (MappingUtils.BuildMap(config, false))
            {
                MappingUtils.TestMap(config);
            }
        }
	}
    public static void BuildAndUploadMap(MapConfig config)
    {
        if (MappingUtils.BuildMap(config, true))
        {
            MappingUtils.UploadMap(config);
        }
    }
    #endregion

    #region Body Part
    [MenuItem("Creature Creator/New/Body Part", priority = 2)]
    public static void NewBodyPart()
    {
        BodyPartUtils.NewBodyPart();
    }
    
    public static void BuildBodyPart(BodyPartConfig config)
	{
		BodyPartUtils.BuildBodyPart(config, false);
	}
    public static void TestBodyPart(BodyPartConfig config)
    {
        BodyPartUtils.TestBodyPart(config);
    }
    public static void BuildAndTestBodyPart(BodyPartConfig config)
    {
        if (BodyPartUtils.BuildBodyPart(config, false))
        {
            BodyPartUtils.TestBodyPart(config);
        }
    }
    public static void BuildAndUploadBodyPart(BodyPartConfig config)
    {
        if (BodyPartUtils.BuildBodyPart(config, true))
        {
            BodyPartUtils.UploadBodyPart(config);
        }
    }
    #endregion

    #region Pattern
    [MenuItem("Creature Creator/New/Pattern", priority = 3)]
    public static void NewPattern()
    {
        PatternUtils.NewPattern();
    }

    public static void BuildPattern(PatternConfig config)
    {
        PatternUtils.BuildPattern(config, false);
    }
    public static void TestPattern(PatternConfig config)
    {
        PatternUtils.TestPattern(config);
    }
    public static void BuildAndTestPattern(PatternConfig config)
    {
        if (PatternUtils.BuildPattern(config, false))
        {
            PatternUtils.TestPattern(config);
        }
    }
    public static void BuildAndUploadPattern(PatternConfig config)
    {
        if (PatternUtils.BuildPattern(config, true))
        {
            PatternUtils.UploadPattern(config);
        }
    }
    #endregion
}