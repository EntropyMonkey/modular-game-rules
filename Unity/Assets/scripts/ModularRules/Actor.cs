using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public abstract class Actor : MonoBehaviour
	{
		public List<GameEvent> Events;

		public void UpdateEvents()
		{
			foreach (GameEvent e in Events)
				e.Update();
		}
	}
}
