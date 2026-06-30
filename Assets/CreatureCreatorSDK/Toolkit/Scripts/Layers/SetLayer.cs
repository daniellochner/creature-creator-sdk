using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class SetLayer : ErrorCollectionBehaviour
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

        public override void RunErrorChecks(ref List<string> errors)
        {
            base.RunErrorChecks(ref errors);

#if UNITY_NAVIGATION
            if (layerType == LayerType.Ground && GetComponentInChildren<Unity.AI.Navigation.NavMeshSurface>() == null)
            {
                errors.Add("Ground layers should probably have a NavMeshSurface component attached and baked.");
            }
#endif
        }
    }
}
