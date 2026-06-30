using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

[InitializeOnLoad]
public static class ProjectInit
{
	public const string SDKVersion = "1.8.5";
	const string requiredVersion = "6000.1.17f1";

	// Marker type from the "com.unity.ai.navigation" package.
	const string NavigationDefine = "UNITY_NAVIGATION";
	const string NavigationMarkerType = "Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation";

	public static bool CanBuild { get; private set; } = true;

	static ProjectInit()
	{
		Start();
	}

	static void Start()
	{
		SetPlayerSettings();
		UpdateScriptingDefines();
		CheckEditorVersion();
		CheckSDKVersion();
	}

	static void SetPlayerSettings()
	{
		if (PlayerSettings.colorSpace != ColorSpace.Linear)
		{
			Debug.Log("Setting color space to Linear.");
			PlayerSettings.colorSpace = ColorSpace.Linear;
		}
	}

	static void UpdateScriptingDefines()
	{
		// Detect by type so this editor script compiles whether or not the package is installed.
		bool navigationInstalled = Type.GetType(NavigationMarkerType) != null;
		SetScriptingDefine(NavigationDefine, navigationInstalled);
	}

	static void SetScriptingDefine(string define, bool enabled)
	{
		var buildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
		PlayerSettings.GetScriptingDefineSymbols(buildTarget, out string[] current);

		var defines = new List<string>(current);
		bool present = defines.Contains(define);
		if (enabled == present)
		{
			return; // Already in the desired state; avoid triggering a recompile loop.
		}

		if (enabled)
		{
			defines.Add(define);
			Debug.Log($"Adding scripting define '{define}'.");
		}
		else
		{
			defines.Remove(define);
			Debug.Log($"Removing scripting define '{define}'.");
		}

		PlayerSettings.SetScriptingDefineSymbols(buildTarget, defines.ToArray());
	}

	static void CheckEditorVersion()
	{
		string version = Application.unityVersion;

		if (version != requiredVersion)
		{
			string error = $"Invalid version! You need Unity {requiredVersion} but are running Unity {version}.";
			EditorUtility.DisplayDialog("Invalid Version", error, "OK");
			Debug.LogError(error);
		}
	}

	static async void CheckSDKVersion()
	{
        var version = await GitHubVersionUtility.GetLatestReleaseAsync("daniellochner", "creature-creator-sdk");

		if (!string.IsNullOrEmpty(version) && IsVersionOutOfDate(SDKVersion, version))
		{
            if (EditorUtility.DisplayDialog("Error", $"The current installed Creature Creator SDK (v{SDKVersion}) is out of date. Please download the new version v{version}!", "New Version"))
            {
                Application.OpenURL("https://github.com/daniellochner/creature-creator-sdk/releases");
            }
        }
    }

	static bool IsVersionOutOfDate(string installedVersion, string latestVersion)
	{
		return TryParseVersion(installedVersion, out var installed) &&
			TryParseVersion(latestVersion, out var latest) &&
			latest.CompareTo(installed) > 0;
	}

	static bool TryParseVersion(string version, out Version parsedVersion)
	{
		return Version.TryParse(version.TrimStart('v', 'V'), out parsedVersion);
	}
}
