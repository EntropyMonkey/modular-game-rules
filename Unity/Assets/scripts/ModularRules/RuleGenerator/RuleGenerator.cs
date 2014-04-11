#define DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

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
		}
#endif
		#endregion

		#region Adding Elements to the scene

		public void AddActorToScene(RuleParserLinq.ActorData data)
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

		public void AddEventToScene(RuleParserLinq.EventData data)
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

		public void AddReactionToScene(RuleParserLinq.ReactionData data)
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
			where U : RuleParserLinq.EventData
		{
			foreach (RuleParserLinq.Param parameter in data.parameters)
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

		void InitializeRuleElements()
		{
			foreach (Actor a in genActors)
			{
				a.Initialize();
			}
		}

		void Regenerate(string filename)
		{
			Debug.LogWarning("Generating rules from testRules.xml ...");

			FindGenerationPlaceholdersInScene();
			ruleParser.Parse(this, filename);

			// initialize elements. order of initializing is important
			InitializeRuleElements();

			Debug.LogWarning("Completed generating rules.");
		}

		void OnGUI()
		{
			if (ShowButton && GUI.Button(new Rect(0, 0, 100, 50), "Generate Rules"))
			{
				Regenerate("testRules");
			}
		}
	}
}
