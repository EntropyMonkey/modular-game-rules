using UnityEngine;
using System.Collections;
using ModularRules;

public class LevelPatch : Actor
{
	public string Tag = "LevelPatch";

	void Start()
	{
		tag = Tag;
	}

	void Update()
	{
		UpdateEvents();
	}
}
