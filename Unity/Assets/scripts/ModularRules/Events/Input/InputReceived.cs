using UnityEngine;
using System.Collections;

public class InputReceived : GameEvent
{
	public enum InputType { NONE = 0, HELD, PRESSED, RELEASED, CONTINUOUS }

	public class InputData
	{
		public InputType inputType;
		public float inputValue;

		public override string ToString()
		{
			return " InputType: " + inputType + " InputValue: " + inputValue;
		}
	};

	public bool ReceiveInput = true;

	public override GameEvent UpdateEvent()
	{
		if (!ReceiveInput) return null;
		else return this;
	}
}
