using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MeshReadWriteUtils
{
    public static int EnableReadWrite(IEnumerable<GameObject> roots)
    {
        HashSet<Mesh> meshes = new HashSet<Mesh>();
        foreach (GameObject root in roots)
        {
            CollectMeshes(root, meshes);
        }

        HashSet<string> modifiedPaths = new HashSet<string>();
        foreach (Mesh mesh in meshes)
        {
            EnableReadWriteForMesh(mesh, modifiedPaths);
        }

        return modifiedPaths.Count;
    }

    private static void CollectMeshes(GameObject root, HashSet<Mesh> meshes)
    {
        if (root == null) return;

        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null) continue;
            meshes.Add(mf.sharedMesh);
        }
        foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr.sharedMesh == null) continue;
            meshes.Add(smr.sharedMesh);
        }
    }

    private static void EnableReadWriteForMesh(Mesh mesh, HashSet<string> modifiedPaths)
    {
        string meshPath = AssetDatabase.GetAssetPath(mesh);
        if (string.IsNullOrEmpty(meshPath))
        {
            // The mesh is stored in the scene or prefab itself, so there is no importer to change.
            WarnIfUnreadable(mesh, "it is not saved as an asset");
            return;
        }

        if (AssetImporter.GetAtPath(meshPath) is ModelImporter importer)
        {
            if (!importer.isReadable && modifiedPaths.Add(meshPath))
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
            return;
        }

        WarnIfUnreadable(mesh, $"'{meshPath}' is not an imported model");
    }

    private static void WarnIfUnreadable(Mesh mesh, string reason)
    {
        if (mesh.isReadable) return;

        Debug.LogWarning($"'{mesh.name}' does not have Read/Write enabled, and {reason}, so the SDK could not enable it for you. Enable it manually, otherwise the mesh may not work correctly in the game.", mesh);
    }
}
