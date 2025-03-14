
using UnityEngine;

[ExecuteInEditMode]
public class MapInfo : MonoBehaviour
{
    [Header("Minimap")]
    public Texture minimapImage;
    public float minimapSize;

    public bool IsValidMinimap => (minimapImage != null) && (minimapSize > 0);

    private void Update()
    {
#if UNITY_EDITOR
        if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(UnityEditor.Selection.activeGameObject == gameObject && IsValidMinimap);
        }
#endif
    }
}