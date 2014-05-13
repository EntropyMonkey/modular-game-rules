using UnityEngine;
using System.Collections;
using ModularRules;

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

	void Update()
	{
		UpdateEvents();
	}
}
