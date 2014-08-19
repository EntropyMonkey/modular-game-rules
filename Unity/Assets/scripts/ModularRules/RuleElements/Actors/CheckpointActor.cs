using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckpointActor : Actor
{
	private enum State { RESPAWNING, NORMAL }

	public string Tag;
	public bool UseGravity;

	private State currentState = State.RESPAWNING;
	private Checkpoint[] checkpoints;
	private int currentCheckpoint = -1;

	public override BaseRuleElement.RuleData GetRuleInformation()
	{
		BaseRuleElement.ActorData rule = base.GetRuleInformation() as BaseRuleElement.ActorData;

		if (rule.parameters == null)
			rule.parameters = new List<Param>();

		rule.parameters.Add(new Param()
			{
				name = "Tag",
				type = Tag.GetType(),
				value = Tag
			});

		return rule;
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		if (!rigidbody)
			gameObject.AddComponent<Rigidbody>();

		rigidbody.useGravity = UseGravity;
		rigidbody.freezeRotation = true;

		generator.OnGeneratedLevel += delegate(List<BaseRuleElement.ActorData> actorData,
			List<BaseRuleElement.EventData> eventData,
			List<BaseRuleElement.ReactionData> reactionData,
			string filename)
		{
			currentState = State.RESPAWNING;
			checkpoints = null;
		};
	}

	void Start()
	{
		tag = Tag;
	}

	public override void ResetGenerationData()
	{
		base.ResetGenerationData();

		currentCheckpoint = -1;
		currentState = State.RESPAWNING;
	}

	public override void Respawn()
	{
		GameObject[] allCheckpoints = GameObject.FindGameObjectsWithTag(Checkpoint.Tag);
		// find starting checkpoint

		List<Checkpoint> points = new List<Checkpoint>();
		for (int i = 0; i < allCheckpoints.Length; i++)
		{
			Checkpoint p = allCheckpoints[i].GetComponent<Checkpoint>();
			if (p.TargetTag == Tag)
			{
				points.Add(p);
			}
		}

		checkpoints = points.ToArray();

		if (currentCheckpoint == -1)
			currentCheckpoint = Random.Range(0, checkpoints.Length - 1);

		transform.position = checkpoints[currentCheckpoint].transform.position;
		currentState = State.NORMAL;
	}

	void Update()
	{
		switch (currentState)
		{
			case State.RESPAWNING:
				Respawn();
				break;
			case State.NORMAL:
				UpdateNormalState();
				break;
		}
	}

	void UpdateNormalState()
	{
		UpdateEvents();

		if (checkpoints != null)
		{
			for (int i = 0; i < checkpoints.Length; i++)
			{
				if (checkpoints[i] != null && checkpoints[i].IsActiveAt(transform.position))
				{
					currentCheckpoint = i;
					break;
				}
			}
		}
	}

	public override void ShowGui(RuleData ruleData)
	{
		base.ShowGui(ruleData);

		RuleGUI.VerticalLine();

		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Checkpoint Tag", RuleGUI.smallLabelStyle);
		Tag = RuleGUI.ShowParameter(Tag);
		ChangeParameter("Tag", ruleData.parameters, Tag);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Use Gravity", RuleGUI.smallLabelStyle);
		UseGravity = RuleGUI.ShowParameter(UseGravity);
		ChangeParameter("UseGravity", ruleData.parameters, UseGravity);
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
	}
}
