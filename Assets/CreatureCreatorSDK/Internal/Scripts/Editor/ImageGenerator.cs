using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ImageGenerator
{
	public static bool TryGetThumbnail(int width, int height, out Texture2D tex)
	{
		tex = null;
		Camera cam = Object.FindAnyObjectByType<Camera>();

		if (cam == null)
		{
			EditorUtility.DisplayDialog("Error", "Drag the camera prefab into the scene from the toolkit.", "OK");
			return false;
		}

        tex = GetTextureFromCamera(cam, width, height);
        return true;
	}

    private static Texture2D GetTextureFromCamera(Camera mCamera, int width, int height)
    {
        Rect rect = new Rect(0, 0, width, height);
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGBA32, false);

        mCamera.targetTexture = renderTexture;
        mCamera.Render();

        RenderTexture.active = renderTexture;

        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();

        mCamera.targetTexture = null;
        RenderTexture.active = null;
        return screenShot;
    }
}