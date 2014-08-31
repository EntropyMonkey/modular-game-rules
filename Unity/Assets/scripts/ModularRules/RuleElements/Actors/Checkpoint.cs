using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
	public int Id
	{
		get;
		private set;
	}

	public static string Tag = "Checkpoint";

	public static string[] PossibleCheckpointTargetTags = { "Player", "NPC" };

	private static int nextFreeId = 0;

	public string TargetTag = Player.Tag;
	public float CheckpointRadius = 2;
	public Color CheckpointColor;

	private GameObject target;

	public Checkpoint()
	{
		Id = nextFreeId++;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = CheckpointColor;
		Gizmos.DrawSphere(transform.position, CheckpointRadius);
	}

	public bool IsActiveAt(Vector3 otherPosition)
	{
		if (Vector3.Distance(otherPosition, transform.position) < CheckpointRadius)
			return true;
		else
			return false;
	}
}
