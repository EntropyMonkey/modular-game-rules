using UnityEngine;
using System.Collections;
using ModularRules;
using System.Collections.Generic;

namespace ModularRules
{
	public enum Direction { FORWARD = 0, BACKWARD, LEFT, RIGHT, UP, DOWN }

	public enum RelativeTo { WORLD = 0, CAMERA, SELF }

	public class MoveObject : Reaction
	{
		public Direction MoveDirection;

		public float MoveSpeed = 10;
		public float MaxSpeed = 10;

		public RelativeTo DirectionRelativeTo = RelativeTo.SELF;

		public Actor ActorDirectionIsRelativeTo;

		public bool RotateWithMovement = true;
		public float RotationSpeed;

		public override void Initialize()
		{
			base.Initialize();

			if (Reactor.rigidbody == null)
				Reactor.gameObject.AddComponent<Rigidbody>();

			Reactor.rigidbody.freezeRotation = true;
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
				name = "MaxSpeed",
				type = MaxSpeed.GetType(),
				value = MaxSpeed
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

		protected override void React(GameEventData eventData)
		{
			if (eventData == null ||
				(DirectionRelativeTo == RelativeTo.CAMERA && ActorDirectionIsRelativeTo == null)) 
				return;

			float v;
			if (eventData.Get(EventDataKeys.InputData) != null)
			{
				v = ((InputReceived.InputData)eventData.Get(EventDataKeys.InputData).data).inputValue;
			}
			else
				v = 1;

			Transform relevantTransform;
			if (DirectionRelativeTo == RelativeTo.CAMERA)
				relevantTransform = ActorDirectionIsRelativeTo.transform;
			else if (DirectionRelativeTo == RelativeTo.SELF)
				relevantTransform = transform;
			else
				relevantTransform = new GameObject("origin").transform;

			Vector3 dir = Vector3.zero;
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

			if (Reactor.rigidbody.velocity.magnitude < MaxSpeed)
				Reactor.rigidbody.AddForce(dir * v * MoveSpeed);

			if (relevantTransform.name == "origin")
				Destroy(relevantTransform.gameObject);

			if (RotateWithMovement)
				Reactor.transform.forward = Vector3.Lerp(Reactor.transform.forward, Reactor.rigidbody.velocity.normalized, RotationSpeed * Time.deltaTime);
		//	Reactor.transform.Rotate(Vector3.up, 1 * (MoveDirection == Direction.LEFT ? -1 : MoveDirection == Direction.RIGHT ? 1 : 0));
		}
	}
}
