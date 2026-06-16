using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class UnlockableBodyPartProxy : UnlockableItemProxy
    {
        public static List<UnlockableBodyPartProxy> Proxies { get; private set; } = new();

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
