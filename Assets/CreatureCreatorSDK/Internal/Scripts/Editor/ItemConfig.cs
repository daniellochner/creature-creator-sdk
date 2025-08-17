using UnityEditor;
using UnityEngine;

public abstract class ItemConfig : ScriptableObject
{
    public new string name;
    public string author;
    [TextArea] public string description;
    [HideInInspector] public string bundleName;
    public Texture2D thumbnail;

    public abstract string Singular { get; }
    public abstract string Plural { get; }

    public abstract string GetJSON();

    public string GetDirectory()
    {
        string path = AssetDatabase.GetAssetPath(this);
        path = path.Substring(0, path.Length - "config.asset".Length);
        return path;
    }
    public string GetFullDirectory()
    {
        return ModdingUtils.ConvertLocalPathToGlobalPath(GetDirectory());
    }
}