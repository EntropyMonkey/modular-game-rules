using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public class FollowObject : Reaction
	{
		public float FollowSpeed = 10;

		public GameObject FixedToObject = null;
		public Vector3 Offset = Vector3.zero;

		private Vector3 targetPos = Vector3.zero;

		public override void Initialize()
		{
			base.Initialize();

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
				targetPos = target.transform.position;
			}
		}

		void FixedUpdate()
		{
			if (!FixedToObject)
			{
				Reactor.transform.position = Vector3.Lerp(Reactor.transform.position, targetPos + Offset, Time.deltaTime * FollowSpeed);
				Reactor.transform.rotation = Quaternion.Lerp(Reactor.transform.rotation, Quaternion.LookRotation(targetPos - Reactor.transform.position), Time.deltaTime * FollowSpeed);
			}
		}
	}
}
