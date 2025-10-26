using System.IO;
using System.Linq;
using DanielLochner.Assets.CreatureCreator;
using UnityEditor;
using UnityEngine;

public static class BodyPartUtils
{
    private static float MAX_BODYPART_SIZE = 1f;

    public static void NewBodyPart()
    {
        if (ModdingUtils.TryCreateNewItem(out string bodyPartName, out string bodyPartPath, out BodyPartConfig config))
        {
            string dstPath = Path.Combine(bodyPartPath, $"{bodyPartName}.prefab");
            AssetDatabase.CopyAsset("Assets/CreatureCreatorSDK/Internal/Templates/BodyPart.prefab", dstPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    public static bool BuildBodyPart(BodyPartConfig config, bool buildAll)
    {
        string bodyPartName = config.GetDirectoryName();

        string[] prefabs = Directory.GetFiles(config.GetFullDirectory(), $"{bodyPartName}.prefab", SearchOption.AllDirectories);
        if (prefabs.Length != 1)
        {
            ModdingUtils.ThrowError($"One prefab must exist with the name '{bodyPartName}' (i.e., it must match the directory's name).");
            return false;
        }

        string prefabPath = ModdingUtils.ConvertGlobalPathToLocalPath(prefabs[0]);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        var meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
        var skinnedMeshRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        int vertCount = 0;
        int meshCount = 0;
        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;
            EnableReadWriteForMesh(mf.sharedMesh, ref meshCount);
            vertCount += mf.sharedMesh.vertexCount;
        }
        foreach (var smr in skinnedMeshRenderers)
        {
            if (smr.sharedMesh == null) continue;
            EnableReadWriteForMesh(smr.sharedMesh, ref meshCount);
            vertCount += smr.sharedMesh.vertexCount;
        }
        if (meshCount > 0)
        {
            Debug.Log($"Enabled Read/Write mode for {meshCount} mesh(es) in prefab '{prefab.name}'.");
        }
        if (vertCount > 1000)
        {
            ModdingUtils.ThrowError("Your body part's total vertex count cannot exceed 1000.");
            return false;
        }

        return ModdingUtils.TryBuildItem<BodyPartConfig, BodyPartConfigData>(config, buildAll, delegate
        {
            string excludeDir = Path.Combine(config.GetDirectory(), "Exclude");
            if (Directory.Exists(excludeDir))
            {
                string thumbnailPath = Path.Combine(excludeDir, "thumb.png");
                if (File.Exists(thumbnailPath))
                {
                    Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);
                    config.thumbnail = savedTexture;
                }
            }
        });
    }

    public static void TestBodyPart(BodyPartConfig config)
    {
        var path = ModdingUtils.GetBuildPath(config);
        if (!Directory.Exists(path))
        {
            ModdingUtils.ThrowError("You have not built this body part yet. You have to build it before testing.");
            return;
        }

        var windowsBundlePath = Path.Combine(path, "Body Part", "Bundles_WindowsPlayer");
        if (ModdingUtils.IsTooLarge(windowsBundlePath, MAX_BODYPART_SIZE))
        {
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "loadbodypart");
    }

    public static void UploadBodyPart(BodyPartConfig config)
    {
        var path = ModdingUtils.GetBuildPath(config);
        if (!Directory.Exists(path))
        {
            ModdingUtils.ThrowError("You have not built this body part yet. You have to build it before uploading.");
            return;
        }

        if (ModdingUtils.IsTooLarge(path, MAX_BODYPART_SIZE * 5f))
        {
            return;
        }

        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "uploadbodypart");
    }

    private static void EnableReadWriteForMesh(Mesh mesh, ref int modifiedCount)
    {
        string meshPath = AssetDatabase.GetAssetPath(mesh);
        if (string.IsNullOrEmpty(meshPath)) return;

        var importer = AssetImporter.GetAtPath(meshPath) as ModelImporter;
        if (importer != null)
        {
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
                modifiedCount++;
            }
        }
        else
        {
            Debug.Log($"Could not load ModelImporter for mesh '{mesh.name}' ({meshPath})");
        }
    }
}
