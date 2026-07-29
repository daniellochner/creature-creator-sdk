using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class DamageableProxy : ProxyBehaviour
    {
        [Range(0, 100)] public float damageAmount = 10f;
        public float thresholdSpeed = 3f;

        public override bool IsValid(out string error)
        {
            if (!IsFinite(damageAmount) || damageAmount < 0f || damageAmount > 100f)
            {
                error = "Damage amount must be a finite value in the range [0, 100].";
                return false;
            }

            if (!IsFinite(thresholdSpeed) || thresholdSpeed < 0f)
            {
                error = "Threshold speed must be a finite non-negative value.";
                return false;
            }

            return base.IsValid(out error);
        }
    }
}
