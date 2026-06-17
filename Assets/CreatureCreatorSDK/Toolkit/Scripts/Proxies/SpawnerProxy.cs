using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class SpawnerProxy : ProxyBehaviour
    {
        public CustomObjectProxy model;
        public Vector2 spawnCooldown = new Vector2(120, 180);

        public static List<SpawnerProxy> Proxies { get; private set; } = new();

        private void OnEnable()
        {
            Proxies.Add(this);
        }
        private void OnDisable()
        {
            Proxies.Remove(this);
        }

        public override bool IsValid()
        {
            if (model == null)
            {
                Debug.LogError("A model must be assigned.");
                return false;
            }

            if (spawnCooldown.x < 10)
            {
                Debug.LogError("Spawn cooldown must be in the range [10, Infinity).", gameObject);
                return false;
            }

            return base.IsValid();
        }
    }
}
