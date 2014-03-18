using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class RaycastHitEvent : GameEvent
	{
		public override GameEvent UpdateEvent()
		{
			return this;
		}
	}
}
