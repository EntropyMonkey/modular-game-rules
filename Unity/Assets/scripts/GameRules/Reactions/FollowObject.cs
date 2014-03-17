using UnityEngine;
using System.Collections;

using ModularRules;

public class FollowObject : Reaction
{
	public float FollowSpeed = 10;


	private Vector3 targetPos = Vector3.zero;

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
			targetPos = target.transform.position;
		}
	}

	void Update()
	{
		transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * FollowSpeed);
	}
}
