using UnityEngine;
using System.Collections;

using ModularRules;

public class Hittable : Actor, ICount
{
	void Start()
	{
		InitializeActor();
	}

	public void ChangeCount(EventData data, float changeBy)
	{
		Debug.Log(1);
		if (!rigidbody.useGravity)
			rigidbody.useGravity = true;
		else
			rigidbody.AddForce(Vector3.up * 100);
	}
}
