using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MouseInput : InputReceived
{
	public enum MouseButton { LEFT = 0, RIGHT = 1, MIDDLE = 2, NONE = 3 }

	public class MouseData : InputData
	{
		public MouseButton button = MouseButton.NONE;
		public Vector3 screenPosition; // pixels
		public Ray rayFromPosition;
		public Vector2 axisValues;

		public static MouseData Empty
		{
			get
			{
				return new MouseData
				{
					inputType = InputType.NONE,
					button = MouseButton.NONE,
					inputValue = 0.0f,
					screenPosition = Vector3.zero,
					axisValues = Vector2.zero
				};
			}
		}
	};

	// fields
	public MouseButton TrackedButton;

	public Actor TrackedCamera;

	protected Vector3 lastScreenPosition = Vector3.zero;

	public override RuleData GetRuleInformation()
	{
		EventData rule = base.GetRuleInformation() as EventData;

		rule.parameters = new List<Param>();
		rule.parameters.Add(new Param()
		{
			name = "TrackedButton",
			type = TrackedButton.GetType(),
			value = TrackedButton
		});
		rule.parameters.Add(new Param() 
		{
 			name = "TrackedCamera",
			type = TrackedCamera.GetType(),
			value = TrackedCamera.Id
		});

		// gui stuff
		rule.guiPrefix = "On";
		rule.guiName = "Mouse Input";

		rule.guiParams.Add(new Param()
		{
			name = "TrackedButton",
			type = TrackedButton.GetType(),
			value = TrackedButton
		});

		return rule;
	}

	// methods
	public override GameEvent UpdateEvent()
	{
		if (!base.UpdateEvent() || TrackedCamera == null) return null;

		if (TrackedCamera.camera == null) 
		{
			Debug.LogError("There is no camera component on the TrackedCamera actor.");
			return null;
		}
			
		MouseData data = MouseData.Empty;
		data.button = TrackedButton;
		data.screenPosition = Input.mousePosition;
		data.rayFromPosition = TrackedCamera.camera.ScreenPointToRay(data.screenPosition);

		// has the mouse been moved?
		if (lastScreenPosition != Input.mousePosition)
		{
			data.inputType = InputType.CONTINUOUS;
			data.axisValues.x = Input.GetAxis("Mouse X");
			data.axisValues.y = Input.GetAxis("Mouse Y");
			lastScreenPosition = data.screenPosition;
		}

		// was the tracked mouse button clicked?
		if (Input.GetMouseButtonDown((int)TrackedButton))
		{
			data.inputType = InputType.PRESSED;
			data.inputValue = 1.0f;
		}
		// was the tracked mouse button released?
		else if (Input.GetMouseButtonUp((int)TrackedButton))
		{
			data.inputType = InputType.RELEASED;
			data.inputValue = -1;
		}
		// any action with the tracked button?
		else if (Input.GetMouseButton((int)TrackedButton))
		{
			data.inputValue = 1.0f;
			data.inputType = InputType.HELD;
		}

		// trigger only if something happened
		if ((int)data.inputType > 0)
		{
			Trigger(GameEventData.Empty.Add(new DataPiece(EventDataKeys.InputData) { data = data }));
		}

		return this;
	}
}
