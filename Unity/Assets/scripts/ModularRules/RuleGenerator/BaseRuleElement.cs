using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseRuleElement : MonoBehaviour
{
	public interface IDeepCopy<T> where T : RuleData, new()
	{
		T DeepCopy();
	}

	public class RuleData
	{
		public int id;
		public string label;
		public System.Type type;
		public List<Param> parameters;
	};

	public class ActorData : RuleData, IDeepCopy<ActorData>
	{
		public List<ComponentData> components;

		public ActorData DeepCopy()
		{
			ActorData result = new ActorData();
			result.id = id;
			result.label = label;
			result.type = type;
			result.parameters = DeepCopyParams(parameters);
			result.components = components;

			return result;
		}
	};

	public class ComponentData : RuleData
	{

	}

	// parameter for events or reactions
	public struct Param
	{
		public string name;
		public System.Type type;
		public object value;
	};

	public class EventData : RuleData, IDeepCopy<EventData>
	{
		public int actorId;

		public EventData DeepCopy()
		{
			EventData result = new EventData();

			result.id = id;
			result.label = label;
			result.type = type;
			result.parameters = DeepCopyParams(parameters);
			result.actorId = actorId;

			return result;
		}

	};

	public class ReactionData : EventData, IDeepCopy<ReactionData>
	{
		public int eventId;

		public new ReactionData DeepCopy()
		{
			ReactionData result = new ReactionData();

			result.id = id;
			result.label = label;
			result.type = type;
			result.parameters = DeepCopyParams(parameters);
			result.actorId = actorId;
			result.eventId = eventId;

			return result;
		}
	};

	static List<Param> DeepCopyParams(List<Param> orig)
	{
		List<Param> newList = new List<Param>();
		for (int i = 0; i < orig.Count; i++)
			newList.Add(orig[i]);

		return newList;
	}

	//		[HideInInspector]
	public int Id = -1;

	private string label;
	public string Label
	{
		set
		{
			label = value;
			name = value;
		}
		get
		{
			return label;
		}
	}

	// if objects arent defined in the rules, they are usually deleted after rule generation. This enables exceptions.
	public bool DontDeleteOnLoad = false;

	// called upon generation
	public virtual void Initialize(RuleGenerator generator)
	{
		generator.RegisterRuleElement(this);
	}

	/// <summary>
	/// Called when the component needs to be reset
	/// </summary>
	public virtual void Reset()
	{
	}

	public abstract RuleData GetRuleInformation();
}
