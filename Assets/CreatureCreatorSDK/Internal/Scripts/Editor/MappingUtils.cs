using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using DanielLochner.CreatureCrafter.SDK;
#if UNITY_NAVIGATION
using Unity.AI.Navigation;
#endif

public static class MappingUtils
{
    private const float MAX_MAP_SIZE = 20f;

    private const string EXPORT_DIRECTORY_NAME = "__MapExport";

    public static void NewMap()
	{
        if (ModdingUtils.TryCreateNewItem(out string mapName, out string mapPath, out MapConfig config))
        {
            string dstPath = Path.Combine(mapPath, $"{mapName}.unity");
            string dstAssetPath = ModdingUtils.ConvertGlobalPathToLocalPath(dstPath);
            AssetDatabase.CopyAsset("Assets/CreatureCreatorSDK/Internal/Templates/Map.unity", dstAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(dstAssetPath);
        }
    }

    public static bool BuildMap(MapConfig config, bool buildAll)
	{
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return false;
        }

        string exportDirectory = GetExportDirectory(config);
        AssetDatabase.DeleteAsset(exportDirectory);

        string[] scenes = Directory.GetFiles(config.GetFullDirectory(), "*.unity", SearchOption.AllDirectories);
        if (scenes.Length > 1)
        {
            ModdingUtils.ThrowError("More than one scene found in the map folder.");
            return false;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!CustomMapValidator.IsSceneValid(scene, out string error))
        {
            ModdingUtils.ThrowError(error);
            return false;
        }

        if (string.IsNullOrEmpty(scene.path))
        {
            ModdingUtils.ThrowError("Save your map scene before building it.");
            return false;
        }

        if (!ShouldBuildWithoutNavMeshSurface(scene))
        {
            return false;
        }

        if (!ShouldBuildWithoutSceneOnlyData(scene))
        {
            return false;
        }

        EnableReadWriteForCustomObjects(scene);

        try
        {
            return ModdingUtils.TryBuildItem<MapConfig, MapConfigData>(config, buildAll, delegate (string buildPath)
            {
                CustomMapSecurityValidator.SanitizeAnimators(scene);
                EditorSceneManager.SaveOpenScenes();
                GenerateThumbnail(config);
                UpdateUnlockables(config);

                ExportMapPrefab(config, scene, exportDirectory);
            });
        }
        finally
        {
            AssetDatabase.DeleteAsset(exportDirectory);
        }
	}

    private static void EnableReadWriteForCustomObjects(Scene scene)
    {
        List<GameObject> customObjects = new List<GameObject>();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (CustomObjectProxy customObject in root.GetComponentsInChildren<CustomObjectProxy>(true))
            {
                customObjects.Add(customObject.gameObject);
            }

            // A spawner's model can be a prefab that only exists in the project, so it is not covered by the scene walk above.
            foreach (SpawnerProxy spawner in root.GetComponentsInChildren<SpawnerProxy>(true))
            {
                if (spawner.model == null) continue;
                customObjects.Add(spawner.model.gameObject);
            }
        }

