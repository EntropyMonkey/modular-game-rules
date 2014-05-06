using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
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
			RuleData result = base.GetRuleInformation();

			if (result.parameters == null)
				result.parameters = new List<Param>();

			result.parameters.Add(new Param()
			{
				name = "CollideWithTag",
				type = CollideWithTag.GetType(),
				value = CollideWithTag
			});
			result.parameters.Add(new Param()
			{
				name = "TriggerOn",
				type = TriggerOn.GetType(),
				value = TriggerOn
			});

			return result;
		}

		public override void Initialize(RuleGenerator generator)
		{
			base.Initialize(generator);

			if (relay == null)
				relay = Actor.gameObject.AddComponent<CollisionEventRelay>();

			relay.OnTriggerEnter_Relay += OnTriggerEnter;
			relay.OnTriggerStay_Relay += OnTriggerStay;
			relay.OnTriggerExit_Relay += OnTriggerExit;

			relay.OnCollisionEnter_Relay += OnCollisionEnter;
			relay.OnCollisionStay_Relay += OnCollisionStay;
			relay.OnCollisionExit_Relay += OnCollisionExit;
		}

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
}
