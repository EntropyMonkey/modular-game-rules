using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class SpawnObject : Reaction
{
	public string[] possibleSpawnTags = new string[] { "Danger", "Fruit", "Collectible" };

	// randomly spawns one object from this list if it is found in the resources folder (must be there at unity compile-time)
	public List<string> SpawnedPrefabs = new List<string>();

	public string TagForSpawnedObjects = "Fruit";

	public Vector3 Direction;

	public Vector3 StartVelocity;

	public float Distance;

	public bool DeactivateAfterSpawning;

	public bool RandomRotation;

	private const int maxSpawnedObjects = 100;
	private static Queue<GameObject> spawnedObjects = new Queue<GameObject>();

	private DropDown tagDropDown;

	void OnEnable()
	{
		Register();
	}

	void OnDisable()
	{
		Unregister();
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		int selected = 0;
		if (TagForSpawnedObjects != "")
			selected = System.Array.FindIndex(possibleSpawnTags, item => item == TagForSpawnedObjects);

		tagDropDown = new DropDown(selected, possibleSpawnTags);
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
			name = "TagForSpawnedObjects",
			type = TagForSpawnedObjects.GetType(),
			value = TagForSpawnedObjects
		});

		data.parameters.Add(new Param()
		{
			name = "Direction",
			type = Direction.GetType(),
			value = "" + Direction.x + " " + Direction.y + " " + Direction.z
		});

		data.parameters.Add(new Param()
		{
			name = "StartVelocity",
			type = StartVelocity.GetType(),
			value = "" + StartVelocity.x + " " + StartVelocity.y + " " + StartVelocity.z
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

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("randomly spawn", RuleGUI.ruleLabelStyle);

		SpawnedPrefabs = RuleGUI.ShowParameter(SpawnedPrefabs);
		ChangeParameter("SpawnedPrefabs", ruleData.parameters, SpawnedPrefabs);

		GUILayout.Label("in direction", RuleGUI.ruleLabelStyle);

		Direction = RuleGUI.ShowParameter(Direction, "spawnDirection" + Id);
		ChangeParameter("Direction", ruleData.parameters, Direction);

		GUILayout.Label("at distance", RuleGUI.ruleLabelStyle);

		Distance = RuleGUI.ShowParameter(Distance);
		ChangeParameter("Distance", ruleData.parameters, Distance);

		GUILayout.Label("with a start speed of", RuleGUI.ruleLabelStyle);

		StartVelocity = RuleGUI.ShowParameter(StartVelocity, "startVelocity");
		ChangeParameter("StartVelocity", ruleData.parameters, StartVelocity);

		GUILayout.Label("and tag it as a", RuleGUI.ruleLabelStyle);

		int resultIndex = tagDropDown.Draw();
		if (resultIndex > -1)
		{
			TagForSpawnedObjects = tagDropDown.Content[resultIndex].text;
		}
	}

	protected override void React(GameEventData eventData)
	{
		if (SpawnedPrefabs == null || SpawnedPrefabs.Count == 0) return;

		string spawn = SpawnedPrefabs[Random.Range(0, SpawnedPrefabs.Count)];

		GameObject spawnedObjectPrefab = Resources.Load(spawn) as GameObject;
		if (spawnedObjectPrefab == null)
		{
			Debug.LogWarning("Couldn't spawn object '" + spawn + "'. It wasn't found in any of the Resources folders or is no GameObject.");
		}
		else
		{
			GameObject spawnedObject = Instantiate(spawnedObjectPrefab) as GameObject;

			AddNewSpawnedObject(spawnedObject);

			Vector3 dir = Reactor.transform.TransformDirection(Direction);

			// set object and rotation
			spawnedObject.transform.position = Reactor.transform.position + dir * Distance;
			if (RandomRotation)
			{
				spawnedObject.transform.rotation = Quaternion.Euler(0, Random.Range(0, 3) * 90, 0);
			}
			spawnedObject.tag = TagForSpawnedObjects;

			if (spawnedObject.rigidbody)
				spawnedObject.rigidbody.AddForce(Reactor.transform.TransformDirection(StartVelocity), ForceMode.Impulse);

			// handle deactivation of this reaction
			if (DeactivateAfterSpawning)
			{
				Unregister();
				gameObject.SetActive(false);
			}
		}
	}

	void AddNewSpawnedObject(GameObject go)
	{
		spawnedObjects.Enqueue(go);

		while (spawnedObjects.Count > maxSpawnedObjects)
		{
			Destroy(spawnedObjects.Dequeue());
		}
			
	}
}
