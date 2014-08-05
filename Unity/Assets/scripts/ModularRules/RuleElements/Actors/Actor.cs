using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Actor : BaseRuleElement
{
	protected List<GameEvent> events;

	protected List<Reaction> reactions;

	private bool pausedEvents = false;
	protected bool Paused
	{
		get
		{
			return pausedEvents;
		}
	}

	private GameObject eventsGO;
	public GameObject Events
	{
		get
		{
			if (eventsGO == null)
			{
				Transform e = transform.FindChild("Events");
				if (e) 
					eventsGO = e.gameObject;
				else
				{
					eventsGO = new GameObject("Events");
					eventsGO.transform.parent = transform;
				}
				// resetting position, just to be sure
				eventsGO.transform.localPosition = Vector3.zero;
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
				Transform r = transform.FindChild("Reactions");
				if (r)
					reactionsGO = r.gameObject;
				else
				{
					reactionsGO = new GameObject("Reactions");
					reactionsGO.transform.parent = transform;
				}
				// resetting position, just to be sure
				reactionsGO.transform.localPosition = Vector3.zero;
			}

			return reactionsGO;
		}
	}

	[HideInInspector]
	public bool WasSpawned = false; // treat spawned actors differently - no storing in the rule files f.ex.

	public RuleGenerator RuleGenerator
	{
		get;
		protected set;
	}

	/// <summary>
	/// Collects all events, so that they can be updated when appropriate
	/// </summary>
	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		RuleGenerator = generator;

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

		if (events != null)
			events.Clear();
		else if (c.Length > 0 && events == null)
			events = new List<GameEvent>();

		foreach (Component co in c)
		{
			GameEvent e = co as GameEvent;
			AddEvent(e);
		}
#if DEBUG
		if (events != null)
			Debug.Log(name + " found " + events.Count + " events.");
#endif
	}

	public void AddEvent(GameEvent gameEvent)
	{
		if (events == null)
			events = new List<GameEvent>();

		if (!events.Contains(gameEvent))
		{
			events.Add(gameEvent);
			gameEvent.transform.parent = Events.transform;
			gameEvent.transform.localPosition = Vector3.zero;
			gameEvent.Actor = this;
#if DEBUG
			Debug.Log(name + " added " + gameEvent.name + ".");
#endif
		}
	}

	public void RemoveEvent(GameEvent gameEvent)
	{
		events.Remove(gameEvent);
	}

	public void InitializeEvents()
	{
		if (events == null) return;

		foreach (GameEvent e in events)
		{
			e.Initialize(RuleGenerator);
		}
	}

	public void ScanReactions()
	{
		Component[] c = GetComponentsInChildren(typeof(Reaction));

		if (reactions != null)
			reactions.Clear();
		else if (c.Length > 0 && reactions == null)
			reactions = new List<Reaction>();

		foreach (Component co in c)
		{
			Reaction r = co as Reaction;
			AddReaction(r);
		}
#if DEBUG
		if (reactions != null)
			Debug.Log(name + " found " + reactions.Count + " reactions.");
#endif
	}

	public void AddReaction(Reaction reaction)
	{
		if (reactions == null)
			reactions = new List<Reaction>();

		if (!reactions.Contains(reaction))
		{
			reactions.Add(reaction);
			reaction.transform.parent = Reactions.transform;
			reaction.transform.localPosition = Vector3.zero;
			reaction.Reactor = this;

#if DEBUG
			Debug.Log(name + " added " + reaction.name + ".");
#endif
		}
	}

	public void RemoveReaction(Reaction reaction)
	{
		reactions.Remove(reaction);
	}

	public void InitializeReactions()
	{
		if (reactions == null) return;

		foreach (Reaction r in reactions)
		{
			r.Initialize(RuleGenerator);
		}
	}

	public override void Reset()
	{
		// destroy all events and reactions belonging to this actor
		if (events != null)
		{
			foreach (GameEvent gameEvent in events)
			{
				RuleGenerator.Unregister(gameEvent);
				Destroy(gameEvent);
			}
			events.Clear();
			Destroy(Events);
		}

		if (reactions != null && reactions.Count > 0)
		{
			foreach (Reaction reaction in reactions)
			{
				reaction.Unregister();
				RuleGenerator.Unregister(reaction);
				Destroy(reaction);
			}
			reactions.Clear();
			Destroy(Reactions);
		}

		base.Reset();
	}

	public override RuleData GetRuleInformation()
	{
		return new ActorData() 
		{ 
			id = Id, 
			type = this.GetType(), 
			label = gameObject.name = Label,
 			prefab = "",
			OnShowGui = ShowGui 
		};
	}

	public void UpdateEvents()
	{
		if (!pausedEvents && events != null)
		{
			foreach (GameEvent e in events)
			{
				if (e != null)
					e.UpdateEvent();
			}
		}
	}

	public override void ShowGui(BaseRuleElement.RuleData ruleData)
	{
		// column 1
		GUILayout.BeginVertical();

		// label
		GUILayout.BeginHorizontal();

		GUILayout.Label("Name: ", RuleGUI.ruleLabelStyle);
		string oldName = Label;
		name = Label = RuleGUI.ShowParameter(Label);
		if (oldName != Label)
			RuleGenerator.Gui.Rename(Id, Label);

		GUILayout.EndHorizontal();

		// type
		GUILayout.BeginHorizontal();
		GUILayout.Label("Type: " + GetType(), RuleGUI.smallLabelStyle);
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();

		// column 2
		RuleGUI.VerticalLine();

		GUILayout.BeginVertical();

		// prefab
		GUILayout.BeginHorizontal();
		GUILayout.Label("Prefab", RuleGUI.ruleLabelStyle);
		(ruleData as ActorData).prefab = RuleGUI.ShowParameter((ruleData as ActorData).prefab);
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
	}

	public void PauseEvents()
	{
		pausedEvents = true;
	}

	public void ExecuteEvents()
	{
		pausedEvents = false;
	}
}
