using UnityEngine;
using System.Collections;

public class RespawnObject : Reaction
{
	public Actor TargetObject;

	private ActorDropDown actorDropdown;

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		string selectedName = "";
		if (TargetObject != null)
			selectedName = TargetObject.Label;
		actorDropdown = new ActorDropDown(
			System.Array.FindIndex(generator.Gui.ActorNames, item => item == selectedName),
			generator.Gui.ActorNames,
			ref generator.Gui.OnAddedActor, ref generator.Gui.OnRenamedActor, ref generator.Gui.OnDeletedActor);
	}

	public override RuleData GetRuleInformation()
	{
		ReactionData rule = base.GetRuleInformation() as ReactionData;

		if (rule.parameters == null)
			rule.parameters = new System.Collections.Generic.List<Param>();

		rule.parameters.Add(new Param()
			{
				name = "TargetObject",
				type = TargetObject.GetType(),
				value = TargetObject.Id
			});

		return rule;
	}

	protected override void React(GameEventData eventData)
	{
		if (TargetObject != null)
		{
			TargetObject.Respawn();
		}
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("respawn", RuleGUI.ruleLabelStyle);

		int index = actorDropdown.Draw();
		if (index > -1)
		{
			RuleGenerator generator = FindObjectOfType<RuleGenerator>();
			int resultId = generator.Gui.GetActorDataByLabel(actorDropdown.Content[index].text).id;
			TargetObject = generator.GetActor(resultId);
			generator.ChangeActor(this, resultId);
			ChangeParameter("TargetObject", ruleData.parameters, TargetObject);
		}
	}
}
