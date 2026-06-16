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
            // Don't assign to the prefab asset.
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
            {
                return;
            }

            // Regenerate when empty, or when another proxy already uses this id (e.g. after duplicating this object).
            if (string.IsNullOrEmpty(spawnerId) || HasDuplicateId())
            {
                spawnerId = Guid.NewGuid().ToString();
            }
        }
        private bool HasDuplicateId()
        {
            foreach (var other in FindObjectsByType<FoodSpawnerProxy>(FindObjectsSortMode.None))
            {
                if (other != this && other.spawnerId == spawnerId)
                {
                    return true;
                }
            }
            return false;
        }
#endif
        private void OnEnable()
        {
            if (string.IsNullOrEmpty(spawnerId) || Proxies.ContainsKey(spawnerId))
            {
                spawnerId = Guid.NewGuid().ToString();
            }
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
