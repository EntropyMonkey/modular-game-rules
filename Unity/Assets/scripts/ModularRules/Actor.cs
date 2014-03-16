using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public abstract class Actor : MonoBehaviour
	{
		protected List<GameEvent> events;

		/// <summary>
		/// Collects all events, so that they can be updated when appropriate
		/// </summary>
		protected void InitializeActor()
		{
			events = new List<GameEvent>();

			GameEvent newEvent;
			for (int i = 0; i < transform.childCount; i++)
			{
				if ((newEvent = transform.GetChild(i).GetComponent(typeof(GameEvent)) as GameEvent) != null)
				{
					Debug.Log("Adding event " + newEvent.name + " to " + name);
					events.Add(newEvent);
				}
			}
		}

		public void UpdateEvents()
		{
			foreach (GameEvent e in events)
				e.UpdateEvent();
		}
	}
}
