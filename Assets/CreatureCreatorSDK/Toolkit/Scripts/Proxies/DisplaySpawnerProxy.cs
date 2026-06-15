using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplaySpawnerProxy : ProxyBehaviour
{
    public TextAsset creatureDataAsset;

    public static List<DisplaySpawnerProxy> Proxies { get; private set; } = new ();

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
        if (creatureDataAsset == null)
        {
            Debug.LogError("A valid .dat file must be provided.");
            return false;
        }

        return base.IsValid();
    }
}
