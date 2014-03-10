using UnityEngine;
using System.Collections;

using ModularRules;

public class ObjectMovedEvent : GameEvent
{
	const float minDistance = 0.01f;
	Vector3 position;

	void Start()
	{
		position = transform.position;
	}

	public override void Update() 
	{
		if (Vector3.Distance(position, transform.position) > minDistance)
		{
			Trigger(new EventData()
				.Add(new DataPiece(EventDataKeys.TargetObject) { data = gameObject }));
		}
	}
}
