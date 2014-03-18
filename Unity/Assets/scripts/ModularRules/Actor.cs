using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public abstract class Actor : MonoBehaviour
	{
		protected List<GameEvent> events;

		protected List<Reaction> reactions;

		/// <summary>
		/// Collects all events, so that they can be updated when appropriate
		/// </summary>
		protected void InitializeActor()
		{
			// get all events
			events = new List<GameEvent>();
			Component[] c = GetComponentsInChildren(typeof(GameEvent));
			foreach (Component co in c)
			{
				GameEvent e = co as GameEvent;
				e.Actor = this;
				events.Add(e);
			}
			Debug.Log(name + " registered " + events.Count + " GameEvents.");

			// get all reactions
			reactions = new List<Reaction>();
			c = GetComponentsInChildren(typeof(Reaction));
			foreach (Component co in c)
			{
				Reaction r = co as Reaction;
				r.Reactor = this;
				reactions.Add(r);
			}
			Debug.Log(name + " registered " + reactions.Count + " Reactions.");

		}

		public void UpdateEvents()
		{
			foreach (GameEvent e in events)
				e.UpdateEvent();
		}
	}
}
