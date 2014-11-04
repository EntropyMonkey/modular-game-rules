using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DistanceEvent : GameEvent
{
	public float TriggerDistance;

	public Comparison TriggerWhenDistance;

	public Actor WatchedObject;

	private const float threshold = 0.1f;

	private ActorDropDown actorDropDown;
	private ActorDropDown watchedActorDropDown;
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

		generator.OnActorGOChanged += delegate(ActorData data, Actor newActor, RuleGenerator ruleGenerator)
		{
			if (WatchedObject == null || WatchedObject.Id == data.id)
				WatchedObject = newActor;
		};

		string[] actors = generator.Gui.ActorNames;
		actorDropDown = new ActorDropDown(
			System.Array.FindIndex(actors, item => item == Actor.Label), actors,
			ref generator.Gui.OnAddedActor, ref generator.Gui.OnRenamedActor, ref generator.Gui.OnDeletedActor);

		if (WatchedObject == null)
		{
			WatchedObject = generator.GetActor(0); // dont get global actor bec it could be anywhere
		}

		watchedActorDropDown = new ActorDropDown(
			System.Array.FindIndex(actors, item => item == WatchedObject.Label), actors,
			ref generator.Gui.OnAddedActor, ref generator.Gui.OnRenamedActor, ref generator.Gui.OnDeletedActor);

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
			int resultId = generator.Gui.GetActorDataByLabel(actorDropDown.Content[resultIndex].text).id;

			(ruleData as EventData).actorId = resultId;
			if (resultId != Actor.Id)
				generator.ChangeActor(this, resultId);
		}

		GUILayout.Label("and", RuleGUI.ruleLabelStyle);

		resultIndex = watchedActorDropDown.Draw();
		if (resultIndex > -1)
		{
			int resultId = generator.Gui.GetActorDataByLabel(watchedActorDropDown.Content[resultIndex].text).id;

			ChangeParameter("WatchedObject", ruleData.parameters, WatchedObject.Id);
			WatchedObject = generator.GetActor(resultId);
		}

		GUILayout.Label("is", RuleGUI.ruleLabelStyle);

		resultIndex = comparisonDropDown.Draw();
		if (resultIndex > -1)
		{
			TriggerWhenDistance = (Comparison)resultIndex;
			ChangeParameter("TriggerWhenDistance", ruleData.parameters, TriggerWhenDistance);
		}

		TriggerDistance = RuleGUI.ShowParameter(TriggerDistance);
		ChangeParameter("TriggerDistance", ruleData.parameters, TriggerDistance);
	}

	public override GameEvent UpdateEvent()
	{
		if (WatchedObject == null) return null;

		float distance = Vector3.Distance(Actor.transform.position, WatchedObject.transform.position);

		if (TriggerWhenDistance == Comparison.EQUAL &&
			Mathf.Abs(distance - TriggerDistance) < threshold)
		{
			Trigger(GameEventData.Empty
				.Add(new DataPiece(EventDataKeys.TargetObject)
				{
					data = WatchedObject.gameObject
				}));
		}
		else if (TriggerWhenDistance == Comparison.LESS &&
			distance < TriggerDistance)
		{
			Trigger(GameEventData.Empty
				.Add(new DataPiece(EventDataKeys.TargetObject)
				{
					data = WatchedObject.gameObject
				}));
		}
		else if (TriggerWhenDistance == Comparison.GREATER &&
			distance > TriggerDistance)
		{
			Trigger(GameEventData.Empty
				.Add(new DataPiece(EventDataKeys.TargetObject)
				{
					data = WatchedObject.gameObject
				}));
		}

		return this;
	}
}
