using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockablePatternProxy : UnlockableItemProxy
{
    public static List<UnlockablePatternProxy> Proxies { get; private set; } = new();

    private void OnEnable()
    {
        Proxies.Add(this);
    }
    private void OnDisable()
    {
        Proxies.Remove(this);
    }
}