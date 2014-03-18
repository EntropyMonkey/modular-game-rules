using UnityEngine;
using System.Collections;

namespace ModularRules
{
	/// <summary>
	/// The rotation follows a relative movement value
	/// </summary>
	public class RotationFollowMovement : Reaction
	{
		void OnEnable()
		{
			ListenedEvent.Register(this);
		}

		void OnDisable()
		{
			ListenedEvent.Unregister(this);
		}

		protected override void React(EventData data)
		{
			DataPiece inputData;
			if ((inputData = data.Get(EventDataKeys.InputData)) != null &&
				inputData.dataType == typeof(MouseInput.MouseData))
			{
				Vector2 deltaMovement = ((MouseInput.MouseData)inputData.data).axisValues;

				if (Reactor is IRotate)
				{
					((IRotate)Reactor).Rotate(data, deltaMovement);
				}
			}
		}
	}
}
