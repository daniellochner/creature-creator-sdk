using System;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class FoodSpawnerProxy : ProxyBehaviour
    {
        public FoodProxy food;
        public Vector2 spawnCooldown = new Vector2(120, 180);
        [HideInInspector] public string spawnerId;

        public static Dictionary<string, FoodSpawnerProxy> Proxies { get; private set; } = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(spawnerId))
            {
                spawnerId = Guid.NewGuid().ToString();
            }
        }
#endif
        private void OnEnable()
        {
            Proxies.Add(spawnerId, this);
        }
        private void OnDisable()
        {
            Proxies.Remove(spawnerId);
        }

        public override bool IsValid()
        {
            if (food == null)
            {
                Debug.LogError("A food proxy must be assigned.");
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
