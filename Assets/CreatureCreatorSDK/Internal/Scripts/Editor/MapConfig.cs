using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Collections.Generic;

public class MapConfig : ItemConfig
{
    public List<string> bodyPartIds;
    public List<string> patternIds;

    public override string Singular => "Map";
    public override string Plural => "Maps";

	public override string GetJSON()
	{
        var config = new MapConfigData
        {
            SDKVersion = ProjectInit.SDKVersion,
            Name = name,
            Description = description,
            Author = author,
            BodyPartIds = bodyPartIds,
            PatternIds = patternIds
        };
        return JsonConvert.SerializeObject(config, Formatting.Indented);
	}

    public static MapConfig GetCurrent()
    {
        string scenePath = SceneManager.GetActiveScene().path;

        int lastIndex = scenePath.LastIndexOf('/');
        string sceneFolder = scenePath.Substring(0, lastIndex);

        string configPath = sceneFolder + "/config.asset";

        var config = AssetDatabase.LoadAssetAtPath<MapConfig>(configPath);

        if (config == null)
        {
            throw new System.Exception($"Missing config file at {configPath}");
        }

        return config;
    }
}