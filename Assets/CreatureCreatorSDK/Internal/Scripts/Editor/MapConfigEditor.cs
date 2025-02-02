using System.IO;
using UnityEditor;
using UnityEngine;

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
            MappingUtils.GenerateThumbnail(config);
		}
	}
}