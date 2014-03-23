using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public class RuleGenerator : MonoBehaviour
	{
		public bool ShowButton = true;

		RuleParser ruleParser;

		List<GenerateElement> placeholders;

		void Awake()
		{
			ruleParser = gameObject.AddComponent<RuleParser>();

			FindGenerationPlaceholders();
		}

		private class GenerationElementComparer : IComparer<GenerateElement>
		{
			public int Compare(GenerateElement x, GenerateElement y)
			{
				if (x == null || y == null || x == y) return 0;

				if (x.Id > y.Id) return 1;
				else if (x.Id < y.Id) return -1;
				else
				{
					Debug.LogError("Two actors have the same id (" + x.Id + "-" + x.name + ":" + y.Id + "-" + y.name + "). Will ignore " + y.name + ".");
					return 0;
				}
			}
		}

		void FindGenerationPlaceholders()
		{
			if (placeholders == null) placeholders = new List<GenerateElement>();
			else placeholders.Clear();

			placeholders.AddRange(FindObjectsOfType<GenerateElement>());
			GenerationElementComparer gec = new GenerationElementComparer();
			placeholders.Sort(gec); // sort according to ids, starting with 0, assuming there are no left-over ids

			// why only one? something's stinky
			Debug.Log(placeholders.ToString());
		}

		public void AddActor(RuleParser.ActorData data)
		{
			Debug.Log("Adding actor " + data.id + ", " + data.type);


		}

		void OnGUI()
		{
			if (ShowButton && GUI.Button(new Rect(0, 0, 100, 50), "Generate Rules"))
			{
				Debug.LogWarning("Generating rules...");
				ruleParser.Parse(this, "testRules");
				Debug.LogWarning("Completed generating rules.");
			}
		}
	}
}
