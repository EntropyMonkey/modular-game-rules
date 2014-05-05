using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class CounterEvent : GameEvent
	{
		public string CounterName;

		public Comparison Compare;

		public float CounterLimit;

		private Counters counters;

		public override void Initialize()
		{
			base.Initialize();

			if (!(counters = Actor.GetComponent<Counters>()))
			{
				Debug.LogWarning("Deactivating " + name + " because the actor doesn't have counters.");
				gameObject.SetActive(false);
			}
		}

		public override BaseRuleElement.RuleData GetRuleInformation()
		{
			RuleData rule =  base.GetRuleInformation();

			if (rule.parameters == null) rule.parameters = new System.Collections.Generic.List<Param>();

			rule.parameters.Add(new Param()
				{
					name = "CounterName",
					type = CounterName.GetType(),
					value = CounterName
				});

			rule.parameters.Add(new Param()
				{
					name = "Compare",
					type = Compare.GetType(),
					value = Compare
				});

			rule.parameters.Add(new Param()
				{
					name = "CounterLimit",
					type = CounterLimit.GetType(),
					value = CounterLimit
				});

			return rule;
		}

		public override GameEvent UpdateEvent()
		{
			if ((Compare == Comparison.LESS && counters.GetValue(CounterName) < CounterLimit) ||
				(Compare == Comparison.EQUAL && counters.GetValue(CounterName) - CounterLimit < 0.01f) ||
				(Compare == Comparison.GREATER && counters.GetValue(CounterName) > CounterLimit))
			{
				Trigger(GameEventData.Empty);
				return this;
			}

			return null;
		}
	}
}
