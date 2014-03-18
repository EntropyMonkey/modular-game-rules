using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	// extends MonoBehaviour because children can then have fields in unity editor
	public abstract class Reaction : MonoBehaviour
	{
		public GameEvent ListenedEvent;

		[HideInInspector]
		public Actor Reactor;

		protected List<Reaction> reactionComponents = new List<Reaction>();

		/// <summary>
		/// execute all sub reactions in depth first order
		/// </summary>
		/// <param name="eventData">the event's data. possibly null</param>
		public void Execute(EventData eventData)
		{
			foreach (Reaction r in reactionComponents)
				r.Execute(eventData);

			React(eventData);
		}

		/// <summary>
		/// The actual reaction to the registered event.
		/// </summary>
		/// <param name="eventData">the event's data</param>
		protected abstract void React(EventData eventData);

		#region Event Registering/Unregistering
		/// <summary>
		/// Add this reaction to an event.
		/// </summary>
		public void AddToEvent(GameEvent e)
		{
			e.Register(this);
		}

		/// <summary>
		/// Removes this reaction from an event.
		/// </summary>
		public void RemoveFromEvent(GameEvent e)
		{
			e.Unregister(this);
		}
		#endregion
	}
}
