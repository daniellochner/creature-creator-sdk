using System;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    public class CustomObjectProxy : ProxyBehaviour
    {
        [HideInInspector] public string modelId;

        public static List<CustomObjectProxy> Proxies { get; private set; } = new();

        private void OnEnable()
        {
            Proxies.Add(this);
        }
        private void OnDisable()
        {
            Proxies.Remove(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
            {
                // Asset GUID keeps the id unique and copy-safe (a duplicated prefab is a new asset).
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
                if (!string.IsNullOrEmpty(guid) && modelId != guid)
                {
                    modelId = guid;
                }
                return;
            }

            if (string.IsNullOrEmpty(modelId) || HasDuplicateId())
            {
                modelId = Guid.NewGuid().ToString();
            }
        }
        private bool HasDuplicateId()
        {
            foreach (var other in FindObjectsByType<CustomObjectProxy>(FindObjectsSortMode.None))
            {
                if (other != this && other.modelId == modelId)
                {
                    return true;
                }
            }
            return false;
        }
#endif
    }
}