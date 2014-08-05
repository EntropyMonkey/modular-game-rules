using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckpointActor : Actor
{
	public string Tag;
	public bool UseGravity;

	private List<Checkpoint> checkpoints = new List<Checkpoint>();
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
	}

	void Start()
	{
		tag = Tag;

		Respawn(true);
	}

	public override void Reset()
	{
		base.Reset();

		currentCheckpoint = -1;
		Respawn(true);
	}

	void Respawn(bool atStart = false)
	{
		GameObject[] checkpoints = GameObject.FindGameObjectsWithTag(Checkpoint.Tag);
		// find starting checkpoint

		for (int i = 0; i < checkpoints.Length; i++)
		{
			Checkpoint p = checkpoints[i].GetComponent<Checkpoint>();
			if (p.TargetTag == Tag)
			{
				this.checkpoints.Add(p);
				if (((atStart || currentCheckpoint == -1) && p.Start) || p.Id == currentCheckpoint)
					transform.position = p.transform.position;
			}
		}
	}

	void Update()
	{
		UpdateEvents();

		for (int i = 0; i < checkpoints.Count; i++)
		{
			if (checkpoints[i] != null && checkpoints[i].IsActiveAt(transform.position))
			{
				currentCheckpoint = checkpoints[i].Id;
				break;
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
