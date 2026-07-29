using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DanielLochner.CreatureCrafter.SDK
{
    [CustomEditor(typeof(MapInfo))]
    public sealed class MapInfoEditor : Editor
    {
        private SerializedProperty minimapImage;
        private SerializedProperty minimapSize;

        private bool controlsEnvironment;

        private void OnEnable()
        {
            minimapImage = serializedObject.FindProperty(nameof(MapInfo.minimapImage));
            minimapSize = serializedObject.FindProperty(nameof(MapInfo.minimapSize));

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Minimap", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(minimapImage);
            EditorGUILayout.PropertyField(minimapSize);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Environment", EditorStyles.boldLabel);

            MapInfo mapInfo = (MapInfo)target;
            if (!CanEditEnvironment(mapInfo))
            {
                controlsEnvironment = false;
                return;
            }

            if (!controlsEnvironment)
            {
                // Always start from the scene when MapInfo is selected so stale
                // serialized values can never overwrite the author's settings.
                CaptureFromScene(mapInfo);
                controlsEnvironment = true;
            }

            // MapInfo remains authoritative until its GameObject is deselected.
            ApplyToScene(mapInfo);
            DrawEnvironment(mapInfo);
        }

        private static bool CanEditEnvironment(MapInfo mapInfo)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return false;
            }

            if (mapInfo == null || PrefabUtility.IsPartOfPrefabAsset(mapInfo))
            {
                return false;
            }

            Scene scene = mapInfo.gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return false;
            }

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null && prefabStage.scene == scene)
            {
                return false;
            }

            if (SceneManager.GetActiveScene() != scene)
            {
                return false;
            }

            return Selection.activeGameObject == mapInfo.gameObject;
        }

        private static void CaptureFromScene(MapInfo mapInfo)
        {
            if (EnvironmentMatchesScene(mapInfo))
            {
                return;
            }

            mapInfo.overrideEnvironment = true;
            mapInfo.skybox = RenderSettings.skybox;
            mapInfo.sun = RenderSettings.sun;

            mapInfo.ambientMode = RenderSettings.ambientMode;
            mapInfo.ambientLight = RenderSettings.ambientLight;
            mapInfo.ambientSkyColor = RenderSettings.ambientSkyColor;
            mapInfo.ambientEquatorColor = RenderSettings.ambientEquatorColor;
            mapInfo.ambientGroundColor = RenderSettings.ambientGroundColor;
            mapInfo.ambientIntensity = RenderSettings.ambientIntensity;

            mapInfo.fog = RenderSettings.fog;
            mapInfo.fogMode = RenderSettings.fogMode;
            mapInfo.fogColor = RenderSettings.fogColor;
            mapInfo.fogDensity = RenderSettings.fogDensity;
            mapInfo.fogStartDistance = RenderSettings.fogStartDistance;
            mapInfo.fogEndDistance = RenderSettings.fogEndDistance;

            SaveChanges(mapInfo, false);
        }

        private static bool EnvironmentMatchesScene(MapInfo mapInfo)
        {
            return mapInfo.overrideEnvironment
                && mapInfo.skybox == RenderSettings.skybox
                && mapInfo.sun == RenderSettings.sun
                && mapInfo.ambientMode == RenderSettings.ambientMode
                && mapInfo.ambientLight == RenderSettings.ambientLight
                && mapInfo.ambientSkyColor == RenderSettings.ambientSkyColor
                && mapInfo.ambientEquatorColor == RenderSettings.ambientEquatorColor
                && mapInfo.ambientGroundColor == RenderSettings.ambientGroundColor
                && Mathf.Approximately(mapInfo.ambientIntensity, RenderSettings.ambientIntensity)
                && mapInfo.fog == RenderSettings.fog
                && mapInfo.fogMode == RenderSettings.fogMode
                && mapInfo.fogColor == RenderSettings.fogColor
                && Mathf.Approximately(mapInfo.fogDensity, RenderSettings.fogDensity)
                && Mathf.Approximately(mapInfo.fogStartDistance, RenderSettings.fogStartDistance)
                && Mathf.Approximately(mapInfo.fogEndDistance, RenderSettings.fogEndDistance);
        }

        private static void DrawEnvironment(MapInfo mapInfo)
        {
            EditorGUI.BeginChangeCheck();
            Material skybox = (Material)EditorGUILayout.ObjectField(
                "Skybox Material",
                RenderSettings.skybox,
                typeof(Material),
                false);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeEnvironment(mapInfo, "Change Map Skybox", () =>
                {
                    mapInfo.skybox = skybox;
                    RenderSettings.skybox = skybox;
                });
            }

            EditorGUI.BeginChangeCheck();
            Light sun = (Light)EditorGUILayout.ObjectField(
                "Sun Source",
                RenderSettings.sun,
                typeof(Light),
                true);
            if (EditorGUI.EndChangeCheck())
            {
                if (sun == null || sun.gameObject.scene == mapInfo.gameObject.scene)
                {
                    ChangeEnvironment(mapInfo, "Change Map Sun", () =>
                    {
                        mapInfo.sun = sun;
                        RenderSettings.sun = sun;
                    });
                }
                else
                {
                    Debug.LogError("The map's Sun Source must belong to the same scene as its MapInfo.", mapInfo);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ambient", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            AmbientMode ambientMode = (AmbientMode)EditorGUILayout.EnumPopup(
                "Source",
                RenderSettings.ambientMode);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeEnvironment(mapInfo, "Change Map Ambient Source", () =>
                {
                    mapInfo.ambientMode = ambientMode;
                    RenderSettings.ambientMode = ambientMode;
                });
            }

            switch (RenderSettings.ambientMode)
            {
                case AmbientMode.Flat:
                    DrawColor(
                        mapInfo,
                        "Ambient Color",
                        RenderSettings.ambientLight,
                        value =>
                        {
                            mapInfo.ambientLight = value;
                            RenderSettings.ambientLight = value;
                        });
                    break;

                case AmbientMode.Trilight:
                    DrawColor(
                        mapInfo,
                        "Sky Color",
                        RenderSettings.ambientSkyColor,
                        value =>
                        {
                            mapInfo.ambientSkyColor = value;
                            RenderSettings.ambientSkyColor = value;
                        });
                    DrawColor(
                        mapInfo,
                        "Equator Color",
                        RenderSettings.ambientEquatorColor,
                        value =>
                        {
                            mapInfo.ambientEquatorColor = value;
                            RenderSettings.ambientEquatorColor = value;
                        });
                    DrawColor(
                        mapInfo,
                        "Ground Color",
                        RenderSettings.ambientGroundColor,
                        value =>
                        {
                            mapInfo.ambientGroundColor = value;
                            RenderSettings.ambientGroundColor = value;
                        });
                    break;
            }

            EditorGUI.BeginChangeCheck();
            float ambientIntensity = EditorGUILayout.Slider(
                "Intensity Multiplier",
                RenderSettings.ambientIntensity,
                0f,
                8f);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeEnvironment(mapInfo, "Change Map Ambient Intensity", () =>
                {
                    mapInfo.ambientIntensity = ambientIntensity;
                    RenderSettings.ambientIntensity = ambientIntensity;
                });
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Fog", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            bool fog = EditorGUILayout.Toggle("Enabled", RenderSettings.fog);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeEnvironment(mapInfo, "Change Map Fog", () =>
                {
                    mapInfo.fog = fog;
                    RenderSettings.fog = fog;
                });
            }

            if (!RenderSettings.fog)
            {
                return;
            }

            DrawColor(
                mapInfo,
                "Color",
                RenderSettings.fogColor,
                value =>
                {
                    mapInfo.fogColor = value;
                    RenderSettings.fogColor = value;
                });

            EditorGUI.BeginChangeCheck();
            FogMode fogMode = (FogMode)EditorGUILayout.EnumPopup("Mode", RenderSettings.fogMode);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeEnvironment(mapInfo, "Change Map Fog Mode", () =>
                {
                    mapInfo.fogMode = fogMode;
                    RenderSettings.fogMode = fogMode;
                });
            }

            if (RenderSettings.fogMode == FogMode.Linear)
            {
                DrawFloat(
                    mapInfo,
                    "Start",
                    RenderSettings.fogStartDistance,
                    value =>
                    {
                        mapInfo.fogStartDistance = value;
                        RenderSettings.fogStartDistance = value;
                    });
                DrawFloat(
                    mapInfo,
                    "End",
                    RenderSettings.fogEndDistance,
                    value =>
                    {
                        mapInfo.fogEndDistance = value;
                        RenderSettings.fogEndDistance = value;
                    });
            }
            else
            {
                DrawFloat(
                    mapInfo,
                    "Density",
                    RenderSettings.fogDensity,
                    value =>
                    {
                        mapInfo.fogDensity = value;
                        RenderSettings.fogDensity = value;
                    });
            }
        }

        private static void DrawColor(
            MapInfo mapInfo,
            string label,
            Color currentValue,
            System.Action<Color> apply)
        {
            EditorGUI.BeginChangeCheck();
            Color value = EditorGUILayout.ColorField(label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeEnvironment(mapInfo, $"Change Map {label}", () => apply(value));
            }
        }

        private static void DrawFloat(
            MapInfo mapInfo,
            string label,
            float currentValue,
            System.Action<float> apply)
        {
            EditorGUI.BeginChangeCheck();
            float value = EditorGUILayout.FloatField(label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeEnvironment(mapInfo, $"Change Map Fog {label}", () => apply(value));
            }
        }

        private static void ChangeEnvironment(MapInfo mapInfo, string undoName, System.Action change)
        {
            Undo.RecordObject(mapInfo, undoName);
            change();
            mapInfo.overrideEnvironment = true;
            SaveChanges(mapInfo, true);
        }

        private static void SaveChanges(MapInfo mapInfo, bool updateEnvironment)
        {
            EditorUtility.SetDirty(mapInfo);
            EditorSceneManager.MarkSceneDirty(mapInfo.gameObject.scene);

            if (updateEnvironment)
            {
                DynamicGI.UpdateEnvironment();
                SceneView.RepaintAll();
            }
        }

        private void OnUndoRedo()
        {
            MapInfo mapInfo = target as MapInfo;
            if (!controlsEnvironment || !CanEditEnvironment(mapInfo))
            {
                return;
            }

            ApplyToScene(mapInfo);
            Repaint();
        }

        private static void ApplyToScene(MapInfo mapInfo)
        {
            if (EnvironmentMatchesScene(mapInfo))
            {
                return;
            }

            mapInfo.overrideEnvironment = true;
            RenderSettings.skybox = mapInfo.skybox;
            RenderSettings.sun = mapInfo.sun;

            RenderSettings.ambientMode = mapInfo.ambientMode;
            RenderSettings.ambientLight = mapInfo.ambientLight;
            RenderSettings.ambientSkyColor = mapInfo.ambientSkyColor;
            RenderSettings.ambientEquatorColor = mapInfo.ambientEquatorColor;
            RenderSettings.ambientGroundColor = mapInfo.ambientGroundColor;
            RenderSettings.ambientIntensity = mapInfo.ambientIntensity;

            RenderSettings.fog = mapInfo.fog;
            RenderSettings.fogMode = mapInfo.fogMode;
            RenderSettings.fogColor = mapInfo.fogColor;
            RenderSettings.fogDensity = mapInfo.fogDensity;
            RenderSettings.fogStartDistance = mapInfo.fogStartDistance;
            RenderSettings.fogEndDistance = mapInfo.fogEndDistance;

            SaveChanges(mapInfo, true);
        }
    }
}