        int modifiedAssetCount = MeshReadWriteUtils.EnableReadWrite(customObjects);
        if (modifiedAssetCount > 0)
        {
            Debug.Log($"Enabled Read/Write mode for {modifiedAssetCount} model asset(s) used by the map's custom objects.");
        }
    }

    private static string GetExportDirectory(MapConfig config)
    {
        return config.GetDirectory() + EXPORT_DIRECTORY_NAME;
    }

    private static void ExportMapPrefab(MapConfig config, Scene scene, string exportDirectory)
    {
        if (!AssetDatabase.IsValidFolder(exportDirectory))
        {
            AssetDatabase.CreateFolder(config.GetDirectory().TrimEnd('/'), EXPORT_DIRECTORY_NAME);

            if (!AssetDatabase.IsValidFolder(exportDirectory))
            {
                ModdingUtils.ThrowError($"Failed to create the export folder '{exportDirectory}'. Close anything that might be holding onto that folder, then try again.");
            }
        }

        string mapName = Path.GetFileNameWithoutExtension(scene.path);

        string copyPath = $"{exportDirectory}/{mapName}.unity";
        string prefabPath = $"{exportDirectory}/{mapName}.prefab";

        if (!AssetDatabase.CopyAsset(scene.path, copyPath))
        {
            ModdingUtils.ThrowError($"Failed to copy '{scene.path}' for export.");
        }

        Scene copy = default;
        try
        {
            copy = EditorSceneManager.OpenScene(copyPath, OpenSceneMode.Additive);

            CaptureEnvironment(copy, exportDirectory);

            GameObject[] sceneRoots = copy.GetRootGameObjects();

            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(copy);
            GameObject root = new GameObject(mapName);
            SceneManager.SetActiveScene(activeScene);

            foreach (GameObject sceneRoot in sceneRoots)
            {
                sceneRoot.transform.SetParent(root.transform, true);
            }

#if UNITY_NAVIGATION
            UnpackNavMeshSurfaceInstances(root);
#endif

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out bool success);
            if (!success || prefab == null)
            {
                if (TryFindMissingScript(root, out string missingScriptPath))
                {
                    ModdingUtils.ThrowError($"'{missingScriptPath}' has a missing script on it, which stops the map from being exported. This usually means a package the map relies on is not installed, or that the project has compiler errors.");
                }
                ModdingUtils.ThrowError($"Failed to export the map as a prefab to '{prefabPath}'. Unity logged the reason to the Console just above this error.");
            }

#if UNITY_NAVIGATION
            VerifyNavMeshData(root, prefabPath);
#endif

            if (!CustomMapValidator.IsMapPrefabValid(prefab, new HashSet<GameObject>(), out string error))
            {
                ModdingUtils.ThrowError(error);
            }

            if (!IsMapPrefabIdentifiable(config, prefab, exportDirectory, out string identityError))
            {
                ModdingUtils.ThrowError(identityError);
            }
        }
        finally
        {
            if (copy.IsValid())
            {
                EditorSceneManager.CloseScene(copy, true);
            }
            AssetDatabase.DeleteAsset(copyPath);
        }
    }

    private static void CaptureEnvironment(Scene copy, string exportDirectory)
    {
        MapInfo mapInfo = null;
        foreach (GameObject root in copy.GetRootGameObjects())
        {
            mapInfo = root.GetComponentInChildren<MapInfo>(true);
            if (mapInfo != null)
            {
                break;
            }
        }

        if (mapInfo == null)
        {
            return;
        }

        mapInfo.overrideEnvironment = true;
        mapInfo.skybox = EnsureBundlableMaterial(RenderSettings.skybox, exportDirectory);

        mapInfo.sun = FindLightInScene(RenderSettings.sun, copy);

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

        EditorUtility.SetDirty(mapInfo);
    }

    private static Material EnsureBundlableMaterial(Material material, string exportDirectory)
    {
        if (material == null)
        {
            return null;
        }

        string assetPath = AssetDatabase.GetAssetPath(material);
        if (assetPath.StartsWith("Assets/"))
        {
            return material;
        }

        string copyPath = AssetDatabase.GenerateUniqueAssetPath($"{exportDirectory}/{material.name}.mat");
        AssetDatabase.CreateAsset(new Material(material), copyPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"'{material.name}' is a built-in Unity material and cannot go into an AssetBundle, so a copy of it was included in the map instead.");

        return AssetDatabase.LoadAssetAtPath<Material>(copyPath);
    }

    private static Light FindLightInScene(Light light, Scene scene)
    {
        if (light == null)
        {
            return null;
        }

        string path = GetHierarchyPath(light.transform);

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (path == root.name)
            {
                return root.GetComponent<Light>();
            }

            if (path.StartsWith(root.name + "/"))
            {
                Transform match = root.transform.Find(path.Substring(root.name.Length + 1));
                if (match != null)
                {
                    return match.GetComponent<Light>();
                }
            }
        }

        return null;
    }

    private static bool TryFindMissingScript(GameObject root, out string path)
    {
        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
        {
            foreach (Component component in transform.GetComponents<Component>())
            {
                if (component == null)
                {
                    path = GetHierarchyPath(transform);
                    return true;
                }
            }
        }

        path = "";
        return false;
    }

    private static string GetHierarchyPath(Transform transform)
    {
        string path = transform.name;

        for (Transform parent = transform.parent; parent != null; parent = parent.parent)
        {
            path = parent.name + "/" + path;
        }

        return path;
    }

    private static bool IsMapPrefabIdentifiable(MapConfig config, GameObject prefab, string exportDirectory, out string error)
    {
        int count = prefab.GetComponentsInChildren<MapInfo>(true).Length;
        if (count != 1)
        {
            error = count == 0
                ? "The exported map has no MapInfo on it. Add one to an object in your map."
                : $"The exported map has {count} MapInfo components on it, and a map can only have 1. Note that this counts the ones on inactive objects too.";
            return false;
        }

        string mapDirectory = config.GetFullDirectory();
        string excludeDirectory = $"{mapDirectory.TrimEnd('/')}/Exclude";

        foreach (string file in Directory.GetFiles(mapDirectory, "*.prefab", SearchOption.AllDirectories))
        {
            if (file.Replace('\\', '/').StartsWith(excludeDirectory))
            {
                continue;
            }

            string assetPath = ModdingUtils.ConvertGlobalPathToLocalPath(file);
            if (assetPath.StartsWith(exportDirectory))
            {
                continue;
            }

            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset != null && asset.GetComponentsInChildren<MapInfo>(true).Length > 0)
            {
                error = $"'{assetPath}' has a MapInfo on it. The game identifies your map by its MapInfo, so nothing else in the map folder can have one. Remove it, or move that prefab into the 'Exclude' folder.";
                return false;
            }
        }

        error = "";
        return true;
    }

    private static bool ShouldBuildWithoutSceneOnlyData(Scene scene)
    {
        bool hasBakedLighting = LightmapSettings.lightmaps.Length > 0
            || (LightmapSettings.lightProbes != null && LightmapSettings.lightProbes.count > 0);

        string warningKey = "CreatureCreatorSDK.SceneOnlyDataWarning." + scene.path;
        if (EditorPrefs.GetBool(warningKey, false))
        {
            return true;
        }

        bool buildAnyway = true;
        if (hasBakedLighting)
        {
            buildAnyway = EditorUtility.DisplayDialog(
                "Scene Data Will Not Be Exported",
                "Your map is currently lit by baked lighting, so it will look different in the game than it does here.",
                "Build Anyway",
                "Cancel Build");

            if (buildAnyway)
            {
                EditorPrefs.SetBool(warningKey, true);
            }
        }

        return buildAnyway;
    }

