using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : Actor
{
	public static string Tag = "Level";

	void Start()
	{
		tag = Tag;
	}

	public override BaseRuleElement.RuleData GetRuleInformation()
	{
		BaseRuleElement.ActorData actorData = base.GetRuleInformation() as BaseRuleElement.ActorData;

		actorData.prefab = "Level1";

		return actorData;
	}

	public override void ShowGui(RuleData ruleData)
	{
		base.ShowGui(ruleData);
	}

	void Update()
	{
		UpdateEvents();
	}
}
