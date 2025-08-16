using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Collections.Generic;
using DanielLochner.Assets.CreatureCreator;

[CreateAssetMenu(menuName = "Creature Creator/Body Part Config", fileName = "config")]
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
}