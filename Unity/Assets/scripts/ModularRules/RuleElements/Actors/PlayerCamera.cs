using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCamera : Actor
{
	public static string Tag = "PlayerCamera";

	public float ViewportWidth = 0.5f;
	public float ViewportXPos;

	private string viewportXPosString = "";
	private string viewportWidthString = "";

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

		viewportXPosString = ViewportXPos.ToString();
		viewportWidthString = ViewportWidth.ToString();
	}

	public override RuleData GetRuleInformation()
	{
		RuleData rule = base.GetRuleInformation();

		(rule as ActorData).prefab = "PlayerCamera";

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
		base.ShowGui(ruleData);

		RuleGUI.VerticalLine();

		// column 3
		GUILayout.BeginVertical();


		// parameters
		GUILayout.BeginHorizontal();
		GUILayout.Label("Viewport Width (0..1)", RuleGUI.ruleLabelStyle);
		viewportWidthString = RuleGUI.ShowParameter(viewportWidthString);
		if (GUILayout.Button("Set", RuleGUI.buttonStyle))
		{
			ViewportWidth = float.Parse(viewportWidthString);
			ChangeParameter("ViewportWidth", ruleData.parameters, ViewportWidth);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Viewport X Position (0..1)", RuleGUI.ruleLabelStyle);
		viewportXPosString = RuleGUI.ShowParameter(viewportXPosString);
		if (GUILayout.Button("Set", RuleGUI.buttonStyle))
		{
			ViewportXPos = float.Parse(viewportXPosString);
			ChangeParameter("ViewportXPos", ruleData.parameters, ViewportXPos);
		}
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();
	}
}
