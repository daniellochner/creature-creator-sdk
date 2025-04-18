using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Creature Creator/Map Config", fileName = "config")]
public class MapConfig : ScriptableObject
{
	public new string name;
    public string author;
    [TextArea] public string description;
    public Texture2D thumbnail;
    public List<string> bodyPartIds;
    public List<string> patternIds;
    [HideInInspector] public string bundleName;

    public string GetMapDirectory()
	{
		string path = AssetDatabase.GetAssetPath(this);
		path = path.Substring(0, path.Length - "config.asset".Length);
		return path;
	}

	public string GetFullMapDirectory()
	{
		return MappingUtils.ConvertLocalPathToGlobalPath(GetMapDirectory());
	}

	public static MapConfig GetCurrent()
	{
		string scenePath = SceneManager.GetActiveScene().path;

		int lastIndex = scenePath.LastIndexOf('/');
		string sceneFolder = scenePath.Substring(0, lastIndex);

		string configPath = sceneFolder + "/config.asset";

		var config = AssetDatabase.LoadAssetAtPath<MapConfig>(configPath);

		if(config == null)
		{
			throw new System.Exception($"Missing config file at {configPath}");
		}

		return config;
	}

	public string GetJSON()
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
}