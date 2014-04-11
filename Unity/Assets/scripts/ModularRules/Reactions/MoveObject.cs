using UnityEngine;
using System.Collections;
using ModularRules;

namespace ModularRules
{
	public enum Direction { FORWARD, BACKWARD, LEFT, RIGHT, UP, DOWN }

	public class MoveObject : Reaction
	{
		public Direction MoveDirection;

		void OnEnable()
		{
			Register();
		}

		void OnDisable()
		{
			Unregister();
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
