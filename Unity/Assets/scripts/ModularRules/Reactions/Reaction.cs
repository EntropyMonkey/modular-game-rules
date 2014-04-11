using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	// extends MonoBehaviour because children can then have fields in unity editor
	public abstract class Reaction : BaseRuleElement
	{
		public GameEvent ListenedEvent;

		[HideInInspector]
		public Actor Reactor;

		protected List<Reaction> reactionComponents = new List<Reaction>();

		public override void Initialize()
		{
			base.Initialize();

			Register();
		}

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


		#region Register/Unregister with events
		protected virtual void Register()
		{
			if (ListenedEvent != null)
				ListenedEvent.Register(this);
		}

		protected virtual void Unregister()
		{
			if (ListenedEvent != null)
				ListenedEvent.Unregister(this);
		}
		#endregion
	}
}
