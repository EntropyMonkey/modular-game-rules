using UnityEngine;
using System.Collections;
using ModularRules;

namespace ModularRules
{
	public class MoveObject : Reaction
	{
		public enum Direction { FORWARD, BACKWARD, LEFT, RIGHT, UP, DOWN }

		[SerializeField]
		private Direction MoveDirection;

		[SerializeField]
		private Actor reactor;

		void OnEnable()
		{
			if (reactor == null) reactor = transform.parent.gameObject.GetComponent<Actor>();

			ListenedEvent.Register(this);
		}

		void OnDisable()
		{
			ListenedEvent.Unregister(this);
		}

		protected override void React(EventData eventData)
		{
			float value = (float)eventData.Get(EventDataKeys.InputValue).data;

			if (reactor is IMovable)
				((IMovable)reactor).Move(eventData, MoveDirection);
		}
	}
}
