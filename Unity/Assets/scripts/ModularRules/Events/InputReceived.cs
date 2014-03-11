using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class InputReceived : GameEvent
	{
		public enum InputType { HELD, PRESSED, RELEASED, AXIS }

		[SerializeField]
		private KeyCode InputKey = KeyCode.None;

		[SerializeField]
		private string AxisName = "";

		public override void Update()
		{
			if (InputKey != KeyCode.None && Input.GetKey(InputKey))
			{
				Trigger(new EventData()
					.Add(new DataPiece(EventDataKeys.InputKey) { data = InputKey }) // add which key has been pressed
					.Add(new DataPiece(EventDataKeys.InputValue) { data = 1.0f })
					.Add(new DataPiece(EventDataKeys.InputType)
					{
						data =  // add the type of keypress
							Input.GetKeyDown(InputKey) ? InputType.PRESSED :
							(Input.GetKeyUp(InputKey) ? InputType.RELEASED : InputType.HELD)
					})
					);
			}
			else if (AxisName != "")
			{
				float deadzone = 0.1f;

				float value = Input.GetAxis(AxisName);
				if (Mathf.Abs(value) < deadzone)
					value = 0;
				else
				{
					float sign = value > 0 ? 1 : -1;
					value = sign * (Mathf.Abs(value) - deadzone) / (1 - deadzone);
				}

				Trigger(new EventData()
					.Add(new DataPiece(EventDataKeys.InputAxis) { data = AxisName }) // which axis is active
					.Add(new DataPiece(EventDataKeys.InputValue) { data = value }) // what's the axis value (-1..1)
					.Add(new DataPiece(EventDataKeys.InputType) { data = InputType.AXIS }) // the type of input
					);
			}
		}
	}
}
