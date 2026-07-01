using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class SetLayer : MonoBehaviour
    {
        public bool includeChildren;
        public LayerType layerType;
        [HideInInspector] public string layerName;

        public enum LayerType
        {
            Default,
            Ground,
            UI,
            PostProcessing
        }
    }
}
