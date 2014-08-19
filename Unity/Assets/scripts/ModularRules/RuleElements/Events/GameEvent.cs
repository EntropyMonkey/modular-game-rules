using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

	//#region Condition Handling
	//private List<GameEventCondition> conditions = new List<GameEventCondition>();
	//public void AddCondition(GameEventCondition condition)
	//{
	//	if (!conditions.Contains(condition))
	//		conditions.Add(condition);
	//}
	//public void RemoveCondition(GameEventCondition condition)
	//{
	//	conditions.Remove(condition);
	//}
	//public bool ConditionsMet()
	//{
	//	foreach (GameEventCondition c in conditions)
	//	{
	//		if (!c.IsTrue)
	//			return false;
	//	}
	//	return true;
	//}
	//#endregion

	public override RuleData GetRuleInformation()
	{
		return new EventData()
		{
			id = Id,
			actorId = Actor.Id,
			type = this.GetType(),
			label = this.GetType().ToString(),
			OnShowGui = ShowGui
		};
	}

	public abstract GameEvent UpdateEvent();

	/// <summary>
	/// Trigger the event
	/// </summary>
	/// <param name="data">fill in event data</param>
	/// <returns>true once all reactions were executed, false if conditions aren't met</returns>
	public bool Trigger(GameEventData data)
	{
		//if (!ConditionsMet()) return false;

		for (int i = 0; i < triggeredReactions.Count; i++ )
		{
			if (triggeredReactions[i] != null)
			{
#if UNITY_EDITOR
				EditorGUIUtility.PingObject(Actor.gameObject);
				EditorGUIUtility.PingObject(gameObject);
#endif
				triggeredReactions[i].Execute(data);
			}
		}

		return true;
	}
}
