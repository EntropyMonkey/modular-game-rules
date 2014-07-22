using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IngameGUI : MonoBehaviour
{
	private static IngameGUI instance = null;
	public static IngameGUI Instance()
	{
		return instance;
	}

	void Awake()
	{
		if (instance == null)
			instance = this;
		else
			Destroy(this);
	}

	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0, 0, Screen.width - 20, Screen.height - 20));
		GUILayout.BeginHorizontal();

		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
