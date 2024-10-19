using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReferenceCacher : MonoBehaviour
{
	public static void BuildCache(Scene scene)
	{
		List<PlatformProxy> platforms = new();

		MapInfo mapInfo = null;

		void CheckRecursively(Transform trans)
		{
			foreach(Transform child in trans)
			{
				if(child != null)
					CheckRecursively(child);
			}

			if(trans.gameObject.TryGetComponent(out PlatformProxy proxy))
			{
				platforms.Add(proxy);
			}
			else if(trans.gameObject.TryGetComponent(out MapInfo info))
			{
				mapInfo = info;
			}
		}

		foreach(var root in scene.GetRootGameObjects())
		{
			CheckRecursively(root.transform);
		}

		mapInfo.PlatformProxies = platforms.ToArray();
		EditorUtility.SetDirty(mapInfo);

		EditorSceneManager.SaveScene(scene, scene.path);
	}
}
