using System.IO;
using UnityEditor;
using UnityEngine;

#if !REDMATCH
[CustomEditor(typeof(MapConfig))]
public class MapConfigEditor : Editor
{
	MapConfig config => (MapConfig)target;

	public override void OnInspectorGUI()
	{
		GUILayout.Label("Settings", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

		DrawDefaultInspector();

		if(GUILayout.Button("Generate Thumbnail"))
		{
			if(ImageGenerator.TryGetThumbnail("ThumbnailCamera", 512, 512, out Texture2D tex))
			{
				string thumbnailDirectory = Path.Combine(config.GetMapDirectory(), "Exclude");

				if(!Directory.Exists(thumbnailDirectory))
				{
					Directory.CreateDirectory(thumbnailDirectory);
				}

				string thumbnailPath = Path.Combine(thumbnailDirectory, "thumb.png");

				byte[] textureData = tex.EncodeToPNG();
				File.WriteAllBytes(thumbnailPath, textureData);
				
				AssetDatabase.Refresh();

				Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);
				config.thumbnail = savedTexture;
				EditorUtility.SetDirty(config);
			}
		}
	}
}
#endif