#if UNITY_NAVIGATION
    // A surface added to a prefab instance is stored as an override on it, and the export does not
    // carry that over. This only touches the copy of the scene, which is deleted after the build.
    private static void UnpackNavMeshSurfaceInstances(GameObject root)
    {
        foreach (NavMeshSurface surface in root.GetComponentsInChildren<NavMeshSurface>(true))
        {
            if (surface.navMeshData == null)
            {
                continue;
            }

            // Unpacking the outermost instance can leave the surface on a nested one.
            for (int depth = 0; depth < 8 && PrefabUtility.IsPartOfPrefabInstance(surface.gameObject); depth++)
            {
                GameObject instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(surface.gameObject);
                if (instanceRoot == null)
                {
                    break;
                }

                PrefabUtility.UnpackPrefabInstance(instanceRoot, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }
        }
    }

    private static void VerifyNavMeshData(GameObject root, string prefabPath)
    {
        NavMeshSurface[] sourceSurfaces = root.GetComponentsInChildren<NavMeshSurface>(true);
        NavMeshSurface[] exportedSurfaces = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath).GetComponentsInChildren<NavMeshSurface>(true);

        for (int i = 0; i < sourceSurfaces.Length; i++)
        {
            if (sourceSurfaces[i].navMeshData != null && (i >= exportedSurfaces.Length || exportedSurfaces[i].navMeshData == null))
            {
                ModdingUtils.ThrowError($"The nav mesh baked onto '{GetHierarchyPath(sourceSurfaces[i].transform)}' could not be exported with the map.");
            }
        }
    }
