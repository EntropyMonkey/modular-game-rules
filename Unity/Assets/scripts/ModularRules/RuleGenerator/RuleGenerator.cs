#define DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RuleGenerator : MonoBehaviour
{
	public static string Tag = "RuleGenerator";
	public RuleGUI Gui
	{
		get;
		private set;
	}

	RuleParserLinq ruleParser;

	public string CurrentRuleFileName
	{
		get;
		private set;
	}

	public delegate void GeneratedLevel(List<BaseRuleElement.ActorData> actorData, 
		List<BaseRuleElement.EventData> eventData, 
		List<BaseRuleElement.ReactionData> reactionData,
		string filename); // fired when the parser is done with parsing
	public GeneratedLevel OnGeneratedLevel;

	// rule data collection
	public List<BaseRuleElement.ActorData> ActorData = new List<BaseRuleElement.ActorData>();
	public List<BaseRuleElement.EventData> EventData = new List<BaseRuleElement.EventData>();
	public List<BaseRuleElement.ReactionData> ReactionData = new List<BaseRuleElement.ReactionData>();

	// contains all placeholders for actors in the scene, in order of their Id
	//List<PlaceholderElement> placeholders;

	// keeping track of generated elements
	List<Actor> genActors = new List<Actor>();
	List<GameEvent> genEvents = new List<GameEvent>();
	List<Reaction> genReactions = new List<Reaction>();

	List<BaseRuleElement> unusedElements = new List<BaseRuleElement>(); // elements in this list are deleted after the parser is done generating

	void Awake()
	{
		tag = Tag;
		if ((ruleParser = gameObject.GetComponent<RuleParserLinq>()) == null)
			ruleParser = gameObject.AddComponent<RuleParserLinq>();

		Gui = FindObjectOfType<RuleGUI>();
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
			Application.Quit();

		string filepath = Application.dataPath + @"/Rules/";

		if (Directory.Exists(filepath))
		{
			Gui.Files = Directory.GetFiles(filepath, "*.xml");
		}
		else
		{
			Debug.LogError("There are no rules in " + filepath + ".");
		}
	}

	#region Pausing/Starting Event Execution
	public void PauseEventExecution()
	{
		foreach (Actor a in genActors)
		{
			a.PauseEvents();
		}

		GlobalActor.Instance.PauseEvents();
	}

	public void StartEventExecution()
	{
		foreach (Actor a in genActors)
		{
			a.ExecuteEvents();
		}

		GlobalActor.Instance.ExecuteEvents();
	}
	#endregion

	#region Adding Parsed Rule Data (Interface for Parser)
	void ClearData()
	{
		ActorData.Clear();
		//ActorNames.Clear();
		EventData.Clear();
		ReactionData.Clear();
	}

	public void AddRuleData(BaseRuleElement.ActorData actorData)
	{
		if (ActorData.Find(item => item.id == actorData.id) == null)
		{
			ActorData.Add(actorData.DeepCopy());
			//ActorNames.Add(actorData.id, actorData.label);
		}
	}

	public void AddRuleData(BaseRuleElement.EventData eventData)
	{
		if (EventData.Find(item => item.id == eventData.id) == null)
		{
			EventData.Add(eventData.DeepCopy());
		}
	}

	public void AddRuleData(BaseRuleElement.ReactionData reactionData)
	{
		if (ReactionData.Find(item => item.id == reactionData.id) == null)
		{
			ReactionData.Add(reactionData.DeepCopy());
		}
	}
	#endregion

	#region ID Handling
	public int GetEventId()
	{
		return GetId(genEvents.ToArray());
	}

	public int GetReactionId()
	{
		return GetId(genReactions.ToArray());
	}

	public int GetActorId()
	{
		int result = -1;

		int[] ids = new int[genActors.Count + ActorData.Count]; // can contain duplicates, but doesn't matter
		for (int i = 0; i < genActors.Count + ActorData.Count; i++)
		{
			if (i < genActors.Count)
			{
				ids[i] = genActors[i].Id;
			}
			else
			{
				ids[i] = ActorData[i - genActors.Count].id;
			}
		}

		Array.Sort<int>(ids);
		result = ids[ids.Length - 1] + 1;

		return result;
	}

	int GetId(BaseRuleElement[] genElements)
	{
		int highestId = 0;
		int scndHighestId = 0;
		for (int i = 0; i < genElements.Length; i++)
		{
			if (genElements[i].Id > highestId) // keep track of highest id (so far)
			{
				scndHighestId = highestId;
				highestId = genElements[i].Id;
			}
		}

		if (highestId - scndHighestId > 1) // if there's an unused id between two others, use it, stop the loop
		{
			return scndHighestId + 1;
		}

		return highestId + 1;
	}

	bool IdExists(BaseRuleElement[] genElements, int id)
	{
		return Array.Find<BaseRuleElement>(genElements, item => item.Id == id) != null;
	}
	#endregion

	#region Adding Elements to the scene

	public void AddActorToScene(BaseRuleElement.ActorData data)
	{
#if DEBUG
		Debug.Log("Adding actor " + data.id + ", " + data.type);
#endif
		Actor a = genActors.Find(item => item.Id == data.id);
		if (a != null)
		{
			UpdateActor(data, a);
		}
		else
		{
			GameObject prefabGo = Resources.Load(data.prefab) as GameObject;
			if (prefabGo == null)
				prefabGo = Resources.Load("FallbackPrefab") as GameObject;

			GameObject actorGo = Instantiate(prefabGo, Vector3.zero, Quaternion.identity) as GameObject;
			
			actorGo.name = data.label;
			actorGo.transform.position = Vector3.zero;

			// create new actor
			Actor actor = actorGo.AddComponent(data.type) as Actor;
			actor.Id = data.id;
			actor.Label = data.label;

			data.OnShowGui = actor.ShowGui;

			RegisterRuleElement(actor);

			SetParameters(actor, data);

			// not needed right now
			//SetComponentParameters(actor, data);
		}
	}

	public void AddEventToScene(BaseRuleElement.EventData data)
	{
#if DEBUG
		Debug.Log("Adding event " + data.label + "(" + data.id + ")" + " to actor " + data.actorId + ". Setting " + data.parameters.Count + " parameters.");
#endif

		Actor actor = GetActor(data.actorId);

		if (actor != null)
		{
			GameEvent gameEvent = genEvents.Find(item => item.Id == data.id);

			if (gameEvent == null)
			{
				GameObject newEventGO = new GameObject("E (" + data.type + ") => " + data.label);

				gameEvent = newEventGO.AddComponent(data.type) as GameEvent;
				gameEvent.Id = data.id;
				gameEvent.Label = data.label;

				data.OnShowGui = gameEvent.ShowGui;

				//RegisterRuleElement(gameEvent);

				actor.AddEvent(gameEvent);

				// setting parameters
				SetParameters(gameEvent, data);

				// init
				gameEvent.Initialize(this);
			}
			else
			{
				UpdateEvent(gameEvent, data);
			}
		}
		else
		{
			Debug.LogError("There is no actor for event " + data.id + " in the scene.");
		}
	}

	public void AddReactionToScene(BaseRuleElement.ReactionData data)
	{
#if DEBUG
		Debug.Log("Adding reaction " + data.label + "(" + data.id + ")" + " to actor " + data.actorId + ". Setting " + data.parameters.Count + " parameters.");
#endif
		// actor should exist by now - if it doesn't, don't create or update any related actions or events
		Actor actor = GetActor(data.actorId);

		if (actor != null)
		{
			// find reaction if already in scene
			Reaction reaction = genReactions.Find(item => item.Id == data.id);

			if (reaction == null)
			{
				GameObject newReactionGO = new GameObject(data.label);

				reaction = newReactionGO.AddComponent(data.type) as Reaction;
				reaction.Id = data.id;
				reaction.Label = data.label;

				data.OnShowGui = reaction.ShowGui;

				actor.AddReaction(reaction);

				// parameters
				SetParameters(reaction, data);

				reaction.ListenedEvent = genEvents.Find(item => item.Id == data.eventId);

				// init after everything was prepared
				reaction.Initialize(this);
			}
			else
			{
				UpdateReaction(reaction, data);
			}
		}
		else
		{
			Debug.LogError("There is no actor for reaction " + data.id + " in the scene.");
		}
	}

	#region SetParameters
	private void SetParameters<T, U>(T gObject, U data) 
		where T : Component 
		where U : BaseRuleElement.RuleData
	{
		foreach (BaseRuleElement.Param parameter in data.parameters)
		{
#if DEBUG
			Debug.Log("Adding param " + parameter.name + " ("+ parameter.type + ") to " + data.type + ", value: " + parameter.value);
#endif

			FieldInfo pFieldInfo = data.type.GetField(parameter.name);

			if (pFieldInfo == null)
			{
				Debug.LogError("Couldn't find parameter " + parameter.name + " (in type " + parameter.type + ").");
				continue;
			}

			// different handling for object references
			if (parameter.type.IsSubclassOf(typeof(Actor)) || parameter.type.IsAssignableFrom(typeof(Actor))
				|| (parameter.value is int && pFieldInfo.FieldType.IsAssignableFrom(typeof(Actor))))
			{
				if ((int)parameter.value < genActors.Count && (int)parameter.value >= 0)
					pFieldInfo.SetValue(gObject, genActors.Find(item => item.Id == (int)parameter.value));
			}
			// set all other values
			else
			{
				pFieldInfo.SetValue(gObject, parameter.value);
			}
		}
	}

	//private void SetComponentParameters<T, U>(T gObject, U data)
	//	where T : Actor
	//	where U : BaseRuleElement.ActorData
	//{
	//	foreach (BaseRuleElement.ComponentData componentData in data.components)
	//	{
	//		// check if component was added
	//		Component component = gObject.GetComponent(componentData.type);
	//		if (component != null)
	//		{
	//			// if so, set parameters
	//			SetParameters(component, componentData);
	//		}
	//		else
	//		{
	//			// if not, log warning about trying to set parameters for ghost component
	//			Debug.LogWarning("Actor (" + data.id + "): trying to set parameter for component (" + componentData.type + ") which doesn't exist.");
	//		}
	//	}
	//}
	#endregion

	#endregion

	#region Updating Elements
	void UpdateActor(BaseRuleElement.ActorData actorData, Actor actor)
	{
		// check if matching id actor has same type
		// if different type, change it. Self-destroy old one, create new actor.
		if (actor != null && actor.GetType() != actorData.type)
		{
			actor.Reset();

			// remove from lists
			genActors.Remove(actor);
			unusedElements.Remove(actor);


#if DEBUG
			Debug.LogWarning("Updating actor (" + actor.Id + "), new type: " + actorData.type);
#endif

			Destroy(actor);

			AddActorToScene(actorData);
		}
		else
		{
			actor.Label = actorData.label;
			actorData.OnShowGui = actor.ShowGui;

			// update parameters
			SetParameters(actor, actorData);

			//SetComponentParameters(oldActor, actorData);

			// element was processed, remove from unused list (would be deleted otherwise)
			unusedElements.Remove(actor);
		}
	}

	void UpdateEvent(GameEvent gameEvent, BaseRuleElement.EventData eventData)
	{
		// check if it's of the right type
		if (gameEvent.GetType() != eventData.type)
		{
			// if it's not the same type, tell old event to self-destroy

			gameEvent.Reset();

			// cleaning up lists
			genEvents.Remove(gameEvent);
			unusedElements.Remove(gameEvent);

#if DEBUG
			Debug.LogWarning("Updating event (" + gameEvent.Id + "), new type: " + eventData.type);
#endif

			// delete old event
			Destroy(gameEvent);

			// create new event from data
			AddEventToScene(eventData);
		}
		else
		{
			gameEvent.Label = eventData.label;

			eventData.OnShowGui = gameEvent.ShowGui;

			Debug.Log("updating params..");
			for (int i = 0; i < eventData.parameters.Count; i++)
				Debug.Log("value: " + eventData.parameters[i].value);

			// if same type, set the parameters
			SetParameters(gameEvent, eventData);

			// query the actor id, reset actor ref and hierarchy placement
			Actor oldActor = gameEvent.Actor;

			// add event to new actor
			GetActor(eventData.actorId).AddEvent(gameEvent);
				
			if (oldActor != gameEvent.Actor)
				oldActor.RemoveEvent(gameEvent);
			unusedElements.Remove(gameEvent);
		}
	}

	void UpdateReaction(Reaction reaction, BaseRuleElement.ReactionData reactionData)
	{
		// check type
		if (reaction.GetType() != reactionData.type)
		{
			// if it's not the same, tell the old reaction to reset and clean up
			reaction.Reset();

			// clean up lists
			genReactions.Remove(reaction);
			unusedElements.Remove(reaction);

#if DEBUG
			Debug.LogWarning("Updating reaction (" + reaction.Id + "), new type: " + reactionData.type);
#endif

			// destroy reaction
			Destroy(reaction);

			// create new reaction from scratch
			AddReactionToScene(reactionData);
		}
		else
		{
			reaction.Label = reactionData.label;

			reactionData.OnShowGui = reaction.ShowGui;

			// set parameters
			SetParameters(reaction, reactionData);

			// set actor reference
			Actor oldActor = reaction.Reactor;
			GetActor(reactionData.actorId).AddReaction(reaction);
			oldActor.RemoveReaction(reaction);

			// set event reference
			reaction.Unregister();
			reaction.ListenedEvent = genEvents.Find(item => item.Id == reactionData.eventId);
			reaction.Register();

			// remove from unused objects
			unusedElements.Remove(reaction);
		}
	}
	#endregion

	#region Moving Events and Reactions between Actors
	public void ChangeActor(GameEvent gameEvent, int newActorId)
	{
		if (gameEvent.Actor.Id != newActorId)
		{
			gameEvent.Reset();
			gameEvent.Actor.RemoveEvent(gameEvent);
			Actor newActor = GetActor(newActorId);
			if (newActor)
			{
				newActor.AddEvent(gameEvent);
				gameEvent.Initialize(this);
			}
		}
	}

	public void ChangeActor(Reaction reaction, int newActorId)
	{
		if (reaction.Reactor.Id != newActorId)
		{
			reaction.Reset();
			reaction.Reactor.RemoveReaction(reaction);
			Actor newActor = GetActor(newActorId);
			if (newActor)
			{
				newActor.AddReaction(reaction);
				reaction.Initialize(this);
			}
		}
	}

	#endregion

	#region Initialize Rule Elements
	void InitializeRuleElements()
	{
		foreach (Actor a in genActors)
		{
			a.Initialize(this);
		}
	}
	#endregion

	#region Register/Delete RuleElement
	public void RegisterRuleElement(BaseRuleElement element)
	{
		if (element as Actor != null)
		{
			RegisterRuleElement(element as Actor);
		}
		else if (element as GameEvent != null)
		{
			RegisterRuleElement(element as GameEvent);
		}
		else if (element as Reaction != null)
		{
			RegisterRuleElement(element as Reaction);
		}
		else
		{
			Debug.LogWarning("Couldn't register element: " + element.ToString());
		}
	}

	public void RegisterRuleElement(Actor actor)
	{
		if (!GetActor(actor.Id))
		{
			if (actor.Id == -1) // TODO use actordata AND genActors for id generation
				actor.Id = GetId(genActors.ToArray());
				
			genActors.Add(actor);
		}
	}

	public void RegisterRuleElement(GameEvent gameEvent)
	{
		if (!genEvents.Find(item => item.Id == gameEvent.Id))
		{
			if (gameEvent.Id == -1)
				gameEvent.Id = GetId(genEvents.ToArray());

			genEvents.Add(gameEvent);
		}
	}

	public void RegisterRuleElement(Reaction reaction)
	{
		if (!genReactions.Find(item => item.Id == reaction.Id))
		{
			if (reaction.Id == -1)
				reaction.Id = GetId(genReactions.ToArray());

			genReactions.Add(reaction);

#if DEBUG
			Debug.Log("Registered reaction: " + reaction.name + " (" + reaction.Id + ")");
#endif
		}
	}

	public void Unregister(GameEvent gameEvent)
	{
		genEvents.Remove(gameEvent);
	}

	public void Unregister(Reaction reaction)
	{
		genReactions.Remove(reaction);
	}
	#endregion

	#region UnusedElementsList Handling
	void PopulateUnusedElementsList()
	{
		foreach (BaseRuleElement elem in GameObject.FindObjectsOfType(typeof(BaseRuleElement)) as BaseRuleElement[])
		{
			if (!elem.DontDeleteOnLoad)
				unusedElements.Add(elem);
		}
	}

	void DeleteUnusedElements()
	{
		foreach (BaseRuleElement element in unusedElements)
		{
			element.Reset();

			if (element as GameEvent)
			{
				genEvents.Remove(element as GameEvent);
			}
			else if (element as Reaction)
			{
				genReactions.Remove(element as Reaction);
			}
			else if (element as Actor)
			{
				genActors.Remove(element as Actor);
			}
#if DEBUG
			Debug.Log("Destroyed unused element: " + element.name + " (" + element.Id + ") " + element.GetType());
#endif
			Destroy(element.gameObject);
		}

		unusedElements.Clear();
	}
	#endregion

	#region Loading Rules
	public void LoadRules(string filename)
	{
		Debug.LogWarning("Generating level rules from " + filename + ".xml ...");

		CurrentRuleFileName = filename;

		// clear previously parsed rule data lists
		ClearData();

		// the actual parsing and filling data lists
		ruleParser.Parse(this, CurrentRuleFileName);

		// generate or update the level objects from the collected data
		GenerateLevelFromData(ActorData, EventData, ReactionData);

		// initialize elements. order of initializing is important (first actors, then events, then reactions)
		InitializeRuleElements();

		Debug.LogWarning("Completed generating level.");
	}

	public void LoadRules(List<BaseRuleElement.ActorData> actorData, 
		List<BaseRuleElement.EventData> eventData, 
		List<BaseRuleElement.ReactionData> reactionData)
	{
		Debug.LogWarning("Generating level from rule data.");

		GenerateLevelFromData(actorData, eventData, reactionData);

		//ClearData();

		//ActorData.AddRange(actorData);
		//EventData.AddRange(eventData);
		//ReactionData.AddRange(reactionData);

		InitializeRuleElements();

		Debug.LogWarning("Completed generating level.");
	}

	void GenerateLevelFromData(List<BaseRuleElement.ActorData> actorData, 
		List<BaseRuleElement.EventData> eventData, 
		List<BaseRuleElement.ReactionData> reactionData)
	{
		GlobalActor.Instance.Initialize(this);
		Counter.ResetCounters();

		// store all base rule elements in a list, to keep track of which ones were used, which ones weren't
		PopulateUnusedElementsList();

		GenerateActorsFromData(actorData);

		GenerateEventsFromData(eventData);

		GenerateReactionsFromData(reactionData);

		// delete all base rule elements which haven't been updated/created
		DeleteUnusedElements();

		// fire event
		if (OnGeneratedLevel != null)
			OnGeneratedLevel(actorData, eventData, reactionData, CurrentRuleFileName);
	}

	void GenerateActorsFromData(List<BaseRuleElement.ActorData> actorData)
	{
		// generate actors
		for (int i = 0; i < actorData.Count; i++)
		{
			AddActorToScene(actorData[i]);
		}
	}

	void GenerateEventsFromData(List<BaseRuleElement.EventData> eventData)
	{
		// generate events
		for (int i = 0; i < eventData.Count; i++)
		{
			AddEventToScene(eventData[i]);
		}
	}

	void GenerateReactionsFromData(List<BaseRuleElement.ReactionData> reactionData)
	{
		// generate reactions
		for (int i = 0; i < reactionData.Count; i++)
		{
			AddReactionToScene(reactionData[i]);
		}
	}
	#endregion

	#region Saving Rules
	// TODO rewrite to using the ruledata, not the level elements?
	public void SaveRules(string filename, bool overwrite, List<BaseRuleElement.RuleData> rules = null)
	{
		Debug.LogWarning("Saving rules into " + filename + ".xml ...");

		if (rules == null)
		{
			foreach (Actor a in GameObject.FindObjectsOfType(typeof(Actor)))
			{
				a.ScanEvents();
				a.ScanReactions();
			}

			// broadcast save request, let elements handle storing info about themselves
			BaseRuleElement[] brelements = GameObject.FindObjectsOfType(typeof(BaseRuleElement)) as BaseRuleElement[];

			rules = new List<BaseRuleElement.RuleData>();
			foreach (BaseRuleElement br in brelements)
			{
				rules.Add(br.GetRuleInformation());
			}
		}

		ruleParser.SaveRules(rules, filename, overwrite);

		Debug.LogWarning("Completed saving rules.");
	}
	#endregion

	#region Accessors for generated elements
	public Actor GetActor(int id)
	{
		Actor actor = null;
		if (id == -1)
		{
			actor = GlobalActor.Instance;
		}
		else
		{
			actor = genActors.Find(item => item.Id == id);
		}

		return actor;
	}

	public GameEvent GetEvent(int id)
	{
		return genEvents.Find(item => item.Id == id);
	}

	public Reaction GetReaction(int id)
	{
		return genReactions.Find(item => item.Id == id);
	}
	#endregion
}
