using UnityEngine;
using UnityEngine.Rendering;

namespace DanielLochner.CreatureCrafter.SDK
{
    [ExecuteInEditMode]
    public class MapInfo : MonoBehaviour
    {
        [Header("Minimap")]
        public Texture minimapImage;
        public float minimapSize;

        // The SDK fills these in from the authoring scene on export.
        [Header("Environment")]
        public bool overrideEnvironment = true;
        public Material skybox;
        public Light sun;

        [Header("Environment / Ambient")]
        public AmbientMode ambientMode = AmbientMode.Skybox;
        public Color ambientLight = new Color(0.212f, 0.227f, 0.259f);
        public Color ambientSkyColor = new Color(0.212f, 0.227f, 0.259f);
        public Color ambientEquatorColor = new Color(0.114f, 0.125f, 0.133f);
        public Color ambientGroundColor = new Color(0.047f, 0.043f, 0.035f);
        public float ambientIntensity = 1f;

        [Header("Environment / Fog")]
        public bool fog;
        public FogMode fogMode = FogMode.ExponentialSquared;
        public Color fogColor = new Color(0.5f, 0.5f, 0.5f);
        public float fogDensity = 0.01f;
        public float fogStartDistance = 0f;
        public float fogEndDistance = 300f;

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
}