#endif

    private static bool HasBakedSceneNavMesh(Scene scene)
    {
        string sceneDataDirectory = Path.Combine(
            Path.GetDirectoryName(scene.path),
            Path.GetFileNameWithoutExtension(scene.path));

        return File.Exists(Path.Combine(sceneDataDirectory, "NavMesh.asset"));
    }

    private static bool ShouldBuildWithoutNavMeshSurface(Scene scene)
    {
        if (HasNavMeshSurface(scene))
        {
            return true;
        }

        string warningKey = "CreatureCreatorSDK.MissingNavMeshSurfaceWarning." + scene.path;
        if (EditorPrefs.GetBool(warningKey, false))
        {
            return true;
        }

        bool buildAnyway = EditorUtility.DisplayDialog(
            "Missing NavMesh Surface",
            "This map does not contain a NavMeshSurface. You should probably add one before building... otherwise the game will need to build the nav mesh at runtime, which can cause significant lag when loading.",
            "Build Anyway",
            "Cancel Build");
        if (buildAnyway)
        {
            EditorPrefs.SetBool(warningKey, true);
        }

        return buildAnyway;
    }

    private static bool HasNavMeshSurface(Scene scene)
    {
        System.Type navMeshSurfaceType = System.Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
        if (navMeshSurfaceType == null)
        {
            return false;
        }

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponentsInChildren(navMeshSurfaceType, true).Length > 0)
            {
                return true;
            }
        }

        return false;
    }

	public static void TestMap(MapConfig config)
	{
		var path = ModdingUtils.GetBuildPath(config);
        if (!Directory.Exists(path))
		{
			ModdingUtils.ThrowError("You have not built this map yet. You have to build it before testing.");
			return;
		}

        var bundlePath = ModdingUtils.GetBundleBuildPath(config, ModdingUtils.GetCurrentEditorPlayerPlatform());
        if (ModdingUtils.IsTooLarge(bundlePath, MAX_MAP_SIZE))
        {
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "loadmap");
	}

	public static void UploadMap(MapConfig config)
	{
        var path = ModdingUtils.GetBuildPath(config);
        if (!Directory.Exists(path))
        {
            ModdingUtils.ThrowError("You have not built this map yet. You have to build it before uploading.");
            return;
        }

        if (config.thumbnail == null)
		{
			ModdingUtils.ThrowError("Missing thumbnail. Assign a thumbnail in the config file of your map.");
			return;
		}

        if (ModdingUtils.IsTooLarge(path, MAX_MAP_SIZE * 5f))
        {
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "uploadmap");
	}

    public static void CheckForErrors()
    {
        if (CustomMapValidator.IsSceneValid(SceneManager.GetActiveScene(), out string error))
        {
            EditorUtility.DisplayDialog("There are no errors.", "Everything is OK!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", error, "OK");
        }
    }
    public static void GenerateThumbnail(MapConfig config)
    {
        if (ImageGenerator.TryGetThumbnail(512, 512, out Texture2D tex))
        {
            string thumbnailDirectory = $"{config.GetDirectory().TrimEnd('/')}/Exclude";

            if (!Directory.Exists(thumbnailDirectory))
            {
                Directory.CreateDirectory(thumbnailDirectory);
            }

            string thumbnailPath = $"{thumbnailDirectory}/thumb.png";

            byte[] textureData = tex.EncodeToPNG();
            File.WriteAllBytes(thumbnailPath, textureData);

            AssetDatabase.Refresh();

            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);
            config.thumbnail = savedTexture;
            EditorUtility.SetDirty(config);
        }
    }
    public static void UpdateUnlockables(MapConfig config)
    {
        config.EnsureInitialized();

        config.bodyPartIds.Clear();
        foreach (var bodyPartProxy in Object.FindObjectsByType<UnlockableBodyPartProxy>(FindObjectsSortMode.None))
        {
            if (!string.IsNullOrEmpty(bodyPartProxy.itemId))
            {
                config.bodyPartIds.Add(bodyPartProxy.itemId);
            }
        }
        
        config.patternIds.Clear();
        foreach (var patternProxy in Object.FindObjectsByType<UnlockablePatternProxy>(FindObjectsSortMode.None))
        {
            if (!string.IsNullOrEmpty(patternProxy.itemId))
            {
                config.patternIds.Add(patternProxy.itemId);
            }
        }

        EditorUtility.SetDirty(config);
    }
}
