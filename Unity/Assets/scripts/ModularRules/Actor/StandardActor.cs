using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StandardActor : Actor
{
	void Update()
	{
		UpdateEvents();
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("Showing a standard actor", RuleGUI.ruleLabelStyle);
	}
}
