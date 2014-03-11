using UnityEngine;
using System.Collections;

using ModularRules;

public class FollowObject : Reaction
{
	[SerializeField]
	private GameObject objectToMove;

	void OnEnable()
	{
		if (objectToMove == null) objectToMove = transform.parent.gameObject;

		ListenedEvent.Register(this);
	}

	void OnDisable()
	{
		ListenedEvent.Unregister(this);
	}

	protected override void React(EventData eventData)
	{
		GameObject target = eventData.Get(EventDataKeys.TargetObject).data as GameObject;
		Debug.Log(eventData.Get(EventDataKeys.TargetObject).dataType);
		if (target != null)
		{
			objectToMove.transform.position = target.transform.position;
		}
	}
}
