using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DanielLochner.CreatureCrafter.SDK
{
	public static class CustomMapValidator
	{
		public static bool IsSceneValid(Scene scene, out string error)
		{
			return IsSceneValid(scene, new HashSet<GameObject>(), out error);
		}

		public static bool IsSceneValid(Scene scene, HashSet<GameObject> validated, out string error)
		{
			if(!ProxySemanticValidator.IsSceneValid(scene, out error))
			{
				return false;
			}

			if(!CustomMapSecurityValidator.IsSceneValid(scene, validated, out error))
			{
				return false;
			}

			if(!ProxySemanticValidator.AreGameObjectsValid(validated, out error))
			{
				return false;
			}

			if(!CustomMapRequiredComponentsValidator.IsSceneValid(scene, out error))
			{
				return false;
			}

			if(!CustomMapErrorValidator.IsSceneValid(scene, out error))
			{
				return false;
			}

			return true;
		}

		public static bool IsMapPrefabValid(GameObject prefab, HashSet<GameObject> validated, out string error)
		{
			if(prefab == null)
			{
				error = "The map prefab could not be loaded.";
				return false;
			}

			if(!ProxySemanticValidator.IsGameObjectValid(prefab, out error))
			{
				return false;
			}

			if(!CustomMapSecurityValidator.IsGameObjectValid(prefab, validated, out error))
			{
				return false;
			}

			return ProxySemanticValidator.AreGameObjectsValid(validated, out error);
		}

		public static bool IsMapSceneStructureValid(Scene scene, out string error)
		{
			if(!CustomMapRequiredComponentsValidator.IsSceneValid(scene, out error))
			{
				return false;
			}

			return CustomMapErrorValidator.IsSceneValid(scene, out error);
		}
	}
}
