using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelPatch : Actor
{
	public static string Tag = "LevelPatch";

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

	public override BaseRuleElement.RuleData GetRuleInformation()
	{
		BaseRuleElement.ActorData actorData = base.GetRuleInformation() as BaseRuleElement.ActorData;

		actorData.prefabName = "LevelPatch";

		return actorData;
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("LevelPatch", RuleGUI.ruleLabelStyle);
	}

	void Update()
	{
		UpdateEvents();
	}
}
