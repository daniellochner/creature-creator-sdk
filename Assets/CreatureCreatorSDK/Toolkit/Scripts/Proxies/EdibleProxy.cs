using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class EdibleProxy : ProxyBehaviour
    {
        public Diet diet = Diet.Omnivore;
        public Vector2 minMaxHunger = new Vector2(0.25f, 0.5f);
        public Vector2 minMaxHeal = new Vector2(15f, 20f);

        public override bool IsValid()
        {
            if (minMaxHunger.x < 0 || minMaxHunger.y > 1)
            {
                Debug.LogError("Hunger value must be in the range [0, 1].");
                return false;
            }

            if (minMaxHeal.x < 0 || minMaxHeal.y > 100)
            {
                Debug.LogError("Heal value must be in the range [0, 100].");
                return false;
            }

            return base.IsValid();
        }

        public enum Diet
        {
            Omnivore,
            Carnivore,
            Herbivore
        }
    }
}
