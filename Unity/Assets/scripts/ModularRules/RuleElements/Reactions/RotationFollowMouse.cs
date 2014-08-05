using UnityEngine;
using System.Collections;

/// <summary>
/// The rotation follows a relative movement value
/// </summary>
public class RotationFollowMouse : Reaction
{
	public float Sensitivity = 10;
	public float MaxYRotation = 60;

	float yRotation;

	ActorDropDown actorDropDown;

	RuleGenerator generator;

	void OnEnable()
	{
		Register();
	}

	void OnDisable()
	{
		Unregister();
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		this.generator = generator;

		string[] actors = generator.Gui.ActorNames;
		actorDropDown = new ActorDropDown(
			System.Array.FindIndex(actors, item => item == Reactor.Label),
			actors,
			ref generator.Gui.OnAddedActor, ref generator.Gui.OnRenamedActor, ref generator.Gui.OnDeletedActor);
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("Rotate", RuleGUI.ruleLabelStyle);

		int resultIndex = actorDropDown.Draw();
		if (resultIndex > -1)
		{
			int resultId = generator.ActorData.Find(item => item.label == actorDropDown.Content[resultIndex].text).id;
			generator.ChangeActor(this, resultId);
			(ruleData as ReactionData).actorId = resultId;
		}

		GUILayout.Label("with mouse. Use a sensitivity of", RuleGUI.ruleLabelStyle);

		Sensitivity = RuleGUI.ShowParameter(Sensitivity);
		ChangeParameter("Sensitivity", (ruleData as ReactionData).parameters, Sensitivity);

		GUILayout.Label("and a maximum up/down rotation of", RuleGUI.ruleLabelStyle);

		MaxYRotation = RuleGUI.ShowParameter(MaxYRotation);
		ChangeParameter("MaxYRotation", (ruleData as ReactionData).parameters, MaxYRotation);
	}

	protected override void React(GameEventData data)
	{
		DataPiece inputData;
		if ((inputData = data.Get(EventDataKeys.InputData)) != null &&
			inputData.dataType == typeof(MouseInput.MouseData))
		{
			Vector2 deltaMovement = ((MouseInput.MouseData)inputData.data).axisValues;

			//((IRotate)Reactor).Rotate(data, deltaMovement);

			float xRotation = Reactor.transform.localEulerAngles.y + deltaMovement.x * Sensitivity;
			yRotation += deltaMovement.y * Sensitivity;

			yRotation = Mathf.Clamp(yRotation, -MaxYRotation, MaxYRotation);

			Reactor.transform.localEulerAngles = new Vector3(-yRotation, xRotation, 0);
		}
	}
}

