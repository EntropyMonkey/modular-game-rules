using UnityEngine;
using System.Collections;
using ModularRules;

namespace ModularRules
{
	public class MoveObject : Reaction
	{
		public enum Direction { FORWARD, BACKWARD, LEFT, RIGHT, UP, DOWN }

		public Direction MoveDirection;

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
			if (eventData == null) return;

			IMove movingObject = Reactor.gameObject.GetComponent(typeof(IMove)) as IMove;
			Debug.Log(movingObject);
			if (movingObject != null)
				movingObject.Move(eventData, MoveDirection);
		}
	}
}
