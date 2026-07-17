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
    [Range(1, 10)] public int complexity;
    [Range(1, 25)] public int health;
    [Range(0.1f, 25f)] public float weight;
    [Range(-0.1f, 0.5f)] public float speed;
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