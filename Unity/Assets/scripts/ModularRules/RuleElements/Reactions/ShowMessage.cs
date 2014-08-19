using UnityEngine;
using System.Collections;

public class ShowMessage : Reaction
{
	public string Message = "";
	public int Time;

	private RuleGenerator generator;

	public override BaseRuleElement.RuleData GetRuleInformation()
	{
		BaseRuleElement.RuleData rule = base.GetRuleInformation();

		if (rule.parameters == null)
			rule.parameters = new System.Collections.Generic.List<Param>();

		rule.parameters.Add(new Param()
			{
				name = "Message",
				type = Message.GetType(),
				value = Message
			});

		rule.parameters.Add(new Param()
			{
				name = "Time",
				type = Time.GetType(),
				value = Time
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

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		this.generator = generator;
	}

	protected override void React(GameEventData eventData)
	{
		generator.Gui.ShowMessage(Message, Time);
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("show the message:\"", RuleGUI.ruleLabelStyle);

		Message = RuleGUI.ShowParameter(Message);
		ChangeParameter("Message", ruleData.parameters, Message);

		GUILayout.Label("\" for", RuleGUI.ruleLabelStyle);

		Time = RuleGUI.ShowParameter(Time);
		ChangeParameter("Time", ruleData.parameters, Time);

		GUILayout.Label("seconds on the screen.", RuleGUI.ruleLabelStyle);
	}
}
