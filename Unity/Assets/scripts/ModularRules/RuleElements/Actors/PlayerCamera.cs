using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCamera : Actor
{
	public static string Tag = "PlayerCamera";

	public float ViewportWidth = 1f;
	public float ViewportHeight = 1f;
	public float ViewportXPos;
	public float ViewportYPos;

	private string viewportXPosString = "";
	private string viewportYPosString = "";
	private string viewportWidthString = "";
	private string viewportHeightString = "";

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
		viewportYPosString = ViewportYPos.ToString();
		viewportWidthString = ViewportWidth.ToString();
		viewportHeightString = ViewportHeight.ToString();
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
			name = "ViewportHeight",
			type = ViewportHeight.GetType(),
			value = ViewportHeight
		});

		rule.parameters.Add(new Param()
			{
				name = "ViewportXPos",
				type = ViewportXPos.GetType(),
				value = ViewportXPos
			});

		rule.parameters.Add(new Param()
		{
			name = "ViewportYPos",
			type = ViewportYPos.GetType(),
			value = ViewportYPos
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
		GUILayout.Label("Viewport Height (0..1)", RuleGUI.ruleLabelStyle);
		viewportHeightString = RuleGUI.ShowParameter(viewportHeightString);
		if (GUILayout.Button("Set", RuleGUI.buttonStyle))
		{
			ViewportHeight = float.Parse(viewportHeightString);
			ChangeParameter("ViewportHeight", ruleData.parameters, ViewportHeight);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Horizontal Viewport Position (0..1)", RuleGUI.ruleLabelStyle);
		viewportXPosString = RuleGUI.ShowParameter(viewportXPosString);
		if (GUILayout.Button("Set", RuleGUI.buttonStyle))
		{
			ViewportXPos = float.Parse(viewportXPosString);
			ChangeParameter("ViewportXPos", ruleData.parameters, ViewportXPos);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Vertical Viewport Position (0..1)", RuleGUI.ruleLabelStyle);
		viewportYPosString = RuleGUI.ShowParameter(viewportYPosString);
		if (GUILayout.Button("Set", RuleGUI.buttonStyle))
		{
			ViewportYPos = float.Parse(viewportYPosString);
			ChangeParameter("ViewportYPos", ruleData.parameters, ViewportYPos);
		}
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();
	}
}
