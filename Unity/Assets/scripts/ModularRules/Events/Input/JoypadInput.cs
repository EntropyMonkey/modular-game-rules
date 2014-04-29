using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	public class JoypadInput : InputReceived
	{
		public class JoypadData : InputData
		{
			public string axisName;
		};

		public string AxisName = "";

		private float lastAxisValue = 0;

		public override RuleData GetRuleInformation()
		{
			EventData rule = base.GetRuleInformation() as EventData;

			rule.parameters = new List<Param>();
			rule.parameters.Add(new Param() 
			{ 
				name = "AxisName",
				type = AxisName.GetType(),
				value = AxisName
			});

			return rule;
		}

		public override GameEvent UpdateEvent()
		{
			if (!base.UpdateEvent()) return null;

			if (AxisName != "")
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

				if (lastAxisValue != value)
				{
					lastAxisValue = value;
					Trigger(GameEventData.Empty.Add(new DataPiece(EventDataKeys.InputData)
					{
						data = new JoypadData
						{
							axisName = AxisName,
							inputValue = value,
							inputType = InputType.CONTINUOUS
						}
					}));
				}
			}

			return this;
		}
	}
}
