using System.Collections.Generic;
using UnityEngine;

public class PlatformProxy : ProxyBehaviour
{
    private Transform Model => transform.GetChild(0);

    private void Update()
    {
        Model.localPosition = Vector3.zero;
    }

    public static List<PlatformProxy> Platforms { get; private set; } = new ();

    private void OnEnable()
    {
        Platforms.Add(this);
    }
    private void OnDisable()
    {
        Platforms.Remove(this);
    }
}
