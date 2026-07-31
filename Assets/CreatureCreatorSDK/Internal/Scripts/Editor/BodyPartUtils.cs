using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DanielLochner.CreatureCrafter.SDK;

public static class BodyPartUtils
{
    private static float MAX_BODYPART_SIZE = 1f;

    public static void NewBodyPart()
    {
        if (ModdingUtils.TryCreateNewItem(out string bodyPartName, out string bodyPartPath, out BodyPartConfig config))
        {
            string dstPath = Path.Combine(bodyPartPath, $"{bodyPartName}.prefab");
            string dstAssetPath = ModdingUtils.ConvertGlobalPathToLocalPath(dstPath);
            AssetDatabase.CopyAsset("Assets/CreatureCreatorSDK/Internal/Templates/BodyPart.prefab", dstAssetPath);
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

        int modifiedAssetCount = MeshReadWriteUtils.EnableReadWrite(new[] { prefab });
        if (modifiedAssetCount > 0)
        {
            Debug.Log($"Enabled Read/Write mode for {modifiedAssetCount} model asset(s) in prefab '{prefab.name}'.");
        }

        int vertCount = 0;
        int meshCount = 0;
        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;
            vertCount += mf.sharedMesh.vertexCount;
            meshCount++;
        }
        foreach (var smr in skinnedMeshRenderers)
        {
            if (smr.sharedMesh == null) continue;
            vertCount += smr.sharedMesh.vertexCount;
            meshCount++;
        }

        var maxVertCount = 2048;
        if (vertCount > maxVertCount)
        {
            ModdingUtils.ThrowError($"'{prefab.name}' exceeds the maximum allowed vertex count of {maxVertCount}. It currently has {vertCount} vertices across {meshCount} mesh(es).");
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

        var bundlePath = ModdingUtils.GetBundleBuildPath(config, ModdingUtils.GetCurrentEditorPlayerPlatform());
        if (ModdingUtils.IsTooLarge(bundlePath, MAX_BODYPART_SIZE))
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
}
