using UnityEngine;
using System.Collections;

using ModularRules;

public class FollowObject : Reaction
{
	protected override void React(EventData eventData = null)
	{
		if (eventData != null)
		{
			GameObject target = eventData.Get(EventDataKeys.TargetObject).data as GameObject;
			if (target != null)
				transform.position = target.transform.position;
		}
	}
}
