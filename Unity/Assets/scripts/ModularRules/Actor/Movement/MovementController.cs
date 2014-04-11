using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class MovementController : BehaviourController<MovementBehaviour>, IMove
	{
		public void Move(EventData data, Direction dir)
		{
			currentBehaviour.Move(data, dir);
		}
	}
}
