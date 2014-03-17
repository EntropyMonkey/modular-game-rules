using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public class MouseInput : InputReceived
	{
		public enum MouseButton { LEFT = 0, RIGHT = 1, MIDDLE = 2, NONE = 3}

		public class MouseData : InputData
		{
			public MouseButton button = MouseButton.NONE;
			public GameObject clickedObject = null;
			public Vector3 screenPosition; // pixels
			public Vector3 deltaMovement; // pixels

			public static MouseData Empty
			{
				get
				{
					return new MouseData
					{
						inputType = InputType.CONTINUOUS,
						button = MouseButton.NONE,
						clickedObject = null,
						inputValue = 0.0f,
						screenPosition = Vector3.zero,
						deltaMovement = Vector3.zero
					};
				}
			}

			public override string ToString()
			{
				return base.ToString() + " Button: " + button + " clickedObject: " + clickedObject + " screenPos: " + screenPosition;
			}
		};

		public MouseButton TrackedButton;

		protected Vector3 lastScreenPosition = Vector3.zero;

		public override GameEvent UpdateEvent()
		{
			if (!base.UpdateEvent()) return null;

			MouseData data = MouseData.Empty;
			data.button = TrackedButton;

			if (lastScreenPosition != Input.mousePosition)
			{
				data.screenPosition = Input.mousePosition;
				data.deltaMovement = Input.mousePosition - lastScreenPosition;
				lastScreenPosition = data.screenPosition;
				Debug.Log(data.deltaMovement);
			}

			if (Input.GetMouseButtonDown((int)TrackedButton))
			{
				data.inputType = InputType.PRESSED;
				data.inputValue = 1.0f;

				RaycastMousePosition(data);
			}
			else if (Input.GetMouseButtonUp((int)TrackedButton))
			{
				data.inputType = InputType.RELEASED;
			}
			else if (Input.GetMouseButton((int)TrackedButton))
			{
				data.inputValue = 1.0f;
				data.inputType = InputType.HELD;
			}

			if (data.clickedObject != null || data.screenPosition == lastScreenPosition || (int)data.inputType > 0)
			{
				Trigger(EventData.Empty.Add(new DataPiece(EventDataKeys.InputData) { data = data }));
			}

			return this;
		}

		private void RaycastMousePosition(MouseData data)
		{
			RaycastHit hitInfo;
			Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

			if (hitInfo.collider != null)
				data.clickedObject = hitInfo.collider.gameObject;
			else
				data.clickedObject = null;
		}
	}
}
