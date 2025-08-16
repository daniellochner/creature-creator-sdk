using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ModdingMenus : MonoBehaviour
{
	[MenuItem("Creature Creator/Connect to Steam", priority = 100)]
	public static void ConnectToSteam()
	{
		EditorSteamManager.ConnectToSteam();
	}

	[MenuItem("Creature Creator/Locate Creature Creator.exe", priority = 101)]
	public static void LocateExe()
	{
        ModdingUtils.SetApplicationPath();
	}

    #region Map
    [MenuItem("Creature Creator/Map/New", priority = 50)]
    public static void NewMap()
    {
        MappingUtils.NewMap();
    }

    [MenuItem("Creature Creator/Map/Check For Errors _F3", priority = 299)]
	public static void CheckForErrors()
	{
		MappingUtils.CheckForErrors();
	}

	[MenuItem("Creature Creator/Map/Build _F4", priority = 300)]
	public static void BuildMap()
	{
		MappingUtils.BuildMap(MapConfig.GetCurrent(), false);
	}

	[MenuItem("Creature Creator/Map/Test", priority = 301)]
	public static void TestMap()
	{
		MappingUtils.TestMap(MapConfig.GetCurrent());
	}

	[MenuItem("Creature Creator/Map/Build and Test _F5", priority = 302)]
	public static void BuildAndTestMap()
	{
		MappingUtils.BuildAndTestMap(MapConfig.GetCurrent());
	}

    [MenuItem("Creature Creator/Map/Upload to Workshop", priority = 303)]
    public static void UploadMap()
    {
        MappingUtils.BuildAndUploadMap(MapConfig.GetCurrent());
    }
    #endregion

    #region Body Part
    #endregion
}