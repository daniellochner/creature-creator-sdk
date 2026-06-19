using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class OutOfBoundsProxy : ProxyBehaviour
    {
        public static List<OutOfBoundsProxy> Proxies { get; private set; } = new ();

        private void OnEnable()
        {
            Proxies.Add(this);
        }
        private void OnDisable()
        {
            Proxies.Remove(this);
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
