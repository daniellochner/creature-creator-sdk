using Newtonsoft.Json;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[InitializeOnLoad]
public static class ProjectInit
{
	public const string SDKVersion = "1.6.19";
	const string requiredVersion = "6000.1.12f1";
	const string configURL = "https://playcreature.com/sdk/config.json";

	public static bool CanBuild { get; private set; } = true;

	static bool requestedConfig;
	static bool configDownloadCompleted;
	static UnityWebRequest configRequest;

	static ProjectInit()
	{
		Start();
	}

	static void Start()
	{
		SetPlayerSettings();
		CheckEditorVersion();
		DownloadConfig();
	}

	static void SetPlayerSettings()
	{
		if(PlayerSettings.colorSpace != ColorSpace.Linear)
		{
			Debug.Log("Setting color space to Linear.");
			PlayerSettings.colorSpace = ColorSpace.Linear;
		}
	}

	static void CheckEditorVersion()
	{
		string version = Application.unityVersion;

		if(version != requiredVersion)
		{
			string error = $"Invalid version! You need Unity {requiredVersion} but are running Unity {version}.";
			EditorUtility.DisplayDialog("Invalid Version", error, "OK");
			Debug.LogError(error);
		}
	}

	static void DownloadConfig()
	{
		if(requestedConfig)
		{
			return;
		}

		configRequest = UnityWebRequest.Get(configURL);
		configRequest.SendWebRequest();
		EditorApplication.update += Update_DownloadConfigProcess;

		requestedConfig = true;
	}

	static void Update_DownloadConfigProcess()
	{
		if(!configRequest.isDone || configDownloadCompleted)
		{
			return;
		}

		configDownloadCompleted = true;
		EditorApplication.update -= Update_DownloadConfigProcess;

		if(configRequest.isNetworkError || configRequest.isHttpError)
		{
			Debug.LogError($"Error getting config information: {configRequest.error}");
			return;
		}

		var config = JsonConvert.DeserializeObject<SDKConfig>(configRequest.downloadHandler.text);

		// Major version change
		if(config.currentVersion != SDKVersion)
		{
			CanBuild = false;
			if(EditorUtility.DisplayDialog("Error", $"The installed Creature Creator SDK version (v{SDKVersion}) is out of date. Download the new version v{config.currentVersion}.", "New Version"))
			{
				Application.OpenURL(config.downloadPage);
			}
			return;
		}
	}

	[Serializable]
	class SDKConfig
	{
		public string currentVersion;
		public string downloadPage;
	}
}