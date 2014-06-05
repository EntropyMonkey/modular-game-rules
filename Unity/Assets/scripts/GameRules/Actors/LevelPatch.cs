using UnityEngine;
using System.Collections;

public class LevelPatch : Actor
{
	public string Tag = "LevelPatch";

	void Start()
	{
		tag = Tag;

		if (events == null || events.Count <= 0)
		{
			ScanEvents();
			ScanReactions();
		}

		if (GetComponent<PlaceholderElement>())
		{
			GetComponent<PlaceholderElement>().Id = Id;
		}
	}

	public override void ShowGui()
	{
		GUILayout.Label("LevelPatch", RuleGUI.ruleLabelStyle);
	}

	void Update()
	{
		UpdateEvents();
	}
}
