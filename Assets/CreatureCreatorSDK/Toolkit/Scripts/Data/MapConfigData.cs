using System.Collections.Generic;
using System;

namespace DanielLochner.CreatureCrafter.SDK
{
    [Serializable]
    public class MapConfigData : ItemConfigData
    {
        public List<string> BodyPartIds;
        public List<string> PatternIds;

        public override string Singular => "Map";
    }
}
