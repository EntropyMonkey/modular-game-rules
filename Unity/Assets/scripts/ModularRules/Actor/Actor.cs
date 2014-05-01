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

		private GenerateElement generateElement;

		/// <summary>
		/// Collects all events, so that they can be updated when appropriate
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			generateElement = gameObject.GetComponent<GenerateElement>();

			// get all events
			ScanEvents();
			InitializeEvents();

			// get all reactions
			ScanReactions();
			InitializeReactions();
		}

		public void ScanEvents()
		{
			Component[] c = GetComponentsInChildren(typeof(GameEvent));

			if (c.Length > 0 && events == null)
				events = new List<GameEvent>();

			foreach (Component co in c)
			{
				GameEvent e = co as GameEvent;
				if (!events.Contains(e))
				{
					events.Add(e);
				}
			}
#if DEBUG
			if (events != null)
				Debug.Log(name + " registered " + events.Count + " GameEvents.");
#endif
		}

		void InitializeEvents()
		{
			foreach (GameEvent e in events)
			{
				e.Actor = this;
				e.Initialize();
			}
		}

		public void ScanReactions()
		{
			Component[] c = GetComponentsInChildren(typeof(Reaction));
			
			if (c.Length > 0 && reactions == null)
				reactions = new List<Reaction>();

			foreach (Component co in c)
			{
				Reaction r = co as Reaction;
				if (!reactions.Contains(r))
				{
					reactions.Add(r);
				}
			}
#if DEBUG
			if (reactions != null)
				Debug.Log(name + " registered " + reactions.Count + " Reactions.");
#endif
		}

		void InitializeReactions()
		{
			foreach (Reaction r in reactions)
			{
				r.Reactor = this;
				r.Initialize();
			}
		}

		public override void Reset()
		{
			if (generateElement && generateElement.OriginalComponents != null)
			{
				// reset components to original set
				List<Component> newComps = new List<Component>();
				newComps.AddRange(GetComponents(typeof(Component)));

				foreach (Component c in newComps)
				{
					if (!generateElement.OriginalComponents.Contains(c))
					{
						Destroy(c);
					}
				}
				newComps.Clear();
			}

			// destroy all events and reactions belonging to this actor
			if (events != null)
			{
				foreach (GameEvent gameEvent in events)
				{
					Destroy(gameEvent);
				}
				events.Clear();
				Destroy(Events);
			}

			if (reactions != null)
			{
				foreach (Reaction reaction in reactions)
				{
					reaction.Unregister();
					Destroy(reaction);
				}
				reactions.Clear();
				Destroy(Reactions);
			}

			// enable placeholder
			GetComponent<GenerateElement>().enabled = true;

			base.Reset();
		}

		public override RuleData GetRuleInformation()
		{
			return new ActorData() { id = Id, type = this.GetType(), label = gameObject.name };
		}

		public void UpdateEvents()
		{
			if (events != null)
			{
				foreach (GameEvent e in events)
				{
					if (e != null)
						e.UpdateEvent();
				}
			}
		}
	}
}
