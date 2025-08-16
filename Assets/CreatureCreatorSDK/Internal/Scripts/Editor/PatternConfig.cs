using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class PatternConfig : ItemConfig
{
    public override string Singular => "Pattern";
    public override string Plural => "Patterns";

    public override string GetJSON()
    {
        var config = new PatternConfigData
        {
            SDKVersion = ProjectInit.SDKVersion,
            Name = name,
            Description = description,
            Author = author,
        };
        return JsonConvert.SerializeObject(config, Formatting.Indented);
    }

    public static PatternConfig GetCurrent()
    {
        var selectedObjects = Selection.objects;
        if (selectedObjects.Length == 1 && selectedObjects[0] is PatternConfig config)
        {
            return config;
        }
        return null;
    }
}
