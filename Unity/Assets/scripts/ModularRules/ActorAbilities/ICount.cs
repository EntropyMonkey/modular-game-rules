using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public interface ICount
	{
		void ChangeCount(EventData data, float changeBy);
	}
}
