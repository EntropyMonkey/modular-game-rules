using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public abstract class MovementBehaviour : SwitchableBehaviour
	{
		/// <summary>
		/// called periodically
		/// </summary>
		/// <param name="data">The data of the event which has triggered the move</param>
		/// <param name="direction">The subjective direction in which to move</param>
		public abstract void Move(EventData data, Direction direction);
	}
}
