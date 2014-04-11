using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public interface IMove
	{
		void Move(EventData eventData, Direction direction);
	}
}