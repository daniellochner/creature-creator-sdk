using System;
using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    [Serializable]
    public abstract class ItemConfigData
    {
        public string SDKVersion;
        public string ItemId;
        public string BundleName;

        public string Name;
        [TextArea]
        public string Description;
        public string Author;

        public abstract string Singular { get; }
    }
}
