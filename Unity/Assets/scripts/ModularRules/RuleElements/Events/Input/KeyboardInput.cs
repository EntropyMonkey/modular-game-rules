using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	public KeyCode InputKey = KeyCode.None;

	private DropDown keyDropDown;
	private string[] dropdownNames;

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		dropdownNames = System.Enum.GetNames(typeof(KeyCode));

		keyDropDown = new DropDown(System.Array.FindIndex(dropdownNames, item => item == InputKey.ToString()), dropdownNames);
	}

	public override RuleData GetRuleInformation()
	{
		EventData rule = base.GetRuleInformation() as EventData;

		rule.parameters = new List<Param>();
		rule.parameters.Add(new Param()
		{
			name = "InputKey",
			type = InputKey.GetType(),
			value = InputKey
		});

		return rule;
	}

	public override void ShowGui(RuleData ruleData)
	{
		base.ShowGui(ruleData);

		GUILayout.Label("On Keypress", RuleGUI.ruleLabelStyle);
		
		int resultIndex = keyDropDown.Draw();
		if (resultIndex > -1)
		{
			InputKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), dropdownNames[resultIndex]);

			ChangeParameter("InputKey", (ruleData as EventData).parameters, (int)InputKey);
		}
	}

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

				Trigger(GameEventData.Empty.Add(new DataPiece(EventDataKeys.InputData) { data = k }));
			}
			else if (Input.GetKeyUp(InputKey))
			{
				k.inputValue = -1.0f;
				k.inputType = InputType.RELEASED;

				Trigger(GameEventData.Empty.Add(new DataPiece(EventDataKeys.InputData) { data = k }));
			}
		}

		return this;
	}
}
