using UnityEngine;
using System.Collections;
using ModularRules;
using System.Collections.Generic;

namespace ModularRules
{
	public enum Direction { FORWARD, BACKWARD, LEFT, RIGHT, UP, DOWN }

	public class MoveObject : Reaction
	{
		public Direction MoveDirection;

		void OnEnable()
		{
			Register();
		}

		void OnDisable()
		{
			Unregister();
		}

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

			return rule;
		}

		protected override void React(GameEventData eventData)
		{
			if (eventData == null) return;

			IMove movingObject = Reactor.gameObject.GetComponent(typeof(IMove)) as IMove;

			if (movingObject != null)
				movingObject.Move(eventData, MoveDirection);
			else
				Debug.LogWarning(name + " couldn't find a component of type IMove on " + Reactor.name + ".");
		}
	}
}
