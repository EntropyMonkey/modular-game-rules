using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DropDown
{
	public int Selected;

	public GUIContent[] Content
	{
		get;
		private set;
	}

	float minWidth;
	float minHeight;

	bool extended = false;

	Vector2 keyScrollPos = Vector2.zero;

	public DropDown(int selected, string[] content, float minWidth = 100, float minHeight = 100)
	{
		Selected = selected;
		Content = new GUIContent[content.Length];
		for (int i = 0; i < content.Length; i++)
		{
			Content[i] = new GUIContent(content[i]);
		}

		this.minWidth = minWidth;
		this.minHeight = minHeight;
	}

	public void ChangeContent(string[] newContent, int selected = -1)
	{
		if (selected > 0)
			Selected = selected;

		Content = new GUIContent[newContent.Length];
		for (int i = 0; i < Content.Length; i++)
			Content[i] = new GUIContent(newContent[i]);
	}

	public int Draw()
	{
		int result = -1;
		if (extended)
		{
			keyScrollPos = GUILayout.BeginScrollView(keyScrollPos, GUILayout.MinHeight(minHeight), GUILayout.MinWidth(minWidth));

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

		if (extended && Input.GetMouseButton(0) && Event.current.type == EventType.Repaint &&
			!GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
		{
			extended = false;
		}

		return Selected;
	}
}
