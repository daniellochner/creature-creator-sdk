using System.Collections.Generic;
using UnityEngine;

public class WaterProxy : ProxyBehaviour
{
    public bool allowSwimming = true;
    public GameObject customSplashFX;

    public static List<WaterProxy> Proxies { get; private set; } = new ();

    private void OnEnable()
    {
        Proxies.Add(this);
    }
    private void OnDisable()
    {
        Proxies.Remove(this);
    }
}
