using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public class Counter
	{
		public float value;
		public float minValue;
		public float maxValue;
		public string name;
	};

	public class Counters : MonoBehaviour
	{
		private Dictionary<string, Counter> counters;

		void Start()
		{
			counters = new Dictionary<string, Counter>();
		}

		public void AddCounter(string name, float startValue, float minValue = 0, float maxValue = 1)
		{
			if (!counters.ContainsKey(name))
			{
				counters.Add(name, new Counter()
				{
					name = name,
					value = startValue,
					minValue = minValue,
					maxValue = maxValue
				});
			}
			else
			{
				counters[name] = new Counter()
				{
					name = name,
					value = startValue,
					minValue = minValue,
					maxValue = maxValue
				};
			}
		}

		public void RemoveCounter(string name)
		{
			if (counters.ContainsKey(name))
				counters.Remove(name);
		}

		public void SetCounter(string name, float newValue)
		{
			if (counters.ContainsKey(name))
			{
				SetValue(counters[name], newValue);
			}
		}

		public void ChangeCounter(string name, float changeBy)
		{
			if (counters.ContainsKey(name))
			{
				SetValue(counters[name], counters[name].value + changeBy);
			}
		}

		public float GetValue(string name)
		{
			if (counters.ContainsKey(name))
			{
				return counters[name].value;
			}

			return 0.0f;
		}

		void SetValue(Counter counter, float value)
		{
			counter.value = Mathf.Min(counter.minValue, Mathf.Max(counter.maxValue, value));
		}
	}
}
