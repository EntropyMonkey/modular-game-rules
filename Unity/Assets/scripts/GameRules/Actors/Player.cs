using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public class Player : Actor
{
	public string Tag = "Player";

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
