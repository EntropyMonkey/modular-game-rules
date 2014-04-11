using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public abstract class Actor : BaseRuleElement
	{
		protected List<GameEvent> events;

		protected List<Reaction> reactions;

		private GameObject eventsGO;
		public GameObject Events
		{
			get
			{
				if (eventsGO == null)
				{
					eventsGO = new GameObject("Events");
					eventsGO.transform.parent = transform;
				}

				return eventsGO;
			}
		}

		private GameObject reactionsGO;
		public GameObject Reactions
		{
			get
			{
				if (reactionsGO == null)
				{
					reactionsGO = new GameObject("Reactions");
					reactionsGO.transform.parent = transform;
				}

				return reactionsGO;
			}
		}

		/// <summary>
		/// Collects all events, so that they can be updated when appropriate
		/// </summary>
		public override void Initialize()
		{
			// get all events
			events = new List<GameEvent>();
			Component[] c = GetComponentsInChildren(typeof(GameEvent));
			foreach (Component co in c)
			{
				GameEvent e = co as GameEvent;
				e.Actor = this;
				events.Add(e);
				e.Initialize();
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
				r.Initialize();
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
