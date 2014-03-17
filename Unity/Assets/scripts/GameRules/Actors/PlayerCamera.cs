using UnityEngine;
using System.Collections;
using ModularRules;

public class PlayerCamera : Actor
{
	public bool AttachedToPlayer = true;

	void Start()
	{
		InitializeActor();

		if (AttachedToPlayer)
		{
			transform.parent = GameObject.FindGameObjectWithTag("Player").transform;
			transform.localPosition = Vector3.zero;
		}
	}
}
