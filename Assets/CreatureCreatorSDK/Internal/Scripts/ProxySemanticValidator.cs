using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DanielLochner.CreatureCrafter.SDK
{
    public static class ProxySemanticValidator
    {
        private const int MaxReportedErrors = 64;

        public static bool IsGameObjectValid(GameObject root, out string error)
        {
            if (root == null)
            {
                error = "The proxy-validation root could not be loaded.";
                return false;
            }

            return AreProxiesValid(root.GetComponentsInChildren<ProxyBehaviour>(true), out error);
        }

        public static bool IsSceneValid(Scene scene, out string error)
        {
            List<ProxyBehaviour> proxies = new List<ProxyBehaviour>();

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                proxies.AddRange(root.GetComponentsInChildren<ProxyBehaviour>(true));
            }

            return AreProxiesValid(proxies, out error);
        }

        public static bool AreGameObjectsValid(IEnumerable<GameObject> gameObjects, out string error)
        {
            HashSet<ProxyBehaviour> proxies = new HashSet<ProxyBehaviour>();

            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject == null)
                {
                    continue;
                }

                foreach (ProxyBehaviour proxy in gameObject.GetComponents<ProxyBehaviour>())
                {
                    proxies.Add(proxy);
                }
            }

            return AreProxiesValid(proxies, out error);
        }

        private static bool AreProxiesValid(ICollection<ProxyBehaviour> proxies, out string error)
        {
            StringBuilder errors = new StringBuilder();
            int errorCount = 0;
            Dictionary<Type, int> proxyCounts = new Dictionary<Type, int>();
            HashSet<Type> reportedCountLimits = new HashSet<Type>();

            foreach (ProxyBehaviour proxy in proxies)
            {
                if (proxy == null)
                {
                    continue;
                }

                Type proxyType = proxy.GetType();
                string objectDescription = $"{GetHierarchyPath(proxy.transform)} ({proxyType.FullName})";

                if (!CustomMapSecurityValidator.IsTrustedProxyType(proxyType))
                {
                    AppendError(errors, ref errorCount, $"{objectDescription}: unsupported proxy type; its validation code was not invoked.");
                    continue;
                }

                ValidateCount(proxyType, proxyCounts, reportedCountLimits, errors, ref errorCount);

                try
                {
                    if (!proxy.IsValid(out string proxyError))
                    {
                        if (string.IsNullOrWhiteSpace(proxyError))
                        {
                            proxyError = "The proxy configuration is invalid.";
                        }
                        AppendError(errors, ref errorCount, $"{objectDescription}: {proxyError}");
                    }
                }
                catch (Exception exception)
                {
                    AppendError(errors, ref errorCount, $"{objectDescription}: validation failed cleanly with {exception.GetType().Name}: {exception.Message}");
                }
            }

            if (errorCount > MaxReportedErrors)
            {
                errors.AppendLine($"...and {errorCount - MaxReportedErrors} additional proxy validation error(s).");
            }

            error = errors.ToString().TrimEnd();
            return errorCount == 0;
        }

        private static void ValidateCount(
            Type proxyType,
            Dictionary<Type, int> proxyCounts,
            HashSet<Type> reportedCountLimits,
            StringBuilder errors,
            ref int errorCount)
        {
            ProxyCountLimitAttribute limit = proxyType.GetCustomAttribute<ProxyCountLimitAttribute>(true);
            if (limit == null || limit.Maximum < 1)
            {
                return;
            }

            Type categoryType = limit.CategoryType ?? proxyType;
            proxyCounts.TryGetValue(categoryType, out int count);
            count++;
            proxyCounts[categoryType] = count;

            if (count > limit.Maximum && reportedCountLimits.Add(categoryType))
            {
                AppendError(
                    errors,
                    ref errorCount,
                    $"The map contains more than {limit.Maximum} {categoryType.Name} component(s).");
            }
        }

        private static void AppendError(StringBuilder errors, ref int errorCount, string error)
        {
            ++errorCount;
            if (errorCount <= MaxReportedErrors)
            {
                errors.AppendLine(error);
            }
        }

        private static string GetHierarchyPath(Transform transform)
        {
            string path = transform.name;

            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.name + "/" + path;
            }

            return $"'{path}'";
        }

    }
}
