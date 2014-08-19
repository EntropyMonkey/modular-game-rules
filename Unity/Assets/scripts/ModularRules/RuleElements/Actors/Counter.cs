using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Counter : Actor
{
	public float value = 0;

	public int StartValue = 0;
	public int MaxValue = 100;
	public int MinValue = 0;
	public bool ShowInGame = false;

	// TODO move to a manager
	private static List<Counter> counters = new List<Counter>();

	public static Counter Get(string name)
	{
		return counters.Find(item => item.Label == name);
	}

	public static void ResetCounters()
	{
		counters.Clear();
	}

	public static void ShowCountersInGame()
	{
		foreach (Counter c in counters)
		{
			if (c.ShowInGame)
			{
				c.ShowInGameGui();
			}
		}
	}

	public float Value
	{
		get
		{
			return value;
		}
	}

	public override RuleData GetRuleInformation()
	{
		ActorData rule = base.GetRuleInformation() as ActorData;
		if (rule.parameters == null)
			rule.parameters = new List<Param>();

		rule.parameters.Add(new Param()
			{
				name = "StartValue",
				type = StartValue.GetType(),
				value = StartValue
			});

		rule.parameters.Add(new Param()
			{
				name = "MaxValue",
				type = MaxValue.GetType(),
				value = MaxValue
			});

		rule.parameters.Add(new Param()
			{
				name = "MinValue",
				type = MinValue.GetType(),
				value = MinValue
			});

		rule.parameters.Add(new Param()
			{
				name = "ShowInGame",
				type = ShowInGame.GetType(),
				value = ShowInGame
			});

		return rule;
	}

	public override void ShowGui(RuleData data)
	{
		base.ShowGui(data);

		RuleGUI.VerticalLine();

		// column 2
		GUILayout.BeginVertical();

		// parameters
		GUILayout.BeginHorizontal();
		GUILayout.Label("Start Value", RuleGUI.smallLabelStyle);
		StartValue = RuleGUI.ShowParameter(StartValue);
		ChangeParameter("StartValue", data.parameters, StartValue);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Minimum", RuleGUI.smallLabelStyle);
		MinValue = RuleGUI.ShowParameter(MinValue);
		ChangeParameter("MinValue", data.parameters, MinValue);
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();

		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Maximum", RuleGUI.smallLabelStyle);
		MaxValue = RuleGUI.ShowParameter(MaxValue);
		ChangeParameter("MaxValue", data.parameters, MaxValue);
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();

		RuleGUI.VerticalLine();

		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Show in game?", RuleGUI.ruleLabelStyle);
		ShowInGame = RuleGUI.ShowParameter(ShowInGame);
		ChangeParameter("ShowInGame", data.parameters, ShowInGame);
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();
	}

	public virtual void ShowInGameGui()
	{
		GUILayout.BeginVertical();
		GUILayout.Space(5);

		GUILayout.BeginHorizontal(RuleGUI.areaBackgroundStyle);
		GUILayout.Space(5);
		GUILayout.Label(Label + ": " + Mathf.Floor(Value) + " / " + MaxValue, RuleGUI.smallLabelStyle);
		GUILayout.Space(5);
		GUILayout.EndHorizontal();

		GUILayout.Space(5);
		GUILayout.EndVertical();
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		value = StartValue;

		generator.Gui.OnDeletedActor += OnDeletedActor;

		if (!counters.Find(item => item.name == Label))
			counters.Add(this);
		else
			Debug.LogError("Two counters have the same name. Cannot add more than one counter with the name " + Label);
	}

	private void OnDeletedActor(string[] newContent, ActorData actor)
	{
		int deleted = counters.FindIndex(item => item.Id == actor.id);
		if (deleted >= 0)
		{
			counters.RemoveAt(deleted);
		}
	}

	public void ChangeBy(float value)
	{
		this.value += value;
		if (this.value > MaxValue) this.value = MaxValue;
		if (this.value < MinValue) this.value = MinValue;
	}
}
