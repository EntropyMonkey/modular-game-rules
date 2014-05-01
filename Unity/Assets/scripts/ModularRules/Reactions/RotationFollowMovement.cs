using UnityEngine;
using System.Collections;

namespace ModularRules
{
	/// <summary>
	/// The rotation follows a relative movement value
	/// </summary>
	public class RotationFollowMouse : Reaction
	{
		public float Sensitivity = 10;
		public float MaxYRotation = 60;

		float yRotation;

		void OnEnable()
		{
			Register();
		}

		void OnDisable()
		{
			Unregister();
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
}
