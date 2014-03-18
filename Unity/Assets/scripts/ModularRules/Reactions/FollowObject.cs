using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class FollowObject : Reaction
	{
		public float FollowSpeed = 10;

		public GameObject FixedToObject = null;

		private Vector3 targetPos = Vector3.zero;

		void OnEnable()
		{
			ListenedEvent.Register(this);

			if (FixedToObject)
			{
				transform.parent = FixedToObject.transform;
				transform.localPosition = Vector3.zero;
			}
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
			if (!FixedToObject)
			{
				transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * FollowSpeed);
			}
		}
	}
}
