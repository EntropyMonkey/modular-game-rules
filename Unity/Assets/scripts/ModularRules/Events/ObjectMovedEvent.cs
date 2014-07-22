using UnityEngine;
using System.Collections;

public class ObjectMovedEvent : GameEvent
{
	const float minDistance = 0.1f;
	Vector3 position;

	DropDown actorDropDown;

	RuleGenerator generator;

	void Start()
	{
		position = Actor.transform.position;
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		this.generator = generator;

		actorDropDown = new DropDown(
			System.Array.FindIndex<string>(generator.ActorNames, 0, generator.ActorNames.Length, item => item == Actor.Label),
			generator.ActorNames);
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("When", RuleGUI.ruleLabelStyle);

		int resultIndex = actorDropDown.Draw();
		if (resultIndex > -1)
		{
			int resultId = generator.ActorData.Find(item => item.label == actorDropDown.Content[resultIndex].text).id;
			(ruleData as EventData).actorId = resultId;
			generator.ChangeActor(this, resultId);
		}

		GUILayout.Label("moves,", RuleGUI.ruleLabelStyle);
	}

	public override GameEvent UpdateEvent()
	{
		if (transform == null) return this;

		if (Vector3.Distance(position, Actor.transform.position) > minDistance)
		{
			Trigger(new GameEventData()
				.Add(new DataPiece(EventDataKeys.TargetObject) { data = gameObject }));

			position = Actor.transform.position;
		}

		return this;
	}
}
