using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// extends MonoBehaviour because children can then have fields in unity editor
public abstract class Reaction : BaseRuleElement
{
	public GameEvent ListenedEvent;

	[HideInInspector]
	public Actor Reactor;

	protected List<Reaction> reactionComponents = new List<Reaction>();

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		Register();
	}

	public override RuleData GetRuleInformation()
	{
		if (Reactor && ListenedEvent)
		{
			ReactionData rule = new ReactionData()
			{
				id = Id,
				label = gameObject.name, 
				actorId = Reactor.Id,
				eventId = ListenedEvent.Id,
				type = this.GetType()
			};

			return rule;
		}
		return null;
	}

	/// <summary>
	/// execute all sub reactions in depth first order
	/// </summary>
	/// <param name="eventData">the event's data. possibly null</param>
	public void Execute(GameEventData eventData)
	{
		foreach (Reaction r in reactionComponents)
			r.Execute(eventData);

		React(eventData);
	}

	/// <summary>
	/// The actual reaction to the registered event.
	/// </summary>
	/// <param name="eventData">the event's data</param>
	protected abstract void React(GameEventData eventData);


	#region Register/Unregister with events
	public virtual void Register()
	{
		if (ListenedEvent != null)
			ListenedEvent.Register(this);
	}

	public virtual void Unregister()
	{
		if (ListenedEvent != null)
			ListenedEvent.Unregister(this);
	}
	#endregion
}
