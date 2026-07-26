using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class ProjectInit
{
	public const string SDKVersion = "1.8.9";
	const string requiredVersion = "6000.1.17f1";

	const string NavigationDefine = "UNITY_NAVIGATION";
	const string NavigationMarkerType = "Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation";
	const string NavigationPackageSpec = "com.unity.ai.navigation@2.0.8";
	const string NavigationInstallSessionKey = "CreatureCreatorSDK.NavigationInstallRequested";

	static AddRequest navigationInstallRequest;

	public static bool CanBuild { get; private set; } = true;

	static ProjectInit()
	{
		Start();
	}

	static void Start()
	{
		CheckEditorVersion();
		CheckSDKVersion();
		SetPlayerSettings();
		EnsureBuiltInRenderPipeline();

		if (EnsureNavigationPackageInstalled())
		{
			UpdateScriptingDefines();
		}
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
		bool navigationInstalled = Type.GetType(NavigationMarkerType) != null;
		SetScriptingDefine(NavigationDefine, navigationInstalled);
	}

	static void EnsureBuiltInRenderPipeline()
	{
		bool cleared = false;

		if (GraphicsSettings.defaultRenderPipeline != null)
		{
			Debug.Log("Clearing the default render pipeline asset.");
			GraphicsSettings.defaultRenderPipeline = null;
			cleared = true;
		}

		int activeQualityLevel = QualitySettings.GetQualityLevel();
		bool switchedQualityLevel = false;

		for (int i = 0; i < QualitySettings.names.Length; i++)
		{
			if (QualitySettings.GetRenderPipelineAssetAt(i) == null)
			{
				continue;
			}

			Debug.Log($"Clearing the render pipeline asset on quality level '{QualitySettings.names[i]}'.");
			QualitySettings.SetQualityLevel(i, false);
			QualitySettings.renderPipeline = null;
			switchedQualityLevel = true;
			cleared = true;
		}

		if (switchedQualityLevel)
		{
			QualitySettings.SetQualityLevel(activeQualityLevel, false);
		}

		if (cleared)
		{
			EditorApplication.delayCall += AssetDatabase.SaveAssets;
			Debug.Log("Converted the project to the built-in render pipeline. Materials that used render pipeline shaders need to be reassigned to built-in ones.");
		}
	}

	static bool EnsureNavigationPackageInstalled()
	{
		if (Type.GetType(NavigationMarkerType) != null)
		{
			return true;
		}
		if (navigationInstallRequest != null ||
			SessionState.GetBool(NavigationInstallSessionKey, false))
		{
			return false;
		}

		Debug.Log($"Creature Creator SDK requires {NavigationPackageSpec}. Installing through Unity Package Manager...");
		SessionState.SetBool(NavigationInstallSessionKey, true);
		navigationInstallRequest = Client.Add(NavigationPackageSpec);
		EditorApplication.update += PollNavigationPackageInstall;
		return false;
	}

	static void PollNavigationPackageInstall()
	{
		if (navigationInstallRequest == null || !navigationInstallRequest.IsCompleted)
		{
			return;
		}

		EditorApplication.update -= PollNavigationPackageInstall;

		bool installed = navigationInstallRequest.Status == StatusCode.Success;
		if (installed)
		{
			Debug.Log($"Installed required package {NavigationPackageSpec}.");
		}
		else
		{
			Debug.LogError($"Failed to install required package {NavigationPackageSpec}: {navigationInstallRequest.Error.message}");
		}

		navigationInstallRequest = null;
		SessionState.SetBool(NavigationInstallSessionKey, false);

		if (!installed)
		{
			UpdateScriptingDefines();
		}
	}

	static void SetScriptingDefine(string define, bool enabled)
	{
		var buildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
		PlayerSettings.GetScriptingDefineSymbols(buildTarget, out string[] current);

		var defines = new List<string>(current);
		bool present = defines.Contains(define);
		if (enabled == present)
		{
			return;
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
