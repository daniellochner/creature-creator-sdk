using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class WaterProxy : ProxyBehaviour
    {
        public bool allowSwimming = true;
        public WaterType type;
        public GameObject customSplashPrefab;

        public static List<WaterProxy> Proxies { get; private set; } = new ();

        private void OnEnable()
        {
            Proxies.Add(this);
        }
        private void OnDisable()
        {
            Proxies.Remove(this);
        }

        public enum WaterType
        {
            Empty,
            Ocean,
            Lake
        }
    }
}
