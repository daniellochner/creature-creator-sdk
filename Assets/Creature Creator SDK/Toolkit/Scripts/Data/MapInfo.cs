
using UnityEngine;

public class MapInfo : MonoBehaviour
{
    public PlatformProxy[] platformProxies;
    public UnlockableBodyPartProxy[] unlockableBodyPartProxies;
    public UnlockablePatternProxy[] unlockablePatternProxies;

    public void OnValidate()
    {
        platformProxies = FindObjectsOfType<PlatformProxy>(true);
        unlockableBodyPartProxies = FindObjectsOfType<UnlockableBodyPartProxy>(true);
        unlockablePatternProxies = FindObjectsOfType<UnlockablePatternProxy>(true);
    }
}