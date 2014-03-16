using UnityEngine;
using System.Collections;

using ModularRules;

public class FollowObject : Reaction
{
	void OnEnable()
	{
		ListenedEvent.Register(this);
	}

	void OnDisable()
	{
		ListenedEvent.Unregister(this);
	}

	protected override void React(EventData eventData)
	{

		GameObject target = eventData.Get(EventDataKeys.TargetObject).data as GameObject;

		if (target != null)
		{
			transform.position = target.transform.position;
		}
	}
}
