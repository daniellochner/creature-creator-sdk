using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AssetBundleBuilder
{
#if !REDMATCH
	public static void BuildAssetBundles(MapConfig config, string buildPath)
	{
		AssetDatabase.RemoveUnusedAssetBundleNames();

		BuildPipeline.BuildAssetBundles(buildPath, GetAssetBuilds(config.bundleName), BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.StandaloneWindows64);
	}

	static AssetBundleBuild[] GetAssetBuilds(string bundleName)
	{
		List<string> targetNames = new List<string>() {
			bundleName,
			bundleName + "_scene",
		};

		List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
		foreach(string targetName in targetNames)
		{
			string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(targetName);

			AssetBundleBuild build = new AssetBundleBuild();
			build.assetBundleName = targetName;
			build.assetNames = assets;

			builds.Add(build);
		}

		return builds.ToArray();
	}

	public static void AssignBundleNames(MapConfig config)
	{
		string[] files = Directory.GetFiles(config.GetFullMapDirectory(), "*", SearchOption.AllDirectories);

		string excludedDirectory = Path.Combine(config.GetFullMapDirectory(), "Exclude");
		if(!Directory.Exists(excludedDirectory))
		{
			Directory.CreateDirectory(excludedDirectory);
			AssetDatabase.Refresh();
		}

		string[] excludedFiles = Directory.GetFiles(excludedDirectory, "*", SearchOption.AllDirectories);

		foreach(string file in files)
		{
			if(excludedFiles.Contains(file))
				continue;

			if(file.EndsWith(".meta"))
				continue;

			string extension = Path.GetExtension(file);
			string fileName = Path.GetFileNameWithoutExtension(file) + extension;
			if(fileName == "config.asset")
				continue;

			string localFilePath = "Assets" + file.Substring(Application.dataPath.Length);

			var assetImporter = AssetImporter.GetAtPath(localFilePath);

			if(extension == ".unity")
				assetImporter.assetBundleName = config.bundleName + "_scene";
			else
				assetImporter.assetBundleName = config.bundleName;
		}
	}
#endif
}
