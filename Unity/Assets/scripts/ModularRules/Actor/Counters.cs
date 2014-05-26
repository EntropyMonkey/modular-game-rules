using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Counters : MonoBehaviour
{
	public class Counter
	{
		public float value;
		public float minValue;
		public float maxValue;
		public string name;
		public bool showInGui;
	};

	private Dictionary<string, Counter> counters = new Dictionary<string,Counter>();

	public void AddCounter(string name, float startValue, float minValue, float maxValue, bool showInGui)
	{
		if (!counters.ContainsKey(name))
		{
			counters.Add(name, new Counter()
			{
				name = name,
				value = startValue,
				minValue = minValue,
				maxValue = maxValue,
				showInGui = showInGui
			});
		}
		else
		{
			if (counters[name].showInGui)
				IngameGUI.Instance().RemoveCounter(counters[name]);

			counters[name] = new Counter()
			{
				name = name,
				value = startValue,
				minValue = minValue,
				maxValue = maxValue,
				showInGui = showInGui
			};
		}

		if (counters[name].showInGui)
		{
			IngameGUI.Instance().AddCounter(counters[name]);
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
		counter.value = Mathf.Max(counter.minValue, Mathf.Min(counter.maxValue, value));
	}

	void OnGUI()
	{
		int y = 0;
		foreach (KeyValuePair<string, Counter> kvp in counters)
		{
			GUI.Label(new Rect(0, y, 50, 50), kvp.Value.value.ToString());
			y += 50;
		}
	}
}
