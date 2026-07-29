using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    [SelectionBase]
    public class WaterProxy : ProxyBehaviour
    {
        public WaterType type;
        public GameObject customSplashPrefab;
        public bool allowSwimming = true;

        public static List<WaterProxy> Proxies { get; private set; } = new ();

        private static readonly Vector3 VISUAL_SCALE = new(10f, 1f, 10f);
        private static readonly Vector3 VISUAL_POSITION = new(0f, 0f, 0f);

        private void OnEnable()
        {
            Proxies.Add(this);
        }
        private void OnDisable()
        {
            Proxies.Remove(this);
        }
        private void OnValidate()
        {
            if (transform.childCount > 0)
            {
                var visual = transform.GetChild(0);
                visual.localScale = VISUAL_SCALE;
                visual.localPosition = VISUAL_POSITION;
            }
        }

        public override bool IsValid(out string error)
        {
            if ((int)type < (int)WaterType.Empty || (int)type > (int)WaterType.Lake)
            {
                error = "Water type has an unsupported value.";
                return false;
            }

            return base.IsValid(out error);
        }

        public enum WaterType
        {
            Empty,
            Ocean,
            Lake
        }
    }
}
