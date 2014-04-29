using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public interface IMove
	{
		void Move(GameEventData eventData, Direction direction);
	}
}