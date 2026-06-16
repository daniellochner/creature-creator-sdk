using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class UnlockablePatternProxy : UnlockableItemProxy
    {
        public static List<UnlockablePatternProxy> Proxies { get; private set; } = new();

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
