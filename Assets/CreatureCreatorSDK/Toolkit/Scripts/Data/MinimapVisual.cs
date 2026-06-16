using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    [ExecuteInEditMode]
    public class MinimapVisual : MonoBehaviour
    {
        public Material minimapVisualMaterial;

        private MapInfo mapInfo;

        private void Awake()
        {
            mapInfo = GetComponentInParent<MapInfo>();
        }
        private void Update()
        {
            minimapVisualMaterial.mainTexture = mapInfo.minimapImage;
            transform.localScale = Vector3.one * mapInfo.minimapSize;
        }
    }
}
