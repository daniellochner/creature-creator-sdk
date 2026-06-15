using System.Collections.Generic;
using UnityEngine;

public class FoodProxy : ProxyBehaviour
{
    public Diet diet = Diet.Omnivore;
    public bool isHoldable = true;
    public Vector2 minMaxHunger = new Vector2(0.25f, 0.5f);
    public Vector2 minMaxHeal = new Vector2(15f, 20f);

    public static List<FoodProxy> Proxies { get; private set; } = new ();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && !string.IsNullOrEmpty(gameObject.scene.name))
        {
            Debug.Log("Food must be spawned using a spawner.");
        }
    }
#endif
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
        if (minMaxHunger.x < 0 || minMaxHunger.y > 1)
        {
            Debug.LogError("Hunger value must be in the range [0, 1].");
            return false;
        }

        if (minMaxHeal.x < 0 || minMaxHeal.y > 1)
        {
            Debug.LogError("Heal value must be in the range [0, 1].");
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
