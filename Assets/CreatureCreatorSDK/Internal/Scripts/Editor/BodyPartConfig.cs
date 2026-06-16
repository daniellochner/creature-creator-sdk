using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using DanielLochner.CreatureCrafter.SDK;
using static DanielLochner.CreatureCrafter.SDK.BodyPartConfigData;

public class BodyPartConfig : ItemConfig
{
    [Header("Body Part")]
    public SaveType type;
    public DietType diet;
    public int complexity;
    public int health;
    public float weight;
    public float speed;
    public List<AbilityType> abilities;

    public override string Singular => "Body Part";
    public override string Plural => "Body Parts";

    public override string GetJSON()
	{
        var config = new BodyPartConfigData
        {
            SDKVersion = ProjectInit.SDKVersion,
            BundleName = bundleName,
            Name = name,
            Description = description,
            Author = author,
            Type = type,
            Complexity = complexity,
            Health = health,
            Weight = weight,
            Speed = speed,
            Abilities = abilities,
            Diet = diet
        };
        return JsonConvert.SerializeObject(config, Formatting.Indented);
	}

    public static BodyPartConfig GetSelected()
    {
        var selectedObjects = UnityEditor.Selection.objects;
        if (selectedObjects.Length == 1 && selectedObjects[0] is BodyPartConfig config)
        {
            return config;
        }
        return null;
    }
}