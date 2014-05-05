using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public class SpawnObject : Reaction
	{
		// randomly spawns one object from this list if it is found in the resources folder (must be there at unity compile-time)
		public List<string> SpawnedPrefabs;

		public Vector3 Direction;

		public float Distance;

		public bool DeactivateAfterSpawning;

		public bool RandomRotation;

		void OnEnable()
		{
			Register();
		}

		void OnDisable()
		{
			Unregister();
		}

		public override RuleData GetRuleInformation()
		{
			RuleData data = base.GetRuleInformation();

			if (data.parameters == null) data.parameters = new List<Param>();

			string prefabs = "";
			for (int i = 0; i < SpawnedPrefabs.Count - 1; i++)
			{
				prefabs += SpawnedPrefabs[i] + " ";
			}
			prefabs += SpawnedPrefabs[SpawnedPrefabs.Count - 1];

			data.parameters.Add(new Param()
			{
				name = "SpawnedPrefabs",
				type = SpawnedPrefabs.GetType(),
				value = prefabs
			});

			data.parameters.Add(new Param()
			{
				name = "Direction",
				type = Direction.GetType(),
				value = "" + Direction.x + " " + Direction.y + " " + Direction.z
			});

			data.parameters.Add(new Param()
			{
				name = "Distance",
				type = Distance.GetType(),
				value = Distance
			});

			data.parameters.Add(new Param()
			{
				name = "DeactivateAfterSpawning",
				type = DeactivateAfterSpawning.GetType(),
				value = DeactivateAfterSpawning
			});

			return data;
		}

		protected override void React(GameEventData eventData)
		{
			if (SpawnedPrefabs == null || SpawnedPrefabs.Count == 0) return;

			string spawn = SpawnedPrefabs[Random.Range(0, SpawnedPrefabs.Count)];

			GameObject spawnedObjectPrefab = Resources.Load(spawn) as GameObject;
			if (spawnedObjectPrefab == null)
			{
				Debug.LogError("Couldn't spawn object '" + spawn + "'. It wasn't found in any of the Resources folders or is no GameObject.");
			}
			else
			{
				GameObject spawnedObject = Instantiate(spawnedObjectPrefab) as GameObject;

				Vector3 dir = Reactor.transform.TransformDirection(Direction);

				spawnedObject.transform.position = Reactor.transform.position + dir * Distance;
				if (RandomRotation)
				{
					spawnedObject.transform.rotation = Quaternion.Euler(0, Random.Range(0, 3) * 90, 0);
				}

				if (DeactivateAfterSpawning)
				{
					Unregister();
					gameObject.SetActive(false);
				}
			}
		}
	}
}
