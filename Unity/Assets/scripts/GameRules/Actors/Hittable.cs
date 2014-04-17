using UnityEngine;
using System.Collections;

using ModularRules;

public class Hittable : Actor, ICount
{
	void Start()
	{
		Initialize();
	}

	public void ChangeCount(GameEventData data, float changeBy)
	{
		if (!rigidbody.useGravity)
			rigidbody.useGravity = true;
		else
			rigidbody.AddForce(Vector3.up * 100);
	}
}
