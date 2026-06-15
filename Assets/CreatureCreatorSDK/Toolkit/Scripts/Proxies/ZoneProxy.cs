using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneProxy : ProxyBehaviour
{
    public new string name;

    public static List<ZoneProxy> Proxies { get; private set; } = new ();

    private void OnEnable()
    {
        Proxies.Add(this);
    }
    private void OnDisable()
    {
        Proxies.Remove(this);
    }
}
