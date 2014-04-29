using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public interface IRotate
	{
		void Rotate(GameEventData data, Vector3 deltaMovement);
	}
}