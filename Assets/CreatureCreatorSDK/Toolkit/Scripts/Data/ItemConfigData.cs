using System;
using UnityEngine;

[Serializable]
public class ItemConfigData
{
    public string SDKVersion;
    public string ItemId;

    public string Name;
    [TextArea]
    public string Description;
    public string Author;
}
