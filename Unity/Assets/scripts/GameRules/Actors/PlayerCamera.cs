using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCamera : Actor
{
	public static string Tag = "PlayerCamera";

	public float ViewportWidth = 0.5f;
	public float ViewportXPos;

	void Start()
	{
		tag = Tag;
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		if (!camera)
			gameObject.AddComponent<Camera>();

		if (camera)
		{
			Rect camRect = camera.rect;
			camRect.width = ViewportWidth;
			camRect.x = ViewportXPos;
			camera.rect = camRect;
		}
	}

	public override RuleData GetRuleInformation()
	{
		RuleData rule = base.GetRuleInformation();

		(rule as ActorData).prefabName = "PlayerCamera";

		if (rule.parameters == null) rule.parameters = new System.Collections.Generic.List<Param>();

		rule.parameters.Add(new Param()
			{
				name = "ViewportWidth",
				type = ViewportWidth.GetType(),
				value = ViewportWidth
			});

		rule.parameters.Add(new Param()
			{
				name = "ViewportXPos",
				type = ViewportXPos.GetType(),
				value = ViewportXPos
			});

		return rule;
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("Showing off a cam", RuleGUI.ruleLabelStyle);
	}
}
