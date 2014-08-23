using UnityEngine;
using System.Collections;
using System;

public class ChangeCounter : Reaction
{
	public enum Change { OVER_TIME, INSTANTLY };

	public int ChangeBy = -1;
	public int PerSeconds = 1;
	public string CounterName = "";
	public Change KindOfChange;

	private DropDown kindOfChangeDropDown;

	private Counter counter;

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

		if (counter == null)
		{
			counter = Counter.Get(CounterName);
		}

		kindOfChangeDropDown = new DropDown((int)KindOfChange, System.Enum.GetNames(typeof(Change)));
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
				name = "KindOfChange",
				type = KindOfChange.GetType(),
				value = KindOfChange
			});

		rule.parameters.Add(new Param()
			{
				name = "PerSeconds",
				type = PerSeconds.GetType(),
				value = PerSeconds
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
		counter = Counter.Get(CounterName);

		GUILayout.Label("by", RuleGUI.ruleLabelStyle);

		ChangeBy = RuleGUI.ShowParameter(ChangeBy);
		ChangeParameter("ChangeBy", (ruleData as ReactionData).parameters, ChangeBy);

		KindOfChange = (Change)kindOfChangeDropDown.Draw();
		ChangeParameter("KindOfChange", ruleData.parameters, KindOfChange);

		if (KindOfChange == Change.OVER_TIME)
		{
			GUILayout.Label("every", RuleGUI.ruleLabelStyle);

			PerSeconds = RuleGUI.ShowParameter(PerSeconds);
			ChangeParameter("PerSeconds", (ruleData as ReactionData).parameters, PerSeconds);

			GUILayout.Label("second(s).", RuleGUI.ruleLabelStyle);
		}
	}

	protected override void React(GameEventData eventData)
	{

		if (counter != null)
		{
			if (KindOfChange == Change.OVER_TIME)
			{
				float s = PerSeconds;
				if (PerSeconds == 0)
					s = 0.1f;
				counter.ChangeBy(ChangeBy * Time.deltaTime / s);
			}
			else if (KindOfChange == Change.INSTANTLY)
			{
				counter.ChangeBy(ChangeBy);
			}
		}
		else
		{
			// hack as workaround for possibly wrong initialization order
			counter = Counter.Get(CounterName);
		}
	}
}
