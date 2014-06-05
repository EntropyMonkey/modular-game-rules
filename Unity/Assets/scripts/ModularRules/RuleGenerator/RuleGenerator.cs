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
	public RuleGUI ruleGUI
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

	public delegate void GeneratedLevel(List<BaseRuleElement.ActorData> actorData, List<BaseRuleElement.EventData> eventData, List<BaseRuleElement.ReactionData> reactionData); // fired when the parser is done with parsing
	public GeneratedLevel OnGeneratedLevel;

	// rule data collection
	public List<BaseRuleElement.ActorData> ActorData = new List<BaseRuleElement.ActorData>();
	public List<BaseRuleElement.EventData> EventData = new List<BaseRuleElement.EventData>();
	public List<BaseRuleElement.ReactionData> ReactionData = new List<BaseRuleElement.ReactionData>();

	// contains all placeholders for actors in the scene, in order of their Id
	List<PlaceholderElement> placeholders;

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

		ruleGUI = FindObjectOfType<RuleGUI>();
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
			Application.Quit();

		string filepath = Application.dataPath + @"/Rules/";

		if (Directory.Exists(filepath))
		{
			ruleGUI.Files = Directory.GetFiles(filepath, "*.xml");
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
	}

	public void StartEventExecution()
	{
		foreach (Actor a in genActors)
		{
			a.ExecuteEvents();
		}
	}
	#endregion

	#region Adding Parsed Rule Data (Interface for Parser)
	void ClearData()
	{
		ActorData.Clear();
		EventData.Clear();
		ReactionData.Clear();
	}

	public void AddRuleData(BaseRuleElement.ActorData actorData)
	{
		if (ActorData.Find(item => item.id == actorData.id) == null)
		{
			ActorData.Add(actorData.DeepCopy());
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
	int GetId(BaseRuleElement[] genElements)
	{
		int highestId = 0;
		for (int i = 0; i < genElements.Length; i++)
		{
			if (genElements[i].Id > highestId && genElements[i].Id - highestId > 1)
			{
				// if there's an unused id between two others, use it, stop the loop
				return genElements[i].Id + 1;
			}
			else if (genElements[i].Id > highestId)
			{
				highestId = genElements[i].Id;
			}
		}

		return highestId + 1;
	}

	bool IdExists(BaseRuleElement[] genElements, int id)
	{
		return Array.Find<BaseRuleElement>(genElements, item => item.Id == id) != null;
	}
	#endregion

	#region Adding Elements to the scene

	void AddActorToScene(BaseRuleElement.ActorData data)
	{
		PlaceholderElement placeholder = placeholders.Find(item => item.Id == data.id);
		if (placeholder != null)
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
				GameObject pGo = placeholder.gameObject;

				// create new actor
				Actor actor = pGo.AddComponent(data.type) as Actor;
				actor.Id = data.id;
				actor.Label = data.label;

				data.OnShowGui = actor.ShowGui;

				RegisterRuleElement(actor);

				SetParameters(actor, data);

				// not needed right now
				//SetComponentParameters(actor, data);

				// deactivate placeholder
				placeholder.enabled = false;
			}
				
		}
		else
		{
			Debug.LogError("There is no placeholder for actor " + data.id + " in the scene.");
		}
	}

	void AddEventToScene(BaseRuleElement.EventData data)
	{
#if DEBUG
		Debug.Log("Adding event " + data.id + " to actor " + data.actorId + ". Setting " + data.parameters.Count + " parameters.");
#endif
		// actor should exist by now - if it doesn't, don't create any related actions or events
		Actor actor = genActors.Find(item => item.Id == data.actorId);

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

	void AddReactionToScene(BaseRuleElement.ReactionData data)
	{
#if DEBUG
		Debug.Log("Adding reaction " + data.id + " to actor " + data.actorId + ". Setting " + data.parameters.Count + " parameters.");
#endif
		// actor should exist by now - if it doesn't, don't create or update any related actions or events
		Actor actor = genActors.Find(item => item.Id == data.actorId);

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
			if (parameter.type.IsSubclassOf(typeof(Actor)) || parameter.type.IsAssignableFrom(typeof(Actor)))
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

	private void SetComponentParameters<T, U>(T gObject, U data)
		where T : Actor
		where U : BaseRuleElement.ActorData
	{
		foreach (BaseRuleElement.ComponentData componentData in data.components)
		{
			// check if component was added
			Component component = gObject.GetComponent(componentData.type);
			if (component != null)
			{
				// if so, set parameters
				SetParameters(component, componentData);
			}
			else
			{
				// if not, log warning about trying to set parameters for ghost component
				Debug.LogWarning("Actor (" + data.id + "): trying to set parameter for component (" + componentData.type + ") which doesn't exist.");
			}
		}
	}
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

			// if same type, set the parameters
			SetParameters(gameEvent, eventData);

			// query the actor id, reset actor ref and hierarchy placement
			Actor oldActor = gameEvent.Actor;

			// add event to new actor
			genActors.Find(item => item.Id == eventData.actorId).AddEvent(gameEvent);
				
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

			// set parameters
			SetParameters(reaction, reactionData);

			// set actor reference
			Actor oldActor = reaction.Reactor;
			genActors.Find(item => item.Id == reactionData.actorId).AddReaction(reaction);
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
		if (!genActors.Find(item => item.Id == actor.Id))
		{
			// check if actor id already exists - in created actors AND placeholders
			Actor[] fakeActors = new Actor[genActors.Count + placeholders.Count];
			genActors.CopyTo(fakeActors);
			GameObject fakeObject = new GameObject("fakeActorsTemp");

			// create fake Actors for Id handling from placeholders
			for (int i = genActors.Count; i < fakeActors.Length; i++ )
			{
				fakeActors[i] = fakeObject.AddComponent<StandardActor>();
				fakeActors[i].Id = placeholders[i - genActors.Count].Id;
			}

			if (actor.Id == -1)
				actor.Id = GetId(fakeActors);

			Destroy(fakeObject);
				
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
			if (element as Actor == null)
			{
				if (element as GameEvent)
				{
					genEvents.Remove(element as GameEvent);
				}
				else if (element as Reaction) genReactions.Remove(element as Reaction);
#if DEBUG
				Debug.Log("Destroyed unused element: " + element.name + " (" + element.Id + ")");
#endif
				Destroy(element.gameObject);
			}
			else
			{
				genActors.Remove(element as Actor);
#if DEBUG
				Debug.Log("Destroy unused actor: " + element.name + " (" + element.Id + ")");
#endif
				Destroy(element);
			}
		}

		unusedElements.Clear();
	}
	#endregion

	#region Finding generation placeholders in scene
	private class GenerationElementComparer : IComparer<PlaceholderElement>
	{
		public int Compare(PlaceholderElement x, PlaceholderElement y)
		{
			if (x == null || y == null || x == y) return 0;

			if (x.Id > y.Id) return 1;
			else if (x.Id < y.Id) return -1;
			else
			{
				Debug.LogError("Two actors have the same id (" + x.Id + "-" + x.name + ":" + y.Id + "-" + y.name + ").");
				return 0;
			}
		}
	}

	void FindGenerationPlaceholdersInScene()
	{
		if (placeholders == null) placeholders = new List<PlaceholderElement>();
		else placeholders.Clear();

		placeholders.AddRange(FindObjectsOfType<PlaceholderElement>());
		GenerationElementComparer gec = new GenerationElementComparer();
		placeholders.Sort(gec); // sort according to ids, starting with 0, assuming there are no left-over ids

#if DEBUG
		{
			Debug.Log("Found generation placeholders in scene: " + placeholders.Count);
			for (int i = 0; i < placeholders.Count; i++)
			{
				Debug.Log(i + " - " + placeholders[i].name + " - " + placeholders[i].Id);
			}
		}
#endif
	}
	#endregion

	#region Loading Rules
	public void LoadRules(string filename)
	{
		Debug.LogWarning("Generating level rules from " + filename + ".xml ...");

		CurrentRuleFileName = filename;

		// find all active placeholders for actors in the scene
		FindGenerationPlaceholdersInScene();

		// clear previously parsed rule data lists
		ClearData();

		// the actual parsing and adding elements to the scene
		ruleParser.Parse(this, filename);

		// store all base rule elements in a list, to keep track of which ones were used, which ones weren't
		PopulateUnusedElementsList();

		// generate or update the level objects from the collected data
		GenerateLevelFromData();

		// delete all base rule elements which haven't been updated/created
		DeleteUnusedElements();

		// fire event
		if (OnGeneratedLevel != null)
			OnGeneratedLevel(ActorData, EventData, ReactionData);

		// initialize elements. order of initializing is important (first actors, then events, then reactions)
		InitializeRuleElements();

		Debug.LogWarning("Completed generating level.");
	}

	void GenerateLevelFromData()
	{
		GenerateActorsFromData();

		GenerateEventsFromData();

		GenerateReactionsFromData();
	}

	void GenerateActorsFromData()
	{
		// generate actors
		for (int i = 0; i < ActorData.Count; i++)
		{
			AddActorToScene(ActorData[i]);
		}
	}

	void GenerateEventsFromData()
	{
		// generate events
		for (int i = 0; i < EventData.Count; i++)
		{
			AddEventToScene(EventData[i]);
		}
	}

	void GenerateReactionsFromData()
	{
		// generate reactions
		for (int i = 0; i < ReactionData.Count; i++)
		{
			AddReactionToScene(ReactionData[i]);
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
}
