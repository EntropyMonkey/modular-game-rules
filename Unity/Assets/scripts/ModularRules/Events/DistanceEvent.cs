using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public class DistanceEvent : GameEvent
	{
		public float TriggerDistance;

		public Comparison TriggerWhenDistance;

		public Actor WatchedObject;

		private const float threshold = 0.1f;

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
}
