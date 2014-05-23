using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ModularRules;

public class IngameGUI : MonoBehaviour
{
	private List<Counters.Counter> guiCounters = new List<Counters.Counter>();

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

		foreach (Counters.Counter counter in guiCounters)
		{
			GUILayout.BeginVertical();
			GUILayout.Label(counter.name);
			GUILayout.Label(counter.value.ToString());
			GUILayout.EndVertical();
		}

		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	public void AddCounter(Counters.Counter counter)
	{
		if (!guiCounters.Contains(counter))
		{
			guiCounters.Add(counter);
		}
	}

	public void RemoveCounter(Counters.Counter counter)
	{
		guiCounters.Remove(counter);
	}
}
