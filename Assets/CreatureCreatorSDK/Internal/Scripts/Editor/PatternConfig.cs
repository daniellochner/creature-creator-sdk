using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(menuName = "Creature Creator/Pattern Config", fileName = "config")]
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
}
