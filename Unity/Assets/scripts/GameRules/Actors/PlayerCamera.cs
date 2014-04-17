using UnityEngine;
using System.Collections;
using ModularRules;

public class PlayerCamera : Actor, IRotate
{
	//public enum CameraType { EGO, TOP_DOWN }
	//public CameraType CamType = CameraType.EGO;

	public static string Tag = "PlayerCamera";


	public float Sensitivity = 10;
	public float MaxYRotation = 60;

	float yRotation;

	void Start()
	{
		tag = Tag;

		Initialize();
	}

	public void Rotate(GameEventData data, Vector3 deltaMovement)
	{
		EgoCamBehaviour(data, deltaMovement);
	}

	void EgoCamBehaviour(GameEventData data, Vector3 deltaMovement)
	{
		float xRotation = transform.localEulerAngles.y + deltaMovement.x * Sensitivity;
		yRotation += deltaMovement.y * Sensitivity;

		yRotation = Mathf.Clamp(yRotation, -MaxYRotation, MaxYRotation);

		transform.localEulerAngles = new Vector3(-yRotation, xRotation, 0);
	}
}
