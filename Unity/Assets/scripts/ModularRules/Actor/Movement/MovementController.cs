using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class MovementController : BehaviourController<MovementBehaviour>, IMove
	{
		public void Move(EventData data, MoveObject.Direction dir)
		{
			currentBehaviour.Move(data, dir);
		}
	}
}
