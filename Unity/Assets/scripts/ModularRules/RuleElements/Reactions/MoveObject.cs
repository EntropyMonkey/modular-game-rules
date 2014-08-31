using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveObject : Reaction
{
	public Direction MoveDirection;

	public float MoveSpeed = 10;

	public RelativeTo DirectionRelativeTo = RelativeTo.SELF;

	public Actor ActorDirectionIsRelativeTo;

	public bool RotateWithMovement = true;
	public float RotationSpeed;

	private ActorDropDown actorDropDown;
	private DropDown moveDirectionDropdown;
	private DropDown relativeToDropdown;
	private ActorDropDown relativeActorDropDown;

	RuleGenerator generator;

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		this.generator = generator;

		generator.OnActorGOChanged += delegate(ActorData data, Actor newActor, RuleGenerator ruleGenerator)
		{
			if (ActorDirectionIsRelativeTo != null && ActorDirectionIsRelativeTo.Id == data.id)
			{
				ActorDirectionIsRelativeTo = newActor;
			}
		};

		if (Reactor.rigidbody == null)
			Reactor.gameObject.AddComponent<Rigidbody>();

		Reactor.rigidbody.freezeRotation = true;
		Reactor.rigidbody.useGravity = false;

		string[] actors = generator.Gui.ActorNames;
		actorDropDown = new ActorDropDown(
			System.Array.FindIndex(actors, item => item == Reactor.Label),
			actors,
			ref generator.Gui.OnAddedActor, ref generator.Gui.OnRenamedActor, ref generator.Gui.OnDeletedActor);

		moveDirectionDropdown = new DropDown((int)MoveDirection, System.Enum.GetNames(typeof(Direction)));
		relativeToDropdown = new DropDown((int)DirectionRelativeTo, System.Enum.GetNames(typeof(RelativeTo)));

		relativeActorDropDown = new ActorDropDown(
			ActorDirectionIsRelativeTo != null ?
				System.Array.FindIndex(actors, item => item == ActorDirectionIsRelativeTo.Label) :
				0,
			actors,
			ref generator.Gui.OnAddedActor, ref generator.Gui.OnRenamedActor, ref generator.Gui.OnDeletedActor);
	}

	void OnEnable()
	{
		Register();
	}

	void OnDisable()
	{
		Unregister();
	}

	#region RuleInfo
	public override RuleData GetRuleInformation()
	{
		ReactionData rule = base.GetRuleInformation() as ReactionData;

		if (rule == null) return rule;

		rule.parameters = new List<Param>();
		rule.parameters.Add(new Param()
		{
			name = "MoveDirection",
			type = MoveDirection.GetType(),
			value = MoveDirection
		});
		rule.parameters.Add(new Param()
		{
			name = "MoveSpeed",
			type = MoveSpeed.GetType(),
			value = MoveSpeed
		});
		rule.parameters.Add(new Param()
		{
			name = "DirectionRelativeTo",
			type = DirectionRelativeTo.GetType(),
			value = DirectionRelativeTo
		});
		if (ActorDirectionIsRelativeTo != null)
		{
			rule.parameters.Add(new Param()
			{
				name = "ActorDirectionIsRelativeTo",
				type = ActorDirectionIsRelativeTo.GetType(),
				value = ActorDirectionIsRelativeTo.Id
			});
		}
		rule.parameters.Add(new Param()
		{
			name = "RotateWithMovement",
			type = RotateWithMovement.GetType(),
			value = RotateWithMovement
		});
		rule.parameters.Add(new Param()
		{
			name = "RotationSpeed",
			type = RotationSpeed.GetType(),
			value = RotationSpeed
		});

		return rule;
	}
	#endregion

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("move", RuleGUI.ruleLabelStyle);

		int resultIndex = actorDropDown.Draw();
		if (resultIndex > -1)
		{
			int resultId = generator.Gui.GetActorDataByLabel(actorDropDown.Content[resultIndex].text).id;
			if (resultId != Reactor.Id)
				generator.ChangeActor(this, resultId);
			(ruleData as ReactionData).actorId = resultId;
		}

		GUILayout.Label("relative to", RuleGUI.ruleLabelStyle);
		DirectionRelativeTo = (RelativeTo)relativeToDropdown.Draw();
		ChangeParameter("DirectionRelativeTo", (ruleData as ReactionData).parameters, DirectionRelativeTo);

		if (DirectionRelativeTo == RelativeTo.ACTOR)
		{
			resultIndex = relativeActorDropDown.Draw();
			if (resultIndex > -1)
			{
				int resultId = generator.Gui.GetActorDataByLabel(relativeActorDropDown.Content[resultIndex].text).id;
				ActorDirectionIsRelativeTo = generator.GetActor(resultId);
				ChangeParameter("ActorDirectionIsRelativeTo", (ruleData as ReactionData).parameters, ActorDirectionIsRelativeTo);
			}
		}

		GUILayout.Label("in direction: ", RuleGUI.ruleLabelStyle);

		MoveDirection = (Direction)moveDirectionDropdown.Draw();
		ChangeParameter("MoveDirection", (ruleData as ReactionData).parameters, MoveDirection);

		GUILayout.Label("with a speed of", RuleGUI.ruleLabelStyle);

		MoveSpeed = RuleGUI.ShowParameter((float)MoveSpeed);
		ChangeParameter("MoveSpeed", (ruleData as ReactionData).parameters, MoveSpeed);

		GUILayout.Label("units.", RuleGUI.ruleLabelStyle);
	}

	protected override void React(GameEventData eventData)
	{
		if (DirectionRelativeTo == RelativeTo.ACTOR && ActorDirectionIsRelativeTo == null)
			return;

		float v;
		if (eventData != null && eventData.Get(EventDataKeys.InputData) != null)
		{
			v = ((InputReceived.InputData)eventData.Get(EventDataKeys.InputData).data).inputValue;
		}
		else
			v = 1;

		Transform relevantTransform;
		if (DirectionRelativeTo == RelativeTo.ACTOR)
			relevantTransform = ActorDirectionIsRelativeTo.transform;
		else if (DirectionRelativeTo == RelativeTo.SELF)
			relevantTransform = Reactor.transform;
		else // world
			relevantTransform = new GameObject("origin").transform;

		Vector3 dir = Vector3.zero;
		if (DirectionRelativeTo != RelativeTo.ACTOR)
		{
			switch (MoveDirection)
			{
				case Direction.FORWARD:
					dir = relevantTransform.forward;
					dir.y = 0;
					break;
				case Direction.BACKWARD:
					dir = -relevantTransform.forward;
					dir.y = 0;
					break;
				case Direction.LEFT:
					dir = -relevantTransform.right;
					dir.y = 0;
					break;
				case Direction.RIGHT:
					dir = relevantTransform.right;
					dir.y = 0;
					break;
				case Direction.UP:
					dir = relevantTransform.up;
					break;
				case Direction.DOWN:
					dir = -relevantTransform.up;
					break;
			}
		}
		else
		{
			dir = (relevantTransform.position - Reactor.transform.position).normalized;
			switch (MoveDirection)
			{
				case Direction.FORWARD:
					dir.y = 0;
					break;
				case Direction.BACKWARD:
					dir = -dir;
					dir.y = 0;
					break;
				case Direction.LEFT:
					dir = -relevantTransform.right;
					dir.y = 0;
					break;
				case Direction.RIGHT:
					dir = relevantTransform.right;
					dir.y = 0;
					break;
				case Direction.UP:
					dir = relevantTransform.up;
					break;
				case Direction.DOWN:
					dir = -relevantTransform.up;
					break;
			}
		}

		Reactor.rigidbody.AddForce(dir * v * MoveSpeed);

		if (relevantTransform.name == "origin")
			Destroy(relevantTransform.gameObject);


		if (RotateWithMovement)
		{
			Vector3 horizontalVelocity = Reactor.rigidbody.velocity;
			horizontalVelocity.y = 0;
			Reactor.transform.forward = Vector3.Lerp(Reactor.transform.forward, horizontalVelocity, RotationSpeed * Time.deltaTime);
		}
	}
}
