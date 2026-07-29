using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class EdibleProxy : ProxyBehaviour
    {
        public Diet diet = Diet.Omnivore;
        public Vector2 minMaxHunger = new Vector2(0.25f, 0.5f);
        public Vector2 minMaxHeal = new Vector2(15f, 20f);

        public override bool IsValid(out string error)
        {
            if ((int)diet < (int)Diet.Omnivore || (int)diet > (int)Diet.Herbivore)
            {
                error = "Diet has an unsupported value.";
                return false;
            }

            if (!IsFinite(minMaxHunger) || minMaxHunger.x < 0f || minMaxHunger.y > 1f || minMaxHunger.x > minMaxHunger.y)
            {
                error = "Hunger values must be finite, ordered from minimum to maximum, and in the range [0, 1].";
                return false;
            }

            if (!IsFinite(minMaxHeal) || minMaxHeal.x < 0f || minMaxHeal.y > 100f || minMaxHeal.x > minMaxHeal.y)
            {
                error = "Heal values must be finite, ordered from minimum to maximum, and in the range [0, 100].";
                return false;
            }

            return base.IsValid(out error);
        }

        public enum Diet
        {
            Omnivore,
            Carnivore,
            Herbivore
        }
    }
}
