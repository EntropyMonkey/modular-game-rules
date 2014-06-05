using UnityEngine;
using System.Collections;

public class ObjectMovedEvent : GameEvent
{
	const float minDistance = 0.1f;
	Vector3 position;

	void Start()
	{
		position = Actor.transform.position;
	}

	public override void ShowGui()
	{
		GUILayout.Label("Object moved", RuleGUI.ruleLabelStyle);
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
