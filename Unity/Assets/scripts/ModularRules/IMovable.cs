using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public interface IMovable
	{
		void Move(EventData eventData, MoveObject.Direction direction);
	}
}