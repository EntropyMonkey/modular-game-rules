using UnityEngine;
using System.Collections;
using System;

public class ChangeCounter : Reaction
{
	public int ChangeBy = -1;
	public string CounterName = "";

	public int MaxValue = 100;
	public int MinValue = 0;

	public bool ShowInGUI = false;

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
			counters = Reactor.GetComponent<Counters>();
			if (counters == null)
				counters = Reactor.gameObject.AddComponent<Counters>();
		}

		counters.AddCounter(CounterName, 0, MinValue, MaxValue, ShowInGUI);
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
		rule.parameters.Add(new Param()
			{
				name = "MaxValue",
				type = MaxValue.GetType(),
				value = MaxValue
			});
		rule.parameters.Add(new Param()
			{
				name = "MinValue",
				type = MinValue.GetType(),
				value = MinValue
			});
		rule.parameters.Add(new Param()
			{
				name = "ShowInGUI",
				type = ShowInGUI.GetType(),
				value = ShowInGUI
			});

		return rule;
	}

	protected override void React(GameEventData eventData)
	{
		counters.ChangeCounter(CounterName, ChangeBy);
	}
}
