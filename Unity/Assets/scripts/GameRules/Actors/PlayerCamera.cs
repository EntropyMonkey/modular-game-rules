using UnityEngine;
using System.Collections;
using ModularRules;

public class PlayerCamera : Actor, IRotate
{
	public enum CameraType { EGO, TOP_DOWN }

	public static string Tag = "PlayerCamera";

	public CameraType CamType = CameraType.EGO;

	public float Sensitivity = 10;
	public float MaxYRotation = 60;

	float yRotation;

	void Start()
	{
		tag = Tag;

		InitializeActor();
	}

	public void Rotate(EventData data, Vector3 deltaMovement)
	{
		EgoCamBehaviour(data, deltaMovement);
	}

	void EgoCamBehaviour(EventData data, Vector3 deltaMovement)
	{
		float xRotation = transform.localEulerAngles.y + deltaMovement.x * Sensitivity;
		yRotation += deltaMovement.y * Sensitivity;

		yRotation = Mathf.Clamp(yRotation, -MaxYRotation, MaxYRotation);

		transform.localEulerAngles = new Vector3(-yRotation, xRotation, 0);
	}
}
