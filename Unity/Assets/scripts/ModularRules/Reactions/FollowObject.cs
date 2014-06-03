using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowObject : Reaction
{
	public float FollowSpeed = 10;

	public GameObject FixedToObject = null;
	public Vector3 Offset = Vector3.zero;

	public bool StayBehindObject = true;

	private Transform targetTransform;

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		if (FixedToObject)
		{
			Reactor.transform.parent = FixedToObject.transform;
			Reactor.transform.localPosition = Offset;
		}
	}

	public override RuleData GetRuleInformation()
	{
		ReactionData rule = base.GetRuleInformation() as ReactionData;

		rule.parameters = new List<Param>();
		rule.parameters.Add(new Param()
		{
			name = "FollowSpeed",
			type = FollowSpeed.GetType(),
			value = FollowSpeed
		});
		rule.parameters.Add(new Param() 
		{ 
			name = "Offset",
			type = Offset.GetType(),
			value = Offset.x + " " + Offset.y + " " + Offset.z
		});
		if (FixedToObject != null)
		{
			rule.parameters.Add(new Param()
			{
				name = "FixedToObject",
				type = FixedToObject.GetComponent(typeof(Actor)).GetType(),
				value = (FixedToObject.GetComponent(typeof(Actor)) as Actor).Id
			});
		}
		else
		{
			rule.parameters.Add(new Param()
			{
				name = "FixedToObject",
				type = typeof(Actor),
				value = -1
			});

		}
		rule.parameters.Add(new Param()
		{
			name = "StayBehindObject",
			type = StayBehindObject.GetType(),
			value = StayBehindObject
		});

		return rule;
	}

	void OnEnable()
	{
		Register();
	}

	void OnDisable()
	{
		Unregister();
	}

	protected override void React(GameEventData eventData)
	{
		GameObject target = eventData.Get(EventDataKeys.TargetObject).data as GameObject;

		if (target != null)
		{
			targetTransform = target.transform;
		}
	}

	void FixedUpdate()
	{
		if (!FixedToObject && targetTransform != null)
		{
			if (!StayBehindObject)
			{
				Reactor.transform.position = Vector3.Lerp(Reactor.transform.position, targetTransform.position + Offset, Time.deltaTime * FollowSpeed);
			}
			else
			{
				Reactor.transform.position = Vector3.Lerp(
					Reactor.transform.position, 
					targetTransform.position + targetTransform.forward * Offset.z + targetTransform.right * Offset.x + targetTransform.up * Offset.y, 
					Time.deltaTime * FollowSpeed);
			}

			//Reactor.transform.rotation = Quaternion.Lerp(
			//	Reactor.transform.rotation, 
			//	Quaternion.LookRotation(targetTransform.position - Reactor.transform.position), 
			//	Time.deltaTime * FollowSpeed);

			Reactor.transform.LookAt(targetTransform.position);
		}
	}
}
