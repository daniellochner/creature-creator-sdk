using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Collections.Generic;
using DanielLochner.Assets.CreatureCreator;

public class BodyPartConfig : ItemConfig
{
    public override string Singular => "Body Part";
    public override string Plural => "Body Parts";

    public override string GetJSON()
	{
        var config = new BodyPartConfigData
        {
            SDKVersion = ProjectInit.SDKVersion,
            Name = name,
            Description = description,
            Author = author,
        };
        return JsonConvert.SerializeObject(config, Formatting.Indented);
	}

    public static BodyPartConfig GetCurrent()
    {
        var selectedObjects = Selection.objects;
        if (selectedObjects.Length == 1 && selectedObjects[0] is BodyPartConfig config)
        {
            return config;
        }
        return null;
    }
}