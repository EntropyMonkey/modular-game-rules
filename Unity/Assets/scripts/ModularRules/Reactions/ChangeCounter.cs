﻿using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class ChangeCounter : Reaction
	{
		public int ChangeBy = -1;

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
			GameObject target = (GameObject)eventData.Get(EventDataKeys.TargetObject).data;
			if (target == null || (target == Reactor.gameObject && Reactor is ICount))
			{
				((ICount)Reactor).ChangeCount(eventData, ChangeBy);
			}
		}
	}
}
