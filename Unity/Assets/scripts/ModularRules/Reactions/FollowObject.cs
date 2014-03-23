using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class FollowObject : Reaction
	{
		public float FollowSpeed = 10;

		public GameObject FixedToObject = null;
		public Vector3 Offset = Vector3.zero;

		private Vector3 targetPos = Vector3.zero;

		void OnEnable()
		{
			ListenedEvent.Register(this);

			if (FixedToObject)
			{
				transform.parent = FixedToObject.transform;
				transform.localPosition = Offset;
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
