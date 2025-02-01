using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MappingMenus : MonoBehaviour
{
	[MenuItem("Creature Creator/Connect to Steam", priority = 100)]
	public static void ConnectToSteam()
	{
		EditorSteamManager.ConnectToSteam();
	}

	[MenuItem("Creature Creator/Locate Creature Creator.exe", priority = 101)]
	public static void LocateExe()
	{
		MappingUtils.SetApplicationPath();
	}

	[MenuItem("Creature Creator/Check For Errors _F3", priority = 299)]
	public static void CheckForErrors()
	{
		MappingUtils.CheckForErrors();
	}

	[MenuItem("Creature Creator/Build Map _F4", priority = 300)]
	public static void BuildMap()
	{
		MappingUtils.BuildMap(MapConfig.GetCurrent());
	}

	[MenuItem("Creature Creator/Test Map", priority = 301)]
	public static void TestMap()
	{
		MappingUtils.TestMap(MapConfig.GetCurrent());
	}

	[MenuItem("Creature Creator/Build and Test Map _F5", priority = 302)]
	public static void BuildAndTestMap()
	{
		MappingUtils.BuildAndTestMap(MapConfig.GetCurrent());
	}

    [MenuItem("Creature Creator/Upload Map to Workshop", priority = 303)]
    public static void UploadMap()
    {
        MappingUtils.UploadMap(MapConfig.GetCurrent());
    }

    [MenuItem("Creature Creator/New Map", priority = 50)]
	public static void NewMap()
	{
		MappingUtils.NewMap();
	}
}