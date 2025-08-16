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
        AssetDatabase.RemoveUnusedAssetBundleNames();
        BuildPipeline.BuildAssetBundles(buildPath, GetAssetBuilds(config.bundleName), BuildAssetBundleOptions.DeterministicAssetBundle, buildTarget);
    }

    private static AssetBundleBuild[] GetAssetBuilds(string bundleName)
    {
        List<string> targetNames = new List<string>() {
            bundleName,
            bundleName + "_scene",
        };

        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        foreach (string targetName in targetNames)
        {
            string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(targetName);

            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = targetName;
            build.assetNames = assets;

            builds.Add(build);
        }

        return builds.ToArray();
    }

    public static void AssignBundleNames(ItemConfig config)
    {
        string configPath = config.GetFullDirectory();

        string[] files = Directory.GetFiles(configPath, "*", SearchOption.AllDirectories);

        string excludedDirectory = Path.Combine(configPath, "Exclude");
        if (!Directory.Exists(excludedDirectory))
        {
            Directory.CreateDirectory(excludedDirectory);
            AssetDatabase.Refresh();
        }

        string[] excludedFiles = Directory.GetFiles(excludedDirectory, "*", SearchOption.AllDirectories);

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

            string localFilePath = "Assets" + file.Substring(Application.dataPath.Length);

            var assetImporter = AssetImporter.GetAtPath(localFilePath);

            if (extension == ".unity")
                assetImporter.assetBundleName = config.bundleName + "_scene";
            else
                assetImporter.assetBundleName = config.bundleName;
        }
    }
}
