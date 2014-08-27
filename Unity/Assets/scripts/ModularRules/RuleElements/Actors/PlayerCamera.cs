using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCamera : Actor
{
	public static string Tag = "PlayerCamera";

	public bool AutoCalculateViewport = true;

	public float ViewportWidth = 1f;
	public float ViewportHeight = 1f;
	public float ViewportXPos;
	public float ViewportYPos;

	private string viewportXPosString = "";
	private string viewportYPosString = "";
	private string viewportWidthString = "";
	private string viewportHeightString = "";

	private struct Viewport
	{
		public PlayerCamera cam; // the camera's actor id
		public Vector2 pos;
		public Vector2 size;

		public static bool operator==(Viewport a, Viewport b)
		{
			if (a.cam == null || b.cam == null || a.cam.Id != b.cam.Id)
				return false;
			else
				return true;
		}

		public static bool operator !=(Viewport a, Viewport b)
		{
			return !(a == b);
		}
	}
	private static List<Viewport> viewportSizes = new List<Viewport>();

	void Start()
	{
		tag = Tag;
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		Viewport viewport;

		if (AutoCalculateViewport)
		{
			CalculateViewportSizes();
			viewport = viewportSizes.Find(item => item.cam.Id == Id);
		}
		else
		{
			viewport = new Viewport()
			{
				cam = this,
				pos = new Vector2(ViewportXPos, ViewportYPos),
				size = new Vector2(ViewportWidth, ViewportHeight)
			};
		}

		ResetViewport(viewport);

		ShowPrefabInGUI = false;
	}

	void ResetViewport(Viewport viewport)
	{
		ViewportWidth = viewport.size.x;
		ViewportHeight = viewport.size.y;
		ViewportXPos = viewport.pos.x;
		ViewportYPos = viewport.pos.y;

		if (!camera)
			gameObject.AddComponent<Camera>();

		if (camera)
		{
			Rect camRect = camera.rect;
			camRect.width = ViewportWidth;
			camRect.height = ViewportHeight;
			camRect.x = ViewportXPos;
			camRect.y = ViewportYPos;
			camera.rect = camRect;
		}

		viewportXPosString = ViewportXPos.ToString();
		viewportYPosString = ViewportYPos.ToString();
		viewportWidthString = ViewportWidth.ToString();
		viewportHeightString = ViewportHeight.ToString();
	}

	public static void SetViewportsToRecalculateOnLoad()
	{
		viewportSizes.Clear();
	}

	void CalculateViewportSizes()
	{
		// return if this camera was already calculated
		if (viewportSizes == null || viewportSizes.FindIndex(item => item.cam.Id == Id) != -1)
			return;

		int numCams = viewportSizes.Count + 1;

		bool unevenNumCams = numCams % 2 == 1;
		float camHeight = 1.0f / (unevenNumCams ? (numCams + 1) * 0.5f : numCams * 0.5f);

		Vector2 size = new Vector2(0.5f, camHeight);
		Vector2 pos = Vector2.zero;

		int i = 0;
		for (int y = 0; y < numCams / 2; y++)
		{
			for (int x = 0; x < 2; x++)
			{
				// add this cam
				if (!unevenNumCams && i == viewportSizes.Count)
				{
					viewportSizes.Add(new Viewport()
					{
						cam = this,
						pos = pos,
						size = size
					});
				}
				else
				{
					// recalculate already added cams
					viewportSizes[i] = new Viewport()
					{
						cam = viewportSizes[i].cam,
						pos = pos,
						size = size
					};
					viewportSizes[i].cam.ResetViewport(viewportSizes[i]);
				}

				i++;
				pos.x += size.x;
			}

			pos.x = 0;
			pos.y += camHeight;
		}

		if (unevenNumCams)
		{
			viewportSizes.Add(new Viewport()
			{
				cam = this,
				pos = pos,
				size = new Vector2(1, camHeight)
			});
		}
	}

	public override RuleData GetRuleInformation()
	{
		RuleData rule = base.GetRuleInformation();

		(rule as ActorData).prefab = "PlayerCamera";

		if (rule.parameters == null) rule.parameters = new System.Collections.Generic.List<Param>();

		rule.parameters.Add(new Param()
			{
				name = "AutoCalculateViewport",
				type = AutoCalculateViewport.GetType(),
				value = AutoCalculateViewport
			});

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
		GUILayout.Label("Automatically calculate viewport", RuleGUI.ruleLabelStyle);
		AutoCalculateViewport = RuleGUI.ShowParameter(AutoCalculateViewport);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		if (!AutoCalculateViewport)
		{
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
		}

		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();
	}
}
