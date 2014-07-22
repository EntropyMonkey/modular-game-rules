using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DistanceEvent : GameEvent
{
	public float TriggerDistance;

	public Comparison TriggerWhenDistance;

	public Actor WatchedObject;

	private const float threshold = 0.1f;

	private DropDown actorDropDown;
	private DropDown watchedActorDropDown;
	private DropDown comparisonDropDown;

	private RuleGenerator generator;

	public override RuleData GetRuleInformation()
	{
		RuleData data = base.GetRuleInformation();

		if (data.parameters == null)
			data.parameters = new List<Param>();

		data.parameters.Add(new Param()
		{
			name = "TriggerDistance",
			type = TriggerDistance.GetType(),
			value = TriggerDistance
		});

		data.parameters.Add(new Param()
		{
			name = "TriggerWhenDistance",
			type = TriggerWhenDistance.GetType(),
			value = TriggerWhenDistance
		});

		if (WatchedObject) 
		{
			data.parameters.Add(new Param()
			{
				name = "WatchedObject",
				type = WatchedObject.GetType(),
				value = WatchedObject.Id
			});
		}

		return data;
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		this.generator = generator;

		actorDropDown = new DropDown(
			System.Array.FindIndex<string>(generator.ActorNames, 0, generator.ActorNames.Length, item => item == Actor.Label), 
			generator.ActorNames);
		watchedActorDropDown = new DropDown(
			generator.ActorData.FindIndex(item => item.id == WatchedObject.Id), 
			generator.ActorNames);

		comparisonDropDown = new DropDown(
			(int)TriggerWhenDistance,
			System.Enum.GetNames(typeof(Comparison)));
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("When distance of", RuleGUI.ruleLabelStyle);

		int resultIndex = actorDropDown.Draw();
		if (resultIndex > -1)
		{
			int resultId = generator.ActorData.Find(item => item.label == actorDropDown.Content[resultIndex].text).id;

			(ruleData as EventData).actorId = resultId;
			generator.ChangeActor(this, resultId);
		}

		GUILayout.Label("and", RuleGUI.ruleLabelStyle);

		resultIndex = watchedActorDropDown.Draw();
		if (resultIndex > -1)
		{
			int resultId = generator.ActorData.Find(item => item.label == watchedActorDropDown.Content[resultIndex].text).id;

			ChangeParameter("WatchedObject", (ruleData as EventData).parameters, WatchedObject);
			WatchedObject = generator.GetActor(resultId);
		}

		GUILayout.Label("is", RuleGUI.ruleLabelStyle);

		resultIndex = comparisonDropDown.Draw();
		if (resultIndex > -1)
		{
			TriggerWhenDistance = (Comparison)resultIndex;
			ChangeParameter("TriggerWhenDistance", (ruleData as EventData).parameters, TriggerWhenDistance);
		}

		TriggerDistance = RuleGUI.ShowParameter(TriggerDistance);
		ChangeParameter("TriggerDistance", (ruleData as EventData).parameters, TriggerDistance);
	}

	public override GameEvent UpdateEvent()
	{
		if (WatchedObject == null) return null;

		float distance = Vector3.Distance(Actor.transform.position, WatchedObject.transform.position);

		if (TriggerWhenDistance == Comparison.EQUAL &&
			Mathf.Abs(distance - TriggerDistance) < threshold)
		{
			Trigger(GameEventData.Empty);
		}
		else if (TriggerWhenDistance == Comparison.LESS &&
			distance < TriggerDistance)
		{
			Trigger(GameEventData.Empty);
		}
		else if (TriggerWhenDistance == Comparison.GREATER &&
			distance > TriggerDistance)
		{
			Trigger(GameEventData.Empty);
		}

		return this;
	}
}
