using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public class CollisionEvent : GameEvent
	{
		public override GameEvent UpdateEvent()
		{
			return this;
		}
	}
}
