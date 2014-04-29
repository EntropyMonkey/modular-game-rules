using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class ObjectMovedEvent : GameEvent
	{
		const float minDistance = 0.1f;
		Vector3 position;

		void Start()
		{
			position = transform.position;
		}

		public override GameEvent UpdateEvent()
		{
			if (transform == null) return this;

			if (Vector3.Distance(position, transform.position) > minDistance)
			{
				Trigger(new GameEventData()
					.Add(new DataPiece(EventDataKeys.TargetObject) { data = gameObject }));

				position = transform.position;
			}

			return this;
		}
	}
}
