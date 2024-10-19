using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// https://forum.unity.com/threads/is-there-a-way-to-input-text-using-a-unity-editor-utility.473743/
public class EditorInputDialog : EditorWindow
{
	string description, inputText, error;
	string okButton, cancelButton;
	bool initializedPosition = false;
	Action onOKButton;

	bool shouldClose = false;
	Vector2 maxScreenPos;

	void OnGUI()
	{
		var e = Event.current;
		if(e.type == EventType.KeyDown)
		{
			switch(e.keyCode)
			{
				// Escape pressed
				case KeyCode.Escape:
					shouldClose = true;
					e.Use();
					break;

				// Enter pressed
				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					onOKButton?.Invoke();
					shouldClose = true;
					e.Use();
					break;
			}
		}

		if(shouldClose)
		{
			Close();
		}

		// Draw our control
		var rect = EditorGUILayout.BeginVertical();

		EditorGUILayout.Space(10);
		EditorGUILayout.LabelField(description);
		EditorGUILayout.LabelField(error, EditorStyles.wordWrappedLabel);

		EditorGUILayout.Space(15);
		GUI.SetNextControlName("inText");
		inputText = EditorGUILayout.TextField("", inputText);
		GUI.FocusControl("inText"); // Focus text field
		EditorGUILayout.Space(10);

		// Draw OK / Cancel buttons
		var r = EditorGUILayout.GetControlRect();
		r.width /= 2;
		if(GUI.Button(r, cancelButton))
		{
			inputText = null;   // Cancel - delete inputText
			shouldClose = true;
		}

		r.x += r.width;
		if(GUI.Button(r, okButton))
		{
			if(IsValidString(inputText))
			{
				onOKButton?.Invoke();
				shouldClose = true;
			}
			else
			{
				error = "Invalid character in map name. Only aA-zZ - _ and spaces are allowed.";
			}
		}

		EditorGUILayout.Space(4);
		EditorGUILayout.EndVertical();

		// Force change size of the window
		if(rect.width != 0 && minSize != rect.size)
		{
			minSize = maxSize = rect.size;
		}

		// Set dialog position next to mouse position
		if(!initializedPosition && e.type == EventType.Layout)
		{
			initializedPosition = true;

			// Move window to a new position. Make sure we're inside visible window
			var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			mousePos.x += 32;
			if(mousePos.x + position.width > maxScreenPos.x)
				mousePos.x -= position.width + 64; // Display on left side of mouse
			if(mousePos.y + position.height > maxScreenPos.y)
				mousePos.y = maxScreenPos.y - position.height;

			position = new Rect(mousePos.x, mousePos.y, position.width, position.height);

			// Focus current window
			Focus();
		}
	}

	/// <summary>
	/// Returns text player entered, or null if player cancelled the dialog.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="description"></param>
	/// <param name="inputText"></param>
	/// <param name="okButton"></param>
	/// <param name="cancelButton"></param>
	/// <returns></returns>
	public static string Show(string title, string description, string inputText, string okButton = "OK", string cancelButton = "Cancel")
	{
		// Make sure our popup is always inside parent window, and never offscreen
		// So get caller's window size
		var maxPos = GUIUtility.GUIToScreenPoint(new Vector2(Screen.width, Screen.height));

		string ret = null;
		var window = CreateInstance<EditorInputDialog>();
		window.maxScreenPos = maxPos;
		window.titleContent = new GUIContent(title);
		window.description = description;
		window.inputText = inputText;
		window.okButton = okButton;
		window.cancelButton = cancelButton;
		window.onOKButton += () => ret = window.inputText;
		window.ShowModal();

		return ret;
	}

	static bool IsValidString(string input)
	{
		string pattern = @"^[A-Za-z0-9 _-]*$";
		return Regex.IsMatch(input, pattern);
	}
}