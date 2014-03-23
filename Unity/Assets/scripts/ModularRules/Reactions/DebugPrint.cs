using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class DebugPrint : Reaction
	{
		void OnEnable()
		{
			if (ListenedEvent)
				ListenedEvent.Register(this);
		}

		void OnDisable()
		{
			if (ListenedEvent)
				ListenedEvent.Unregister(this);
		}

		protected override void React(EventData eventData)
		{
			Debug.Log("Received event " + eventData.ToString());
		}
	}
}
