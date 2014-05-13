using UnityEngine;
using System.Collections;

public class CollisionEventRelay : MonoBehaviour
{
	public delegate void OnTriggerRelay(Collider other);
	public event OnTriggerRelay OnTriggerEnter_Relay;
	public event OnTriggerRelay OnTriggerStay_Relay;
	public event OnTriggerRelay OnTriggerExit_Relay;

	public delegate void OnCollisionRelay(Collision collision);
	public event OnCollisionRelay OnCollisionEnter_Relay;
	public event OnCollisionRelay OnCollisionStay_Relay;
	public event OnCollisionRelay OnCollisionExit_Relay;

	void Start()
	{
		int callbacks = 0;
		if (OnCollisionExit_Relay != null)
			callbacks += OnCollisionExit_Relay.GetInvocationList().Length;
	}

	void OnTriggerEnter(Collider other)
	{
		if (OnTriggerEnter_Relay != null)
			OnTriggerEnter_Relay(other);
	}

	void OnTriggerStay(Collider other)
	{
		if (OnTriggerStay_Relay != null)
			OnTriggerStay_Relay(other);
	}

	void OnTriggerExit(Collider other)
	{
		if (OnTriggerExit_Relay != null)
			OnTriggerExit_Relay(other);
	}

	void OnCollisionEnter(Collision collision)
	{
		if (OnCollisionEnter_Relay != null)
			OnCollisionEnter_Relay(collision);
	}

	void OnCollisionStay(Collision collision)
	{
		if (OnCollisionStay_Relay != null)
			OnCollisionStay_Relay(collision);
	}

	void OnCollisionExit(Collision collision)
	{
		if (OnCollisionExit_Relay != null)
			OnCollisionExit_Relay(collision);
	}
}
