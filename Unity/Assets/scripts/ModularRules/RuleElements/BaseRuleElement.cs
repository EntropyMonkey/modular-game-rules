using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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

		public delegate void ShowGuiDelegate(RuleData ruleData);
		public ShowGuiDelegate OnShowGui;
	};

	public class ActorData : RuleData, IDeepCopy<ActorData>
	{
		//public List<ComponentData> components;

		public string prefab;

		public ActorData DeepCopy()
		{
			ActorData result = new ActorData();
			result.id = id;
			result.label = label;
			result.prefab = prefab;
			result.type = type;
			result.OnShowGui = OnShowGui;
			result.parameters = DeepCopyParams(parameters);
			//result.components = components;

			return result;
		}
	};

	//public class ComponentData : RuleData
	//{

	//}

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
			result.OnShowGui = OnShowGui;
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
			result.OnShowGui = OnShowGui;
			result.parameters = DeepCopyParams(parameters);
			result.actorId = actorId;
			result.eventId = eventId;

			return result;
		}
	};

	static List<Param> DeepCopyParams(List<Param> orig)
	{
		if (orig == null) return null;

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
		//generator.OnActorNameChanged += OnActorNameChanged;
	}

	public abstract void ShowGui(RuleData ruleData);

	/// <summary>
	/// Called when the component needs to be reset
	/// </summary>
	public virtual void Reset()
	{
	}

	public virtual void OnActorNameChanged(int id, string oldName, string newName)
	{
	}

	public abstract RuleData GetRuleInformation();

	protected void ChangeParameter<T>(string name, List<Param> parameters, T newValue)
	{
		int index = parameters.FindIndex(item => item.name == name);

		Param param;
		if (index < 0) // add new parameter if not found
		{
			param = new Param()
			{
				name = name,
			};

			if (newValue.GetType().IsEnum)
			{
				param.value = (int)Convert.ChangeType((object)newValue, typeof(int));
			}
			else if (newValue.GetType().IsAssignableFrom(typeof(Actor)))
			{
				param.value = ((Actor)Convert.ChangeType((object)newValue, typeof(Actor))).Id;
			}
			else
			{
				param.value = newValue;
			}

			param.type = typeof(T);
		}
		else // update existing parameter
		{
			param = parameters[index];

			if (param.type != newValue.GetType() && !param.type.IsEnum && !newValue.GetType().IsAssignableFrom(typeof(Actor)))
			{
				return;
			}
			else if ((param.type == typeof(Actor) && !(newValue is int) && newValue.GetType().IsAssignableFrom(typeof(Actor))))
			{
				param.value = ((Actor)Convert.ChangeType((object)newValue, typeof(Actor))).Id;
			}
			else if (param.type.IsEnum && !(newValue is int))
			{
				if (newValue.GetType().IsEnum)
				{
					parameters.RemoveAt(index);
					param.value = (int)Convert.ChangeType((object)newValue, typeof(int));
					parameters.Add(param);
					return;
				}
				else
				{
					return;
				}
			}

			parameters.RemoveAt(index);

			param.value = newValue;
		}

		parameters.Add(param);
	}

}
