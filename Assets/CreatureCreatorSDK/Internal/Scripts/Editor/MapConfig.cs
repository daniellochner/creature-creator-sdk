using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using DanielLochner.CreatureCrafter.SDK;

public class MapConfig : ItemConfig
{
    [Header("Map")]
    public List<LinkedItem> linkedBodyParts;
    public List<LinkedItem> linkedPatterns;

    [HideInInspector] public List<string> bodyPartIds;
    [HideInInspector] public List<string> patternIds;

    public override string Singular => "Map";
    public override string Plural => "Maps";

    private void OnEnable()
    {
        EnsureInitialized();
    }

    public void EnsureInitialized()
    {
        if (linkedBodyParts == null)
        {
            linkedBodyParts = new List<LinkedItem>();
        }
        if (linkedPatterns == null)
        {
            linkedPatterns = new List<LinkedItem>();
        }
        if (bodyPartIds == null)
        {
            bodyPartIds = new List<string>();
        }
        if (patternIds == null)
        {
            patternIds = new List<string>();
        }
    }

	public override string GetJSON()
	{
        EnsureInitialized();

        var config = new MapConfigData
        {
            SDKVersion = ProjectInit.SDKVersion,
            BundleName = bundleName,
            Name = name,
            Description = description,
            Author = author,
            BodyPartIds = bodyPartIds,
            PatternIds = patternIds
        };
        return JsonConvert.SerializeObject(config, Formatting.Indented);
	}

    public static MapConfig GetSelected()
    {
        var selectedObjects = Selection.objects;
        if (selectedObjects.Length == 1 && selectedObjects[0] is MapConfig config)
        {
            return config;
        }
        return null;
    }
    public static MapConfig GetCurrent()
    {
        string scenePath = SceneManager.GetActiveScene().path;
        if (string.IsNullOrEmpty(scenePath))
        {
            return null;
        }

        int lastIndex = scenePath.LastIndexOf('/');
        string sceneFolder = scenePath.Substring(0, lastIndex);

        string configPath = sceneFolder + "/config.asset";

        return AssetDatabase.LoadAssetAtPath<MapConfig>(configPath);
    }

    [Serializable]
    public class LinkedItem
    {
        public string itemId;
        public ItemConfig config;
    }
}
