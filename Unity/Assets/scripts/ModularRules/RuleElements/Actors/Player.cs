using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public class Player : Actor
{
	private enum PlayerState { RESPAWNING, NORMAL }

	public static string Tag = "Player";

	public float Gravity = 3;
	public float HorizontalMaxSpeed = 10;
	public float VerticalMaxSpeed = 10;

	private PlayerState currentState;

	private List<Checkpoint> checkpoints = new List<Checkpoint>();
	private int currentCheckpoint = -1;

	void Start()
	{
		tag = Tag;
		currentState = PlayerState.RESPAWNING;
	}

	public override void Reset()
	{
		base.Reset();

		currentState = PlayerState.RESPAWNING;
		currentCheckpoint = -1;
	}

	public override BaseRuleElement.RuleData GetRuleInformation()
	{
		RuleData rule = base.GetRuleInformation();

		(rule as ActorData).prefab = "Player";
		(rule as ActorData).label = name;

		if (rule.parameters == null)
			rule.parameters = new List<Param>();

		rule.parameters.Add(new Param()
			{
				name = "Gravity",
				type = Gravity.GetType(),
				value = Gravity
			});
		rule.parameters.Add(new Param()
		{
			name = "HorizontalMaxSpeed",
			type = HorizontalMaxSpeed.GetType(),
			value = HorizontalMaxSpeed
		});
		rule.parameters.Add(new Param()
		{
			name = "VerticalMaxSpeed",
			type = VerticalMaxSpeed.GetType(),
			value = VerticalMaxSpeed
		});

		return rule;
	}

	public override void ShowGui(RuleData ruleData)
	{

		base.ShowGui(ruleData);

		RuleGUI.VerticalLine();

		// column 3
		GUILayout.BeginVertical();
		
		// parameters
		GUILayout.BeginHorizontal();
		GUILayout.Label("Horizontal Max Speed", RuleGUI.ruleLabelStyle);
		HorizontalMaxSpeed = RuleGUI.ShowParameter(HorizontalMaxSpeed);
		ChangeParameter("HorizontalMaxSpeed", ruleData.parameters, HorizontalMaxSpeed);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Vertical Max Speed", RuleGUI.ruleLabelStyle);
		VerticalMaxSpeed = RuleGUI.ShowParameter(VerticalMaxSpeed);
		ChangeParameter("VerticalMaxSpeed", ruleData.parameters, VerticalMaxSpeed);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Gravity", RuleGUI.ruleLabelStyle);
		Gravity = RuleGUI.ShowParameter(Gravity);
		ChangeParameter("Gravity", ruleData.parameters, Gravity);
		GUILayout.EndHorizontal();
		
		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();
	}

	// Update is called once per frame
	void Update()
	{
		UpdateEvents();

		switch (currentState)
		{
			case PlayerState.RESPAWNING:
				Respawn();
				break;
			case PlayerState.NORMAL:
				UpdateNormalState();
				break;
		}
	}

	void Respawn(bool atStart = false)
	{
		GameObject[] checkpoints = GameObject.FindGameObjectsWithTag(Checkpoint.Tag);
		this.checkpoints.Clear();
		// find starting checkpoint

		for (int i = 0; i < checkpoints.Length; i++)
		{
			Checkpoint p = checkpoints[i].GetComponent<Checkpoint>();
			if (p != null && p.TargetTag == Tag)
			{
				this.checkpoints.Add(p);
				if (((atStart || currentCheckpoint == -1) && p.Start) || p.Id == currentCheckpoint)
					transform.position = p.transform.position;
			}
		}

		currentState = PlayerState.NORMAL;
	}

	void UpdateNormalState()
	{
		// limit speed
		if (rigidbody)
		{
			Vector3 upVelocity = Vector3.Project(rigidbody.velocity, Vector3.up);
			if (upVelocity.magnitude > VerticalMaxSpeed)
			{
				upVelocity = upVelocity.normalized * VerticalMaxSpeed;
			}

			Vector3 horizontalVelocity = Vector3.Project(rigidbody.velocity, Vector3.forward) + Vector3.Project(rigidbody.velocity, Vector3.right);
			if (horizontalVelocity.magnitude > HorizontalMaxSpeed)
			{
				horizontalVelocity = horizontalVelocity.normalized * HorizontalMaxSpeed;
			}

			rigidbody.velocity = horizontalVelocity + upVelocity;

			rigidbody.AddForce(Vector3.down * Gravity);
		}

		for (int i = 0; i < checkpoints.Count; i++)
		{
			if (checkpoints[i] != null && checkpoints[i].IsActiveAt(transform.position))
			{
				currentCheckpoint = checkpoints[i].Id;
				break;
			}
		}
	}
}
