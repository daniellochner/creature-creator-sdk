using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class ModdingUtils
{
    public static void StartGame(string applicationPath, string mapPath, string arg)
    {
        Process process = new Process();
        process.StartInfo.FileName = applicationPath;
        process.StartInfo.Arguments = $"-{arg} \"{mapPath}\"";
        process.Start();
        Debug.Log("Starting game with arguments: " + process.StartInfo.Arguments);
    }

    public static bool TryCreateNewItem<T>(out string itemName, out string itemPath) where T : ItemConfig
    {
        T config = ScriptableObject.CreateInstance<T>();

        itemName = EditorInputDialog.Show($"New {config.Singular}", $"Create a new {config.Singular}", $"{config.Singular} Name");
        itemPath = Path.Combine(Application.dataPath, "Items", config.Plural, itemName);

        if (Directory.Exists(itemPath))
        {
            ThrowError($"'{itemPath}' already exists!");
            return false;
        }
        Directory.CreateDirectory(itemPath);

        string configPath = Path.Combine(ConvertGlobalPathToLocalPath(itemPath), "config.asset");
        config.bundleName = itemName.ToLower().Replace(' ', '_');
        config.name = itemName;
        AssetDatabase.CreateAsset(config, configPath);

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<MapConfig>(configPath);

        return true;
    }
    public static bool TryBuildItem<T1, T2>(T1 config, bool buildAll, Action<string> onSetup) where T1 : ItemConfig where T2 : ItemConfigData
    {
        if (buildAll)
        {
            if (!BuildPipeline.IsBuildTargetSupported(default, BuildTarget.StandaloneWindows64) || !BuildPipeline.IsBuildTargetSupported(default, BuildTarget.StandaloneOSX) || !BuildPipeline.IsBuildTargetSupported(default, BuildTarget.StandaloneLinux64) || !BuildPipeline.IsBuildTargetSupported(default, BuildTarget.Android) || !BuildPipeline.IsBuildTargetSupported(default, BuildTarget.iOS))
            {
                ThrowError($"Please ensure the following build targets are supported by installing them through Unity Hub: {BuildTarget.StandaloneWindows64}, {BuildTarget.StandaloneOSX}, {BuildTarget.StandaloneLinux64}, {BuildTarget.Android} and {BuildTarget.iOS}.");
                return false;
            }
        }

        var startTime = DateTime.Now;
        Debug.Log("Build started.");

        // Load previous config
        string buildPath = GetBuildPath(config);
        string configPath = Path.Combine(buildPath, "config.json");
        T2 prevConfigData = null;
        if (File.Exists(configPath))
        {
            prevConfigData = JsonConvert.DeserializeObject<T2>(File.ReadAllText(configPath));
        }

        // Delete existing build if it exists
        if (Directory.Exists(buildPath))
        {
            Directory.Delete(buildPath, true);
        }
        Directory.CreateDirectory(buildPath);

        // Setup
        onSetup?.Invoke(buildPath);

        // Build asset bundles
        config.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        BuildBundlesForPlatform(config, RuntimePlatform.WindowsPlayer);
        if (buildAll)
        {
            BuildBundlesForPlatform(config, RuntimePlatform.OSXPlayer);
            BuildBundlesForPlatform(config, RuntimePlatform.LinuxPlayer);
            BuildBundlesForPlatform(config, RuntimePlatform.IPhonePlayer);
            BuildBundlesForPlatform(config, RuntimePlatform.Android);
        }
        config.hideFlags &= ~HideFlags.DontUnloadUnusedAsset;

        // Generate new config using previous ItemId
        string nextDataJson = config.GetJSON();
        T2 nextData = JsonConvert.DeserializeObject<T2>(nextDataJson);
        if (prevConfigData != null)
        {
            nextData.ItemId = prevConfigData.ItemId;
        }
        File.WriteAllText(configPath, JsonConvert.SerializeObject(nextData, Formatting.Indented));

        // Log
        Debug.Log($"Build completed in {DateTime.Now.Subtract(startTime).TotalSeconds.ToString("0")} seconds: {buildPath}");

        return true;
    }

    public static void BuildBundlesForPlatform(ItemConfig config, RuntimePlatform platform)
    {
        string bundleBuildPath = GetBundleBuildPath(config) + $"_{platform}";

        Directory.CreateDirectory(bundleBuildPath);

        AssetBundleBuilder.AssignBundleNames(config);

        BuildTarget buildTarget = default;
        switch (platform)
        {
            case RuntimePlatform.WindowsPlayer:
                buildTarget = BuildTarget.StandaloneWindows64;
                break;

            case RuntimePlatform.OSXPlayer:
                buildTarget = BuildTarget.StandaloneOSX;
                break;

            case RuntimePlatform.LinuxPlayer:
                buildTarget = BuildTarget.StandaloneLinux64;
                break;

            case RuntimePlatform.IPhonePlayer:
                buildTarget = BuildTarget.iOS;
                break;

            case RuntimePlatform.Android:
                buildTarget = BuildTarget.Android;
                break;
        }

        AssetBundleBuilder.BuildAssetBundles(config, bundleBuildPath, buildTarget);

        foreach (var file in new DirectoryInfo(bundleBuildPath).GetFiles("*.manifest"))
        {
            File.Delete(file.FullName);
        }
    }
    public static string GetBuildPath(ItemConfig config)
    {
        return Path.Combine(Application.dataPath, "..", "Items", config.Plural, config.name);
    }
    public static string GetBundleBuildPath(ItemConfig config)
    {
        return Path.Combine(GetBuildPath(config), config.Singular, "Bundles");
    }

    public static string SetApplicationPath()
    {
        string path = EditorUtility.OpenFilePanel("Find Creature Creator.exe", EditorSteamManager.GetInstallFolder(), "exe");

        if (string.IsNullOrEmpty(path))
            return null;

        Debug.Log("Set Creature Creator.exe path to " + path);

        PlayerPrefs.SetString(Constants.PlayerPrefsApplicationPathKey, path);
        PlayerPrefs.Save();
        return path;
    }
    public static string GetApplicationPath()
    {
        string applicationPath;

        if (PlayerPrefs.HasKey(Constants.PlayerPrefsApplicationPathKey))
        {
            applicationPath = PlayerPrefs.GetString(Constants.PlayerPrefsApplicationPathKey);
        }
        else
        {
            applicationPath = EditorSteamManager.GetInstallLocation();
        }

        if (!File.Exists(applicationPath))
        {
            Debug.LogError("The application was not found at the specified path: " + applicationPath);
            applicationPath = SetApplicationPath();
        }

        if (!File.Exists(applicationPath))
        {
            ThrowError("The application was not found at the specified path: " + applicationPath);
        }

        return applicationPath;
    }

    public static string ConvertLocalPathToGlobalPath(string localPath)
    {
        return Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length), localPath);
    }
    public static string ConvertGlobalPathToLocalPath(string globalPath)
    {
        return Path.Combine(globalPath.Substring(Application.dataPath.Length - "Assets".Length));
    }

    public static long GetDataSize(string dataPath)
    {
        if (File.Exists(dataPath))
        {
            FileInfo file = new FileInfo(dataPath);
            return file.Length;
        }

        if (Directory.Exists(dataPath))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dataPath);

            long size = 0;

            // Add file sizes.
            IEnumerable<FileInfo> files = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                size += file.Length;
            }

            return size;
        }

        return 0;
    }
    public static bool CheckFileSize(string dataPath, out double fileSizeMB, out double maxFileSizeMB)
    {
        var fileSize = GetDataSize(dataPath);
        var maxFileSize = 100000000; // 100MB

        fileSizeMB = Math.Round(fileSize / 1000000f, 2);
        maxFileSizeMB = Math.Round(maxFileSize / 1000000f, 2);

        return fileSizeMB > maxFileSizeMB;
    }
    public static void ThrowError(string error)
    {
        EditorUtility.DisplayDialog("Error", error, "OK");
        throw new Exception(error);
    }
}
