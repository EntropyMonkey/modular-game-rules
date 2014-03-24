#define DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public class RuleGenerator : MonoBehaviour
	{
		public bool ShowButton = true;

		RuleParser ruleParser;

		// contains all placeholders for actors in the scene, in order of their Id
		List<GenerateElement> placeholders;

		// keeping track of generated elements
		List<Actor> genActors = new List<Actor>();
		List<Event> genEvents = new List<Event>();
		List<Reaction> genReactions = new List<Reaction>();

		void Awake()
		{
			ruleParser = gameObject.AddComponent<RuleParser>();
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

		public void AddActorToScene(RuleParser.ActorData data)
		{
#if DEBUG
			Debug.Log("Adding actor " + data.id + ", " + data.type);
#endif
			if (placeholders[data.id] != null)
			{
				GameObject pGo = placeholders[data.id].gameObject;

				// create new actor
				Actor newActor = pGo.AddComponent(data.type) as Actor;
				newActor.Id = data.id;
				genActors.Add(newActor);
				genActors.Sort((x, y) => x.Id.CompareTo(y.Id)); // sort list

				// destroy placeholder
				Destroy(placeholders[data.id]);
			}
		}

		public void AddEventToScene(RuleParser.EventData data)
		{
#if DEBUG
			Debug.Log("Adding event " + data.id + " to actor " + data.actorId + ". Setting " + data.parameters.Count + " parameters.");
#endif
		}
		#endregion

		void Regenerate(string filename)
		{
			Debug.LogWarning("Generating rules from testRules.xml ...");

			FindGenerationPlaceholdersInScene();
			ruleParser.Parse(this, filename);

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
