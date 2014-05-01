using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class ChangeCounter : Reaction
	{
		public int ChangeBy = -1;

		void OnEnable()
		{
			Register();
		}

		void OnDisable()
		{
			Unregister();
		}

		protected override void React(GameEventData eventData)
		{
			//GameObject target = (GameObject)eventData.Get(EventDataKeys.TargetObject).data;
			//if (target == null || (target == Reactor.gameObject && Reactor is ICount))
			//{
			//	((ICount)Reactor).ChangeCount(eventData, ChangeBy);
			//}
		}
	}
}
