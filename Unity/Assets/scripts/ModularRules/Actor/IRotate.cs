using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public interface IRotate
	{
		void Rotate(EventData data, Vector3 deltaMovement);
	}
}