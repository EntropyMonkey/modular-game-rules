using UnityEngine;
using System.Collections;

public class RaycastHitEvent : GameEvent
{
	public override void ShowGui(RuleData ruleData)
	{
	}

	public override GameEvent UpdateEvent()
	{
		return this;
	}
}
