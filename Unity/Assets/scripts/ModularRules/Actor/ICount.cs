using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public interface ICount
	{
		void ChangeCount(GameEventData data, float changeBy);
	}
}
