using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionEvent : GameEvent
{
	public class CollisionData : GameEventData
	{
		public Collider otherCollider;
		public Collider thisCollider;

		public CollisionPhase collisionPhase;

		public Vector3 relativeVelocity;
	}

	public string CollideWithTag;

	public CollisionPhase TriggerOn = CollisionPhase.ANY;

	private CollisionEventRelay relay;

	public override RuleData GetRuleInformation()
	{
		RuleData rule = base.GetRuleInformation();

		if (rule.parameters == null)
			rule.parameters = new List<Param>();

		rule.parameters.Add(new Param()
		{
			name = "CollideWithTag",
			type = CollideWithTag.GetType(),
			value = CollideWithTag
		});
		rule.parameters.Add(new Param()
		{
			name = "TriggerOn",
			type = TriggerOn.GetType(),
			value = TriggerOn
		});

		// guistuff
		rule.guiPrefix = "On";
		rule.guiName = "Collision";

		rule.guiParams.Add(new Param()
		{
			name = "CollisionActor",
			type = this.GetType(),
			value = Id,
			guiPostfix = "and"
		});
		rule.guiParams.Add(new Param()
		{
			name = "CollideWithTag",
			type = CollideWithTag.GetType(),
			value = CollideWithTag,
			guiPostfix = "(tag)"
		});

		return rule;
	}

	void Awake()
	{
		// HACK for spawning objects
		//Initialize(GameObject.FindGameObjectWithTag(RuleGenerator.Tag).GetComponent<RuleGenerator>());
	}

	#region Initialize
	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		relay = Actor.gameObject.GetComponent<CollisionEventRelay>();
		if (relay == null)
		{
			relay = Actor.gameObject.AddComponent<CollisionEventRelay>();
			relay.UsedCount = 1;
		}
		else
			relay.UsedCount++;

		SubscribeRelay();
	}
	#endregion

	#region Reset
	public override void Reset()
	{
		base.Reset();

		UnsubscribeRelay();

		relay.UsedCount--;

		if (relay.UsedCount <= 0)
		{
			Destroy(relay);
		}
	}
	#endregion

	void SubscribeRelay()
	{
		relay.OnTriggerEnter_Relay += OnTriggerEnter;
		relay.OnTriggerStay_Relay += OnTriggerStay;
		relay.OnTriggerExit_Relay += OnTriggerExit;

		relay.OnCollisionEnter_Relay += OnCollisionEnter;
		relay.OnCollisionStay_Relay += OnCollisionStay;
		relay.OnCollisionExit_Relay += OnCollisionExit;

#if UNITY_EDITOR
		//relay.OnTriggerEnter_Relay += DebugTriggers;
		////relay.OnTriggerStay_Relay += DebugTriggers;
		//relay.OnTriggerExit_Relay += DebugTriggers;

		//relay.OnCollisionEnter_Relay += DebugCollisions;
		////relay.OnCollisionStay_Relay += DebugCollisions;
		//relay.OnCollisionExit_Relay += DebugCollisions;
#endif
	}

	void UnsubscribeRelay()
	{
		relay.OnTriggerEnter_Relay -= OnTriggerEnter;
		relay.OnTriggerStay_Relay -= OnTriggerStay;
		relay.OnTriggerExit_Relay -= OnTriggerExit;

		relay.OnCollisionEnter_Relay -= OnCollisionEnter;
		relay.OnCollisionStay_Relay -= OnCollisionStay;
		relay.OnCollisionExit_Relay -= OnCollisionExit;

#if UNITY_EDITOR
		//relay.OnTriggerEnter_Relay -= DebugTriggers;
		////relay.OnTriggerStay_Relay -= DebugTriggers;
		//relay.OnTriggerExit_Relay -= DebugTriggers;

		//relay.OnCollisionEnter_Relay -= DebugCollisions;
		////relay.OnCollisionStay_Relay -= DebugCollisions;
		//relay.OnCollisionExit_Relay -= DebugCollisions;
#endif
	}

	//void DebugCollisions(Collision collision)
	//{
	//	Debug.Log("Collision: " + collision.collider);
	//}

	//void DebugTriggers(Collider collider)
	//{
	//	Debug.Log("Trigger: " + collider);
	//}

	public override GameEvent UpdateEvent()
	{
		return this;
	}

	#region OnCollision
	void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.tag == CollideWithTag && 
			(TriggerOn == CollisionPhase.ENTER || TriggerOn == CollisionPhase.ANY))
		{
			Trigger(new CollisionData() {
				otherCollider = collision.collider,
				thisCollider = collider,
				collisionPhase = CollisionPhase.ENTER,
				relativeVelocity = collision.relativeVelocity
			});
		}
	}

	void OnCollisionStay(Collision collision)
	{
		if (collision.collider.tag == CollideWithTag &&
			(TriggerOn == CollisionPhase.STAY || TriggerOn == CollisionPhase.ANY))
		{
			Trigger(new CollisionData()
			{
				otherCollider = collision.collider,
				thisCollider = collider,
				collisionPhase = CollisionPhase.STAY,
				relativeVelocity = collision.relativeVelocity
			});
		}
	}

	void OnCollisionExit(Collision collision)
	{
		if (collision.collider.tag == CollideWithTag &&
			(TriggerOn == CollisionPhase.EXIT || TriggerOn == CollisionPhase.ANY))
		{
			Trigger(new CollisionData()
			{
				otherCollider = collision.collider,
				thisCollider = collider,
				collisionPhase = CollisionPhase.EXIT,
				relativeVelocity = collision.relativeVelocity
			});
		}
	}
	#endregion

	#region OnTrigger
	void OnTriggerEnter(Collider other)
	{
		if (other.collider.tag == CollideWithTag &&
			(TriggerOn == CollisionPhase.ENTER || TriggerOn == CollisionPhase.ANY))
		{
			Trigger(new CollisionData()
			{
				otherCollider = other,
				thisCollider = collider,
				collisionPhase = CollisionPhase.ENTER,
				relativeVelocity = Vector3.zero
			});
		}
	}

	void OnTriggerStay(Collider other)
	{
		if (other.collider.tag == CollideWithTag && 
			(TriggerOn == CollisionPhase.STAY || TriggerOn == CollisionPhase.ANY))
		{
			Trigger(new CollisionData()
			{
				otherCollider = other,
				thisCollider = collider,
				collisionPhase = CollisionPhase.STAY,
				relativeVelocity = Vector3.zero
			});
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.collider.tag == CollideWithTag &&
			(TriggerOn == CollisionPhase.EXIT || TriggerOn == CollisionPhase.ANY))
		{
			Trigger(new CollisionData()
			{
				otherCollider = other,
				thisCollider = collider,
				collisionPhase = CollisionPhase.EXIT,
				relativeVelocity = Vector3.zero
			});
		}
	}

	#endregion
}

