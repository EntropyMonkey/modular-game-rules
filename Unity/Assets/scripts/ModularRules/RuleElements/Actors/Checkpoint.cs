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
	private static int nextFreeId = 0;

	public bool Start = false;
	public string TargetTag = Player.Tag;
	public float CheckpointSize = 2;

	private GameObject target;

	public Checkpoint()
	{
		Id = nextFreeId++;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(transform.position, CheckpointSize);
	}

	public bool IsActiveAt(Vector3 otherPosition)
	{
		if (Vector3.Distance(otherPosition, transform.position) < CheckpointSize)
			return true;
		else
			return false;
	}
}
