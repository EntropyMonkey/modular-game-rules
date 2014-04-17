#define DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.IO;

namespace ModularRules
{
	public class RuleGenerator : MonoBehaviour
	{
		public bool ShowButton = true;

		RuleParserLinq ruleParser;

		// contains all placeholders for actors in the scene, in order of their Id
		List<GenerateElement> placeholders;

		// keeping track of generated elements
		List<Actor> genActors = new List<Actor>();
		List<GameEvent> genEvents = new List<GameEvent>();
		List<Reaction> genReactions = new List<Reaction>();

		void Awake()
		{
			ruleParser = gameObject.AddComponent<RuleParserLinq>();
		}

		#region Finding generation placeholders in scene
		private class GenerationElementComparer : IComparer<GenerateElement>
		{
			public int Compare(GenerateElement x, GenerateElement y)
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
			if (placeholders == null) placeholders = new List<GenerateElement>();
			else placeholders.Clear();

			placeholders.AddRange(FindObjectsOfType<GenerateElement>());
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

		#region Adding Elements to the scene

		public void AddActorToScene(BaseRuleElement.ActorData data)
		{
#if DEBUG
			Debug.Log("Adding actor " + data.id + ", " + data.type);
#endif
			if (placeholders.Count > data.id && placeholders[data.id] != null)
			{
				GameObject pGo = placeholders[data.id].gameObject;

				// create new actor
				Actor newActor = pGo.AddComponent(data.type) as Actor;
				newActor.Id = data.id;
				genActors.Add(newActor);
				genActors.Sort((x, y) => x.Id.CompareTo(y.Id)); // sort list

				// deactivate placeholder
				placeholders[data.id].enabled = false;
			}
			else
			{
				Debug.LogError("There is no placeholder for actor " + data.id + " in the scene.");
			}
		}

		public void AddEventToScene(BaseRuleElement.EventData data)
		{
#if DEBUG
			Debug.Log("Adding event " + data.id + " to actor " + data.actorId + ". Setting " + data.parameters.Count + " parameters.");
#endif
			if (data.actorId < genActors.Count && genActors[data.actorId] != null)
			{
				GameObject newEventGO = new GameObject("E => " + data.type);
				newEventGO.transform.parent = genActors[data.actorId].Events.transform;

				GameEvent newEvent = newEventGO.AddComponent(data.type) as GameEvent;

				// references
				newEvent.Id = data.id;
				newEvent.Actor = genActors[data.actorId];

				// setting parameters
				SetParameters(newEvent, data);

				// keep track of generated events
				genEvents.Add(newEvent);
				genEvents.Sort((x, y) => x.Id.CompareTo(y.Id)); // sort list
			}
		}

		public void AddReactionToScene(BaseRuleElement.ReactionData data)
		{
#if DEBUG
			Debug.Log("Adding reaction " + data.id + " to actor " + data.actorId + ". Setting " + data.parameters.Count + " parameters.");
#endif
			if (data.actorId < genActors.Count && genActors[data.actorId] != null)
			{
				GameObject newReactionGO = new GameObject("R => " + data.type);
				newReactionGO.transform.parent = genActors[data.actorId].Reactions.transform;

				Reaction newReaction = newReactionGO.AddComponent(data.type) as Reaction;

				// references
				newReaction.Id = data.id;
				newReaction.Reactor = genActors[data.actorId];
				newReaction.ListenedEvent = genEvents[data.eventId];

				// parameters
				SetParameters(newReaction, data);

				// keeping track
				genReactions.Add(newReaction);
				genReactions.Sort((x, y) => x.Id.CompareTo(y.Id));
			}
		}

		private void SetParameters<T, U>(T gObject, U data) 
			where T : BaseRuleElement 
			where U : BaseRuleElement.EventData
		{
			foreach (BaseRuleElement.Param parameter in data.parameters)
			{
#if DEBUG
				Debug.Log("Adding param " + parameter.type + " to " + data.type + ", value: " + parameter.value);
#endif

				FieldInfo pFieldInfo = data.type.GetField(parameter.name);
				// different handling for object references
				if (parameter.type.IsSubclassOf(typeof(Actor)) || parameter.type.IsAssignableFrom(typeof(Actor)))
				{
					if ((int)parameter.value < genActors.Count && (int)parameter.value >= 0)
						pFieldInfo.SetValue(gObject, genActors[(int)parameter.value]);
				}
				// set all other values
				else
				{
					pFieldInfo.SetValue(gObject, parameter.value);
				}
			}
		}
		#endregion

		#region Update Elements
		void UpdateActor(BaseRuleElement.ActorData actorData)
		{
			// check if matching id actor has same type

			// if different type, change it. Self-destroy old one, create new actor.

			// ==> have to tell events and reactions that there's a new kind of actor, after new events and reactions have beens set
		}

		void UpdateEvent(BaseRuleElement.EventData eventData)
		{
			// find event with matching id

			// check if it's of the right type

			// if so, set the parameters

			// if it's not the same type, tell old event to self-destroy

			// query the actor id, reset actor ref
		}

		void UpdateReaction(BaseRuleElement.ReactionData reactionData)
		{
			// find reaction with matching id

			// check type

			// if it's ok, set parameters

			// if it's not the same, tell the old reaction to self-destroy (including all added requiredcomponents)

			// query the actor id, reset actor ref
		}
		#endregion

		void InitializeRuleElements()
		{
			foreach (Actor a in genActors)
			{
				a.Initialize();
			}
		}

		void LoadRules(string filename)
		{
			Debug.LogWarning("Generating level rules from " + filename + ".xml ...");

			FindGenerationPlaceholdersInScene();
			ruleParser.Parse(this, filename);

			// initialize elements. order of initializing is important
			InitializeRuleElements();

			Debug.LogWarning("Completed generating level.");
		}

		void SaveRules(string filename)
		{
			Debug.LogWarning("Saving rules into " + filename + ".xml ...");
			
			// broadcast save request, let elements handle storing info about themselves
			BaseRuleElement[] brelements = GameObject.FindObjectsOfType(typeof(BaseRuleElement)) as BaseRuleElement[];

			List<BaseRuleElement.RuleData> rules = new List<BaseRuleElement.RuleData>();
			foreach (BaseRuleElement br in brelements)
			{
				rules.Add(br.GetRuleInformation());
			}

			ruleParser.SaveRules(rules, filename);

			Debug.LogWarning("Completed saving rules.");
		}

		void OnGUI()
		{
			string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"/Assets/Resources/", "rules_*.xml");

			if (ShowButton)
			{
				int x = 0; int width = 150;
				for (int i = 0; i < files.Length; i++)
				{
					string file = Path.GetFileNameWithoutExtension(files[i]);
					if (GUI.Button(new Rect(x, 0, width, 50), "Load " + file))
					{
						LoadRules(file);
					}
					x += width;
				}
			}

			if (ShowButton && GUI.Button(new Rect(0, 50, 100, 50), "Save Rules"))
			{
				SaveRules("rules_0");
			}
		}
	}
}
