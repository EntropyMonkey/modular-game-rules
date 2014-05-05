using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class ChangeCounter : Reaction
	{
		public int ChangeBy = -1;
		public string CounterName = "";

		private Counters counters;

		void OnEnable()
		{
			Register();

			if ((counters = Reactor.GetComponent<Counters>()) == null)
			{
				counters = Reactor.gameObject.AddComponent<Counters>();
			}
		}

		void OnDisable()
		{
			Unregister();
		}

		public override BaseRuleElement.RuleData GetRuleInformation()
		{
			RuleData rule = base.GetRuleInformation();

			if (rule.parameters == null)
				rule.parameters = new System.Collections.Generic.List<Param>();

			rule.parameters.Add(new Param()
				{
					name = "ChangeBy",
					type = ChangeBy.GetType(),
					value = ChangeBy
				});

			rule.parameters.Add(new Param()
				{
					name = "CounterName",
					type = CounterName.GetType(),
					value = CounterName
				});

			return rule;
		}

		protected override void React(GameEventData eventData)
		{
			counters.ChangeCounter(CounterName, ChangeBy);
		}
	}
}
