using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class PlatformProxy : ProxyBehaviour
    {
        private Transform Model => transform.GetChild(0);

        private void Update()
        {
            Model.localPosition = Vector3.zero;
        }

        public static List<PlatformProxy> Proxies { get; private set; } = new ();

        private void OnEnable()
        {
            Proxies.Add(this);
        }
        private void OnDisable()
        {
            Proxies.Remove(this);
        }
    }
}
