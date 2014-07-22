using UnityEngine;
using System.Collections;
using System;

public class ChangeCounter : Reaction
{
	public int ChangeBy = -1;
	public string CounterName = "";

	private Counters counters;

	void OnEnable()
	{
		Register();
	}

	void OnDisable()
	{
		Unregister();
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		if (counters == null)
		{
			counters = FindObjectOfType<Counters>();
			if (counters == null)
			{
				Debug.LogError("Can't find Counters actor in scene. Deactivating " + name + ".");
				gameObject.SetActive(false);
			}
		}
	}

	public override BaseRuleElement.RuleData GetRuleInformation()
	{
		RuleData rule = base.GetRuleInformation();

		if (rule.parameters == null)
			rule.parameters = new System.Collections.Generic.List<Param>();

		rule.parameters.Add(new Param()
			{
				name = "ChangeBy",
				type = ChangeBy.GetType(),
				value = ChangeBy
			});

		rule.parameters.Add(new Param()
			{
				name = "CounterName",
				type = CounterName.GetType(),
				value = CounterName
			});

		return rule;
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("change counter", RuleGUI.ruleLabelStyle);

		CounterName = RuleGUI.ShowParameter(CounterName);
		ChangeParameter("CounterName", (ruleData as ReactionData).parameters, CounterName);

		GUILayout.Label("by", RuleGUI.ruleLabelStyle);

		ChangeBy = RuleGUI.ShowParameter(ChangeBy);
		ChangeParameter("ChangeBy", (ruleData as ReactionData).parameters, ChangeBy);
	}

	protected override void React(GameEventData eventData)
	{
		if (counters != null)
			counters.ChangeCounter(CounterName, ChangeBy);
	}
}
