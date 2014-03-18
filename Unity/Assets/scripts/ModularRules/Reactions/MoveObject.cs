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

			if (Reactor as IMove != null)
				((IMove)Reactor).Move(eventData, MoveDirection);
		}
	}
}
