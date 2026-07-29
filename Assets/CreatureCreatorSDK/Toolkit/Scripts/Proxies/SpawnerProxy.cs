using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DanielLochner.CreatureCrafter.SDK
{
    [ProxyCountLimit(ProxyValidationLimits.MaxSpawners)]
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

        public override bool IsValid(out string error)
        {
            if (model == null)
            {
                error = "A model must be assigned.";
                return false;
            }

            if (!IsFinite(spawnCooldown)
                || spawnCooldown.x < 10f
                || spawnCooldown.y < spawnCooldown.x)
            {
                error = "Spawn cooldown values must be finite, ordered from minimum to maximum, and at least 10 seconds.";
                return false;
            }

            return base.IsValid(out error);
        }
    }
}
