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
		};

		public class ActorData : RuleData
		{
			public System.Type type;
		};

		// parameter for events or reactions
		public struct Param
		{
			public string name;
			public System.Type type;
			public object value;
		};

		public class EventData : ActorData
		{
			public int actorId;

			public List<Param> parameters;
		};

		public class ReactionData : EventData
		{
			public int eventId;
		};

		[HideInInspector]
		public int Id;

		// called upon generation
		public virtual void Initialize()
		{

		}

		/// <summary>
		/// Called when the component needs to be destroyed. Also destroys all requiredcomponent comps
		/// </summary>
		public virtual void SelfDestroy()
		{

		}

		public abstract RuleData GetRuleInformation();
	}
}
