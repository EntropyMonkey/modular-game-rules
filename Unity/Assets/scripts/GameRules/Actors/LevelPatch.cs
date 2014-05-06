using UnityEngine;
using System.Collections;
using ModularRules;

public class LevelPatch : Actor
{
	public string Tag = "LevelPatch";

	void Start()
	{
		tag = Tag;

		if (generator == null)
			generator = GameObject.FindObjectOfType<RuleGenerator>();

		Initialize(generator);
	}

	void Update()
	{
		UpdateEvents();
	}
}
