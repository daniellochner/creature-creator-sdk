using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
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
    #endregion

    #region Map
    [MenuItem("Creature Creator/Map/New", priority = 1)]
    public static void NewMap()
    {
        MappingUtils.NewMap();
    }

    [MenuItem("Creature Creator/Map/Check For Errors _F3", priority = 51)]
    public static void CheckForErrors()
	{
		MappingUtils.CheckForErrors();
	}

	[MenuItem("Creature Creator/Map/Build _F4", priority = 52)]
	public static void BuildMap()
	{
		MappingUtils.BuildMap(MapConfig.GetCurrent(), false);
	}

	[MenuItem("Creature Creator/Map/Test", priority = 53)]
	public static void TestMap()
	{
		MappingUtils.TestMap(MapConfig.GetCurrent());
	}

	[MenuItem("Creature Creator/Map/Build and Test _F5", priority = 54)]
	public static void BuildAndTestMap()
	{
        if (MappingUtils.BuildMap(MapConfig.GetCurrent(), false))
        {
            MappingUtils.TestMap(MapConfig.GetCurrent());
        }
	}

    [MenuItem("Creature Creator/Map/Upload to Workshop", priority = 55)]
    public static void BuildAndUploadMap()
    {
        if (MappingUtils.BuildMap(MapConfig.GetCurrent(), true))
        {
            MappingUtils.UploadMap(MapConfig.GetCurrent());
        }
    }
    #endregion

    #region Body Part
    [MenuItem("Creature Creator/Body Part/New", priority = 2)]
    public static void NewBodyPart()
    {
        BodyPartUtils.NewBodyPart();
    }

    [MenuItem("Creature Creator/Body Part/Build", priority = 51)]
    public static void BuildBodyPart()
	{
		BodyPartUtils.BuildBodyPart(BodyPartConfig.GetCurrent(), false);
	}

    [MenuItem("Creature Creator/Body Part/Test", priority = 52)]
    public static void TestBodyPart()
    {
        BodyPartUtils.TestBodyPart(BodyPartConfig.GetCurrent());
    }

    [MenuItem("Creature Creator/Body Part/Build and Test", priority = 53)]
    public static void BuildAndTestBodyPart()
    {
        if (BodyPartUtils.BuildBodyPart(BodyPartConfig.GetCurrent(), false))
        {
            BodyPartUtils.TestBodyPart(BodyPartConfig.GetCurrent());
        }
    }
    
    [MenuItem("Creature Creator/Body Part/Upload to Workshop", priority = 54)]
    public static void BuildAndUploadBodyPart()
    {
        if (BodyPartUtils.BuildBodyPart(BodyPartConfig.GetCurrent(), true))
        {
            BodyPartUtils.UploadBodyPart(BodyPartConfig.GetCurrent());
        }
    }

    [MenuItem("Creature Creator/Body Part/Build", true)]
    [MenuItem("Creature Creator/Body Part/Test", true)]
    [MenuItem("Creature Creator/Body Part/Build and Test", true)]
    [MenuItem("Creature Creator/Body Part/Upload to Workshop", true)]
    private static bool ValidateBodyPartConfig()
    {
        return BodyPartConfig.GetCurrent() != null;
    }
    #endregion

    #region Pattern
    [MenuItem("Creature Creator/Pattern/New", priority = 3)]
    public static void NewPattern()
    {
        PatternUtils.NewPattern();
    }

    [MenuItem("Creature Creator/Pattern/Build", priority = 51)]
    public static void BuildPattern()
    {
        PatternUtils.BuildPattern(PatternConfig.GetCurrent(), false);
    }

    [MenuItem("Creature Creator/Pattern/Test", priority = 52)]
    public static void TestPattern()
    {
        PatternUtils.TestPattern(PatternConfig.GetCurrent());
    }

    [MenuItem("Creature Creator/Pattern/Build and Test", priority = 53)]
    public static void BuildAndTestPattern()
    {
        if (PatternUtils.BuildPattern(PatternConfig.GetCurrent(), false))
        {
            PatternUtils.TestPattern(PatternConfig.GetCurrent());
        }
    }

    [MenuItem("Creature Creator/Pattern/Upload to Workshop", priority = 54)]
    public static void BuildAndUploadPattern()
    {
        if (PatternUtils.BuildPattern(PatternConfig.GetCurrent(), true))
        {
            PatternUtils.UploadPattern(PatternConfig.GetCurrent());
        }
    }

    [MenuItem("Creature Creator/Pattern/Build", true)]
    [MenuItem("Creature Creator/Pattern/Test", true)]
    [MenuItem("Creature Creator/Pattern/Build and Test", true)]
    [MenuItem("Creature Creator/Pattern/Upload to Workshop", true)]
    private static bool ValidatePatternConfig()
    {
        return PatternConfig.GetCurrent() != null;
    }
    #endregion
}