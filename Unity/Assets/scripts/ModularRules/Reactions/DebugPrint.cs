using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class DebugPrint : Reaction
	{
		void OnEnable()
		{
			ListenedEvent.Register(this);
		}

		void OnDisable()
		{
			ListenedEvent.Unregister(this);
		}

		protected override void React(EventData eventData)
		{
			Debug.Log("Received event " + eventData.ToString());
		}
	}
}
