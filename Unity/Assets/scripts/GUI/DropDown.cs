using UnityEngine;
using System.Collections;

public class DropDown
{
	public int Selected;

	public GUIContent[] Content
	{
		get;
		private set;
	}

	float width;
	float minHeight;

	bool extended = false;

	Vector2 keyScrollPos = Vector2.zero;

	//public DropDown(int selected, GUIContent[] content, float width = 100, float height = 300)
	//{
	//	Selected = selected;
	//	Content = content;
	//	this.width = width;
	//	this.height = height;
	//}

	public DropDown(int selected, string[] content, float width = 100, float minHeight = 100)
	{
		Selected = selected;
		Content = new GUIContent[content.Length];
		for (int i = 0; i < content.Length; i++)
		{
			Content[i] = new GUIContent(content[i]);
		}

		this.width = width;
		this.minHeight = minHeight;
	}

	public int Draw()
	{
		int result = -1;
		if (extended)
		{
			keyScrollPos = GUILayout.BeginScrollView(keyScrollPos, GUILayout.MinHeight(minHeight), GUILayout.Width(width));
			result = GUILayout.SelectionGrid(Selected, Content, 1);

			GUILayout.EndScrollView();

			if (result != Selected)
			{
				extended = false;
			}

			Selected = result;
		}
		else
		{
			if (Selected >= 0 && Selected < Content.Length && GUILayout.Button(Content[Selected], RuleGUI.ruleEditableStyle))
			{
				extended = true;
			}
			else if (Selected < 0 || Selected > Content.Length && GUILayout.Button("...", RuleGUI.ruleEditableStyle))
			{
				extended = true;
			}
		}

		if (extended && Input.GetMouseButton(0) && Event.current.type == EventType.Repaint && !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
			extended = false;

		return Selected;
	}

	//int Draw(GUIStyle style, params GUILayoutOption[] options)
	//{
	//	if (extended)
	//	{
	//		keyScrollPos = GUILayout.BeginScrollView(keyScrollPos, GUILayout.Height(height), GUILayout.Width(width));
	//		Selected = GUILayout.SelectionGrid(Selected, Content, 1, style, options);
	//		GUILayout.EndScrollView();

	//		if (Input.GetMouseButtonUp(0))
	//			extended = false;
	//	}
	//	else
	//	{
	//		if (Selected >= 0 && Selected < Content.Length && GUILayout.Button(Content[Selected], GUILayout.Width(width)))
	//		{
	//			extended = true;
	//		}
	//	}

	//	if (Selected >= 0 && Selected < Content.Length)
	//		return Selected;
	//	else
	//		return -1;
	//}
}
