using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : Actor
{
	public static string Tag = "Level";

	private string[] possibleLevels = new string[] { "MountainLevel", "VillageLevel" };

	void Start()
	{
		tag = Tag;
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		int selected = System.Array.FindIndex(possibleLevels, item => item == CurrentPrefab);
		prefabDropDown = new DropDown(selected, possibleLevels);
	}

	void Update()
	{
		UpdateEvents();
	}
}
