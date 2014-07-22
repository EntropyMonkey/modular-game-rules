using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeactivateObject : Reaction
{
	public Actor ObjectToDeactivate;

	public float Timeout;

	private bool deactivating = false;

	private DropDown actorDropDown;

	private RuleGenerator generator;

	public override RuleData GetRuleInformation()
	{
		RuleData data = base.GetRuleInformation();

		if (data.parameters == null) data.parameters = new List<Param>();

		if (ObjectToDeactivate != null)
		{
			data.parameters.Add(new Param()
			{
				name = "ObjectToDeactivate",
				type = ObjectToDeactivate.GetType(),
				value = ObjectToDeactivate.Id
			});
		}

		data.parameters.Add(new Param()
		{
			name = "Timeout",
			type = Timeout.GetType(),
			value = Timeout
		});

		return data;
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		this.generator = generator;

		if (ObjectToDeactivate == null)
		{
			ObjectToDeactivate = Reactor;
		}

		actorDropDown = new DropDown(System.Array.FindIndex<string>(generator.ActorNames, item => item == Reactor.Label), generator.ActorNames);
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("deactivate", RuleGUI.ruleLabelStyle);

		int resultIndex = actorDropDown.Draw();
		if (resultIndex > -1)
		{
			int resultId = generator.ActorData.Find(item => item.label == actorDropDown.Content[resultIndex].text).id;
			(ruleData as ReactionData).actorId = resultId;
			ChangeParameter("ObjectToDeactivate", (ruleData as ReactionData).parameters, resultId);
			generator.ChangeActor(this, resultId);
			ObjectToDeactivate = Reactor;
		}

		GUILayout.Label("after", RuleGUI.ruleLabelStyle);

		Timeout = RuleGUI.ShowParameter(Timeout);
		ChangeParameter("Timeout", (ruleData as ReactionData).parameters, Timeout);

		GUILayout.Label("seconds", RuleGUI.ruleLabelStyle);
	}

	protected override void React(GameEventData eventData)
	{
		if (!deactivating)
			StartCoroutine(DeactivateAfter(Timeout));
		deactivating = true;
	}

	IEnumerator DeactivateAfter(float t)
	{
		yield return new WaitForSeconds(t);
		ObjectToDeactivate.gameObject.SetActive(false);
		deactivating = false;
	}
}
