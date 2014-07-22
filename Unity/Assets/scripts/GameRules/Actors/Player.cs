using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public class Player : Actor
{
	public static string Tag = "Player";

	public float Gravity = 3;
	public float HorizontalMaxSpeed = 10;
	public float VerticalMaxSpeed = 10;

	void Start()
	{
		tag = Tag;
	}

	public override BaseRuleElement.RuleData GetRuleInformation()
	{
		RuleData rule = base.GetRuleInformation();

		(rule as ActorData).prefabName = "Player";

		if (rule.parameters == null)
			rule.parameters = new List<Param>();

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
		GUILayout.Label("Shwoing off a player here", RuleGUI.ruleLabelStyle);
	}

	// Update is called once per frame
	void Update()
	{
		UpdateEvents();

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
	}
}
