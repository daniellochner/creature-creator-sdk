using System.IO;
using UnityEditor;
using UnityEngine;

public abstract class ItemConfig : ScriptableObject
{
    [Header("Item")]
    public new string name;
    public string author;
    [TextArea] public string description;
    [HideInInspector] public string bundleName;
    [HideInInspector] public Texture2D thumbnail;

    public abstract string Singular { get; }
    public abstract string Plural { get; }

    public abstract string GetJSON();

    public string GetDirectory()
    {
        string path = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(path))
        {
            ModdingUtils.ThrowError($"The {Singular.ToLower()} '{name}' is not a saved asset. Make sure every linked item points to a valid config file on disk.");
            return null;
        }
        return path.Substring(0, path.Length - Path.GetFileName(path).Length);
    }
    public string GetDirectoryName()
    {
        return Path.GetFileName(Path.GetDirectoryName(GetFullDirectory()));
    }
    public string GetFullDirectory()
    {
        return ModdingUtils.ConvertLocalPathToGlobalPath(GetDirectory());
    }
}