using UnityEngine;
using System.Collections;

public class DebugPrint : Reaction
{
	public string logText = "";

	public override BaseRuleElement.RuleData GetRuleInformation()
	{
		BaseRuleElement.RuleData rule = base.GetRuleInformation();

		if (rule.parameters == null)
			rule.parameters = new System.Collections.Generic.List<Param>();

		rule.parameters.Add(new Param()
			{
				name = "logText",
				type = logText.GetType(),
				value = logText
			});

		return rule;
	}

	void OnEnable()
	{
		Register();
	}

	void OnDisable()
	{
		Unregister();
	}

	protected override void React(GameEventData eventData)
	{
		Debug.Log(logText);
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("print", RuleGUI.ruleLabelStyle);

		logText = RuleGUI.ShowParameter(logText);
		ChangeParameter("logText", (ruleData as ReactionData).parameters, logText);

		GUILayout.Label("to the console", RuleGUI.ruleLabelStyle);
	}
}
