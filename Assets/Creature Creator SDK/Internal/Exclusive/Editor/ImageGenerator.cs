using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if !REDMATCH
public static class ImageGenerator
{
	public static bool TryGetThumbnail(string cameraName, int width, int height, out Texture2D tex)
	{
		tex = null;
		GameObject camObject = GameObject.Find(cameraName);

		if(camObject == null)
		{
			EditorUtility.DisplayDialog("Error", $"Drag the {cameraName} prefab into the scene from the \"Toolkit/Prefabs/Map Metadata\" folder.", "OK");
			return false;
		}

		Camera cam = camObject.GetComponent<Camera>();

		RenderTexture renderTexture = new RenderTexture(width, height, 0);
		cam.targetTexture = renderTexture;
		Texture2D generatedTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
		cam.Render();
		RenderTexture.active = renderTexture;
		generatedTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		generatedTexture.Apply();
		RenderTexture.active = null;
		cam.targetTexture = null;

		tex = generatedTexture;
		return true;
	}
}
#endif