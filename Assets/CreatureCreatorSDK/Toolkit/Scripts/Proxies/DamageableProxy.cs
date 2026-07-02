using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class DamageableProxy : ProxyBehaviour
    {
        [Range(0, 100)] public float damageAmount = 10f;
        public float thresholdSpeed = 3f;

        public override bool IsValid()
        {
            if (damageAmount < 0 || damageAmount > 100)
            {
                Debug.LogError("Damage amount must be in the range [0, 100].");
                return false;
            }

            if (thresholdSpeed < 0)
            {
                Debug.LogError("Threshold speed must be a positive value");
                return false;
            }

            return base.IsValid();
        }
    }
}
