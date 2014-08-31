using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CounterEvent : GameEvent
{
	public string CounterName;

	public Comparison Compare;

	public float CounterLimit;

	private Counter counter;

	private DropDown comparisonDropDown;

	public override BaseRuleElement.RuleData GetRuleInformation()
	{
		RuleData rule =  base.GetRuleInformation();

		if (rule.parameters == null) rule.parameters = new System.Collections.Generic.List<Param>();

		rule.parameters.Add(new Param()
			{
				name = "CounterName",
				type = CounterName.GetType(),
				value = CounterName
			});

		rule.parameters.Add(new Param()
			{
				name = "Compare",
				type = Compare.GetType(),
				value = Compare
			});

		rule.parameters.Add(new Param()
			{
				name = "CounterLimit",
				type = CounterLimit.GetType(),
				value = CounterLimit
			});

		return rule;
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		comparisonDropDown = new DropDown((int)Compare, System.Enum.GetNames(typeof(Comparison)));
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("When counter", RuleGUI.ruleLabelStyle);

		CounterName = RuleGUI.ShowParameter(CounterName);
		ChangeParameter("CounterName", (ruleData as EventData).parameters, CounterName);
		counter = Counter.Get(CounterName);

		GUILayout.Label("becomes", RuleGUI.ruleLabelStyle);

		int resultIndex = comparisonDropDown.Draw();
		if (resultIndex > -1)
		{
			Compare = (Comparison)resultIndex;
			ChangeParameter("Compare", (ruleData as EventData).parameters, Compare);
		}

		CounterLimit = RuleGUI.ShowParameter(CounterLimit);
		ChangeParameter("CounterLimit", (ruleData as EventData).parameters, CounterLimit);
	}

	public override GameEvent UpdateEvent()
	{
		if (!counter) return null;

		bool conditionsTrue = Compare == Comparison.LESS && counter.Value < CounterLimit;

		conditionsTrue |= Compare == Comparison.EQUAL && Mathf.Abs(counter.Value - CounterLimit) < 0.01f;

		conditionsTrue |= Compare == Comparison.GREATER && counter.Value > CounterLimit;

		if (conditionsTrue)
		{
			Trigger(GameEventData.Empty);
			return this;
		}

		return null;
	}
}
