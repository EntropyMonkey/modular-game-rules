using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class CollisionEvent : GameEvent
{
	public string CollideWithTag;
	public Actor CollideWithActor;

	public static List<string> PossibleCollisionTags = new List<string>() { Player.Tag, Level.Tag, "NPC", "Fruit", "Danger", "Collectible" };

	public CollisionPhase TriggerOn = CollisionPhase.ANY;

	private CollisionEventRelay relay;

	private ActorDropDown actorDropDown;
	private DropDown chooseObjectKindDropDown;
	private ActorDropDown collisionActorDropDown;
	private DropDown tagDropDown;
	private DropDown triggerDropDown;

	private RuleGenerator generator;

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
			name = "CollideWithObject",
			type = typeof(Actor),
			value = CollideWithActor
		});
		rule.parameters.Add(new Param()
		{
			name = "TriggerOn",
			type = TriggerOn.GetType(),
			value = TriggerOn
		});

			return rule;
	}

	#region Initialize
	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		this.generator = generator;

		generator.OnActorGOChanged += delegate(ActorData data, Actor newActor, RuleGenerator ruleGenerator)
		{
			if (CollideWithActor != null && CollideWithActor.Id == data.id)
				CollideWithActor = newActor;
		};

		// setting up gui elements - actors
		string[] actors = generator.Gui.ActorNames;
		actorDropDown = new ActorDropDown(
			System.Array.FindIndex(actors, item => item == Actor.Label), 
			actors,
			ref generator.Gui.OnAddedActor, ref generator.Gui.OnRenamedActor, ref generator.Gui.OnDeletedActor);

		// gui - tags
		tagDropDown = new DropDown(PossibleCollisionTags.FindIndex(item => item == CollideWithTag), PossibleCollisionTags.ToArray());

		// gui - collision with other actor
		int index = 0;
		if (CollideWithActor != null)
			index = System.Array.FindIndex(actors, item => item == CollideWithActor.Label);
		collisionActorDropDown = new ActorDropDown(index, actors,
			ref generator.Gui.OnAddedActor, ref generator.Gui.OnRenamedActor, ref generator.Gui.OnDeletedActor);

		// gui - triggering
		triggerDropDown = new DropDown((int)TriggerOn, System.Enum.GetNames(typeof(CollisionPhase)));

		// gui - choosing which kinds of objects to trigger
		int init = 0;
		if (CollideWithActor != null)
			init = 1;
		chooseObjectKindDropDown = new DropDown(init, new string[] { "any object which is a", "the actor" });

		SubscribeRelay();
	}

	void OnEnable()
	{
		SubscribeRelay();
	}

	void OnDisable()
	{
		UnsubscribeRelay();
	}
	#endregion

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("On Collision of", RuleGUI.ruleLabelStyle);

		// actor dropdown
		int resultIndex = actorDropDown.Draw();
		if (resultIndex > -1)
		{
			int resultId = generator.Gui.GetActorDataByLabel(actorDropDown.Content[resultIndex].text).id;

			(ruleData as EventData).actorId = resultId;
			if (Actor.Id != resultId)
				generator.ChangeActor(this, resultId);
		}

		GUILayout.Label("and", RuleGUI.ruleLabelStyle);

		resultIndex = chooseObjectKindDropDown.Draw();
		if (resultIndex == 0) // tag
		{
			CollideWithActor = null;
			// tag dropdown
			resultIndex = tagDropDown.Draw();
			if (resultIndex >= 0)
			{
				string resultTag = tagDropDown.Content[resultIndex].text;

				if (resultTag.Length > 0)
				{
					CollideWithTag = resultTag;
					ChangeParameter("CollideWithTag", (ruleData as EventData).parameters, CollideWithTag);
				}
			}
		}
		else if (resultIndex == 1) // actor
		{
			resultIndex = collisionActorDropDown.Draw();
			if (resultIndex > -1)
			{
				int resultId = generator.Gui.GetActorDataByLabel(collisionActorDropDown.Content[resultIndex].text).id;

				CollideWithActor = generator.GetActor(resultId);
				ChangeParameter("CollideWithActor", ruleData.parameters, CollideWithActor.Id);
			}
		}

		GUILayout.Label("when", RuleGUI.ruleLabelStyle);

		// trigger dropdown
		resultIndex = triggerDropDown.Draw();
		if (resultIndex >= 0)
		{
			TriggerOn = (CollisionPhase)resultIndex;
			ChangeParameter("TriggerOn", (ruleData as EventData).parameters, TriggerOn);
		}

		GUILayout.Label("collision", RuleGUI.ruleLabelStyle);
	}

	#region Reset
	public override void ResetGenerationData()
	{
		base.ResetGenerationData();

		UnsubscribeRelay();
	}
	#endregion

	void SubscribeRelay()
	{
		// setting up collision relay
		if (Actor == null) return;

		if (relay == null)
		{
			relay = Actor.gameObject.GetComponent<CollisionEventRelay>();
			if (relay == null)
			{
				relay = Actor.gameObject.AddComponent<CollisionEventRelay>();
				relay.UsedCount = 1;
			}
			else
				relay.UsedCount++;

			relay.OnTriggerEnter_Relay += OnTriggerEnter;
			relay.OnTriggerStay_Relay += OnTriggerStay;
			relay.OnTriggerExit_Relay += OnTriggerExit;

			relay.OnCollisionEnter_Relay += OnCollisionEnter;
			relay.OnCollisionStay_Relay += OnCollisionStay;
			relay.OnCollisionExit_Relay += OnCollisionExit;
		}

#if UNITY_EDITOR
		//relay.OnTriggerEnter_Relay += delegate(Collider other) { Debug.Log("Enter trigger " + other.name); };
		////relay.OnTriggerStay_Relay += DebugTriggers;
		//relay.OnTriggerExit_Relay += DebugTriggers;

		//relay.OnCollisionEnter_Relay += delegate(Collision collision) { Debug.Log("Enter collision " + collision.collider.name); };
		////relay.OnCollisionStay_Relay += DebugCollisions;
		//relay.OnCollisionExit_Relay += DebugCollisions;
#endif
	}

	void UnsubscribeRelay()
	{
		if (relay == null) return;

		relay.UsedCount--;
		if (relay.UsedCount <= 0)
		{
			Destroy(relay);
		}

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
		Actor a = collision.collider.GetComponent(typeof(Actor)) as Actor;
		if ( ((CollideWithActor != null && a != null && a.Id == CollideWithActor.Id) || 
			collision.collider.tag == CollideWithTag) && 
			(TriggerOn == CollisionPhase.ENTER || TriggerOn == CollisionPhase.ANY))
		{
			Trigger(new GameEventData()
				.Add(new DataPiece(EventDataKeys.TargetObject)
				{
					data = collision.collider.gameObject
				})
				.Add(new DataPiece(EventDataKeys.OriginObject)
				{
					data = gameObject
				})
				.Add(new DataPiece(EventDataKeys.RelativeVelocity)
				{
					data = collision.relativeVelocity
				}));
		}
	}

	void OnCollisionStay(Collision collision)
	{
		Actor a = collision.collider.GetComponent(typeof(Actor)) as Actor;
		if (((CollideWithActor != null && a != null && a.Id == CollideWithActor.Id) ||
			collision.collider.tag == CollideWithTag) && 
			(TriggerOn == CollisionPhase.STAY || TriggerOn == CollisionPhase.ANY))
		{
			Trigger(new GameEventData()
				.Add(new DataPiece(EventDataKeys.TargetObject)
				{
					data = collision.collider.gameObject
				})
				.Add(new DataPiece(EventDataKeys.OriginObject)
				{
					data = gameObject
				})
				.Add(new DataPiece(EventDataKeys.RelativeVelocity)
				{
					data = collision.relativeVelocity
				}));
		}
	}

	void OnCollisionExit(Collision collision)
	{
		Actor a = collision.collider.GetComponent(typeof(Actor)) as Actor;
		if (((CollideWithActor != null && a != null && a.Id == CollideWithActor.Id) ||
			collision.collider.tag == CollideWithTag) && 
			(TriggerOn == CollisionPhase.EXIT || TriggerOn == CollisionPhase.ANY))
		{
			Trigger(new GameEventData()
				.Add(new DataPiece(EventDataKeys.TargetObject)
				{
					data = collision.collider.gameObject
				})
				.Add(new DataPiece(EventDataKeys.OriginObject)
				{
					data = gameObject
				})
				.Add(new DataPiece(EventDataKeys.RelativeVelocity)
				{
					data = collision.relativeVelocity
				}));
		}
	}
	#endregion

	#region OnTrigger
	void OnTriggerEnter(Collider other)
	{
		Actor a = other.GetComponent(typeof(Actor)) as Actor;
		if (((CollideWithActor != null && a != null && a.Id == CollideWithActor.Id) ||
			other.collider.tag == CollideWithTag) &&
			(TriggerOn == CollisionPhase.ENTER || TriggerOn == CollisionPhase.ANY))
		{
			Trigger(new GameEventData()
				.Add(new DataPiece(EventDataKeys.TargetObject)
				{
					data = other.gameObject
				})
				.Add(new DataPiece(EventDataKeys.OriginObject)
				{
					data = gameObject
				}));
		}
	}

	void OnTriggerStay(Collider other)
	{
		Actor a = other.GetComponent(typeof(Actor)) as Actor;
		if (((CollideWithActor != null && a != null && a.Id == CollideWithActor.Id) ||
			other.collider.tag == CollideWithTag) &&
			(TriggerOn == CollisionPhase.STAY || TriggerOn == CollisionPhase.ANY))
		{
			Trigger(new GameEventData()
				.Add(new DataPiece(EventDataKeys.TargetObject)
				{
					data = other.gameObject
				})
				.Add(new DataPiece(EventDataKeys.OriginObject)
				{
					data = gameObject
				}));
		}
	}

	void OnTriggerExit(Collider other)
	{
		Actor a = other.GetComponent(typeof(Actor)) as Actor;
		if (((CollideWithActor != null && a != null && a.Id == CollideWithActor.Id) ||
			other.collider.tag == CollideWithTag) &&
			(TriggerOn == CollisionPhase.EXIT || TriggerOn == CollisionPhase.ANY))
		{
			Trigger(new GameEventData()
				.Add(new DataPiece(EventDataKeys.TargetObject)
				{
					data = other.gameObject
				})
				.Add(new DataPiece(EventDataKeys.OriginObject)
				{
					data = gameObject
				}));
		}
	}

	#endregion
}

