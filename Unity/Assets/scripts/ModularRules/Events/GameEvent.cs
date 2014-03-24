using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public abstract class GameEvent : BaseRuleElement
	{
		[HideInInspector]
		public Actor Actor;

		#region Reaction Handling
		private List<Reaction> triggeredReactions = new List<Reaction>();

		public void Register(Reaction newReaction)
		{
			if (!triggeredReactions.Contains(newReaction))
			{
				triggeredReactions.Add(newReaction);
			}
		}

		public void Unregister(Reaction oldReaction)
		{
			triggeredReactions.Remove(oldReaction);
		}
		#endregion

		#region Condition Handling
		private List<GameEventCondition> conditions = new List<GameEventCondition>();
		public void AddCondition(GameEventCondition condition)
		{
			if (!conditions.Contains(condition))
				conditions.Add(condition);
		}
		public void RemoveCondition(GameEventCondition condition)
		{
			conditions.Remove(condition);
		}
		public bool ConditionsMet()
		{
			foreach (GameEventCondition c in conditions)
			{
				if (!c.IsTrue)
					return false;
			}
			return true;
		}
		#endregion

		public abstract GameEvent UpdateEvent();

		/// <summary>
		/// Trigger the event
		/// </summary>
		/// <param name="data">fill in event data</param>
		/// <returns>true once all reactions were executed, false if conditions aren't met</returns>
		public bool Trigger(EventData data)
		{
			if (!ConditionsMet()) return false;

			foreach(Reaction r in triggeredReactions)
			{
				r.Execute(data);
			}

			return true;
		}
	}
}
