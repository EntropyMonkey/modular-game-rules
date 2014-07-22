using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Counters : Actor
{
	public class Counter
	{
		public int value;

		public int maxValue;
		public int minValue;

		public Counter()
		{
			value = 0;
			maxValue = 100;
			minValue = 0;
		}

		public Counter(int _value, int _maxValue, int _minValue)
		{
			value = _value;
			maxValue = _maxValue;
			minValue = _minValue;
		}
	}

	private Dictionary<string, Counter> counters;

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		if (counters == null)
			counters = new Dictionary<string, Counter>();
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("A counters actor");
	}

	public void ChangeCounter(string counterName, int value)
	{
		if (counters.ContainsKey(counterName))
		{
			counters[counterName].value += value;
		}
	}

	public int GetValue(string counterName)
	{
		if (counters.ContainsKey(counterName))
			return counters[counterName].value;

		return 0;
	}
}
