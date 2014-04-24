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

		public override void Reset()
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

			// destroy all events and reactions belonging to this actor
			foreach(GameEvent gameEvent in events)
			{
				Destroy(gameEvent);
			}
			events.Clear();
			Destroy(Events);
			foreach (Reaction reaction in reactions)
			{
				reaction.Unregister();
				Destroy(reaction);
			}
			reactions.Clear();
			Destroy(Reactions);

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
			foreach (GameEvent e in events)
			{
				e.UpdateEvent();
			}
		}
	}
}
