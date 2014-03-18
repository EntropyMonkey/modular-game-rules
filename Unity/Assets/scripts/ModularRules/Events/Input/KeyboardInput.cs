using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class KeyboardInput : InputReceived
	{
		public class KeyboardData : InputData
		{
			public KeyCode key;

			public override string ToString()
			{
				return base.ToString() + " KeyCode: " + key;
			}
		};

		[SerializeField]
		private KeyCode InputKey = KeyCode.None;

		public override GameEvent UpdateEvent()
		{
			if (!base.UpdateEvent()) return null;

			KeyboardData k = new KeyboardData
			{
				key = InputKey
			};

			if (InputKey != KeyCode.None)
			{
				if (Input.GetKey(InputKey))
				{
					k.inputValue = 1.0f;
					k.inputType = Input.GetKeyDown(InputKey) ? InputType.PRESSED : InputType.HELD;

					Trigger(EventData.Empty.Add(new DataPiece(EventDataKeys.InputData) { data = k }));
				}
				else if (Input.GetKeyUp(InputKey))
				{
					k.inputValue = -1.0f;
					k.inputType = InputType.RELEASED;

					Trigger(EventData.Empty.Add(new DataPiece(EventDataKeys.InputData) { data = k }));
				}
			}

			return this;
		}
	}
}
