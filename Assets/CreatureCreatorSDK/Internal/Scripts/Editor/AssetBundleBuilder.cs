using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AssetBundleBuilder
{
	public static void BuildAssetBundles(ItemConfig config, string buildPath, BuildTarget buildTarget)
	{
        BuildPipeline.BuildAssetBundles(buildPath, GetAssetBuilds(config.bundleName), BuildAssetBundleOptions.None, buildTarget);
    }

    private static AssetBundleBuild[] GetAssetBuilds(string bundleName)
    {
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

        string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
        if (assets.Length > 0)
        {
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = bundleName;
            build.assetNames = assets;

            builds.Add(build);
        }

        return builds.ToArray();
    }

    public static void AssignBundleNames(ItemConfig config)
    {
        ClearAllAssetBundleNames();

        var configPath = config.GetFullDirectory();

        var files = Directory.GetFiles(configPath, "*", SearchOption.AllDirectories);

        var excludedDirectory = Path.Combine(configPath, "Exclude");
        var excludedFiles = new string[0];
        if (Directory.Exists(excludedDirectory))
        {
            excludedFiles = Directory.GetFiles(excludedDirectory, "*", SearchOption.AllDirectories);
        }

        foreach (string file in files)
        {
            if (excludedFiles.Contains(file))
                continue;

            if (file.EndsWith(".meta"))
                continue;

            string extension = Path.GetExtension(file);
            string fileName = Path.GetFileNameWithoutExtension(file) + extension;
            if (fileName == "config.asset")
                continue;

            if (extension == ".unity")
                continue;

            string localFilePath = ModdingUtils.ConvertGlobalPathToLocalPath(file);

            var assetImporter = AssetImporter.GetAtPath(localFilePath);
            if (assetImporter == null)
                continue;

            assetImporter.assetBundleName = config.bundleName;
        }
    }

    public static void ClearAllAssetBundleNames()
    {
        string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();

        foreach (string name in bundleNames)
        {
            AssetDatabase.RemoveAssetBundleName(name, true);
        }
    }
}
