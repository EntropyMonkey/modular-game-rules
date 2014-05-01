using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public class SpawnObject : Reaction
	{
		public List<string> ActorTypes; 

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
			RuleData data = base.GetRuleInformation();

			if (data.parameters == null) data.parameters = new List<Param>();

			string types = "";
			foreach (string s in ActorTypes) types += s + " ";

			data.parameters.Add(new Param()
				{
					name = "ActorTypes",
					type = ActorTypes.GetType(),
					value = types
				});

			return data;
		}

		protected override void React(GameEventData eventData)
		{

		}
	}
}
