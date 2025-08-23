using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ProjectInit
{
	public const string SDKVersion = "1.6.21";
	const string requiredVersion = "6000.1.12f1";

	public static bool CanBuild { get; private set; } = true;

	static ProjectInit()
	{
		Start();
	}

	static void Start()
	{
		SetPlayerSettings();
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

		if (!string.IsNullOrEmpty(version) && version != SDKVersion)
		{
            if (EditorUtility.DisplayDialog("Error", $"The current installed Creature Creator SDK (v{SDKVersion}) is out of date. Please download the new version v{version}!", "New Version"))
            {
                Application.OpenURL("https://github.com/daniellochner/creature-creator-sdk/releases");
            }
        }
    }
}