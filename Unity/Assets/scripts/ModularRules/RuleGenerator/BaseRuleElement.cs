using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public abstract class BaseRuleElement : MonoBehaviour
	{
		public class RuleData
		{
			public int id;
			public string label;
			public System.Type type;
			public List<Param> parameters;
		};

		public class ActorData : RuleData
		{
			public List<ComponentData> components;
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

		public class EventData : RuleData
		{
			public int actorId;
		};

		public class ReactionData : EventData
		{
			public int eventId;
		};

//		[HideInInspector]
		public int Id;

		// called upon generation
		public virtual void Initialize()
		{
		}

		/// <summary>
		/// Called when the component needs to be reset
		/// </summary>
		public virtual void Reset()
		{
		}

		public abstract RuleData GetRuleInformation();
	}
}
