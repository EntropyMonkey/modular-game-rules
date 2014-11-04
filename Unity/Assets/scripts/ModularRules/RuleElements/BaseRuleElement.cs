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

	public class ActorData : RuleData, IDeepCopy<ActorData>, IMapTypeToOrder<ActorData>
	{
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

			return result;
		}
		
		enum ActorOrder { PLAYER = 0, PLAYER_CAMERA, NPC, COUNTER, LEVEL, OTHER }

		public int MapTypeToOrder(ActorData actor)
		{
			int r = (int)ActorOrder.OTHER;

			if (actor.type == typeof(Player))
				r = (int)ActorOrder.PLAYER;
			else if (actor.type == typeof(PlayerCamera))
				r = (int)ActorOrder.PLAYER_CAMERA;
			else if (actor.type == typeof(NPC))
				r = (int)ActorOrder.NPC;
			else if (actor.type == typeof(Counter))
				r = (int)ActorOrder.COUNTER;
			else if (actor.type == typeof(Level))
				r = (int)ActorOrder.LEVEL;

			return r;
		}
	};

	// parameter for events or reactions
	public struct Param
	{
		public string name;
		public System.Type type;
		public object value;
	};

	public class EventData : RuleData, IDeepCopy<EventData>, IMapTypeToOrder<EventData>
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

		enum EventOrder { INPUT = 0, COUNTER, COLLISION, DISTANCE, OBJECT_MOVED, OTHER }

		public int MapTypeToOrder(EventData eventt)
		{
			int r = (int)EventOrder.OTHER;

			if (eventt.type == typeof(InputReceived))
				r = (int)EventOrder.INPUT;
			else if (eventt.type == typeof(CounterEvent))
				r = (int)EventOrder.COUNTER;
			else if (eventt.type == typeof(CollisionEvent))
				r = (int)EventOrder.COLLISION;
			else if (eventt.type == typeof(DistanceEvent))
				r = (int)EventOrder.DISTANCE;
			else if (eventt.type == typeof(ObjectMovedEvent))
				r = (int)EventOrder.OBJECT_MOVED;

			return r;
		}
	};

	public class ReactionData : RuleData, IDeepCopy<ReactionData>
	{
		public int actorId;
		public int eventId;

		public ReactionData DeepCopy()
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
	
	#region Comparer
	public interface IMapTypeToOrder<T>
	{
		int MapTypeToOrder(T e);
	}

	public class OrderComparer<T> : IComparer<T> where T : IMapTypeToOrder<T>
	{
		protected readonly Func<T, T, int> func;
		public OrderComparer(Func<T, T, int> comparerFunc)
		{
			this.func = comparerFunc;
		}

		public OrderComparer()
		{
			this.func = (e1, e2) =>
				{
					int e1o = e1.MapTypeToOrder(e1);
					int e2o = e2.MapTypeToOrder(e2);

					if (e1o > e2o)
						return 1;
					else if (e1o < e2o)
						return -1;
					else
						return 0;
				};
		}

		public int Compare(T x, T y)
		{
			return this.func(x, y);
		}
	}
	#endregion

	//		[HideInInspector]
	public int Id = -1;

	private string label;
	public string Label
	{
		set
		{
			label = value;
			if (this != null)
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

	public abstract void ShowGui(RuleData ruleData);

	/// <summary>
	/// Called when the component needs to be reset
	/// </summary>
	public virtual void ResetGenerationData()
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

			if (newValue.GetType().IsEnum) // enum handling
			{
				param.value = (int)Convert.ChangeType((object)newValue, typeof(int));
			}
			else if (newValue.GetType().IsAssignableFrom(typeof(Actor)) || newValue.GetType().IsSubclassOf(typeof(Actor))) // actor handling
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
			object oldValue = param.value;

			if (param.type != newValue.GetType() && !param.type.IsEnum && !newValue.GetType().IsAssignableFrom(typeof(Actor)))
			{
				return; // type doesn't fit expected type
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

			if (param.type != typeof(string) && !oldValue.Equals(newValue))
			{
				Analytics.LogEvent(Analytics.ruleEvent, Analytics.change_param, param.name + " " + param.type);
			}
		}

		parameters.Add(param);
	}

}
