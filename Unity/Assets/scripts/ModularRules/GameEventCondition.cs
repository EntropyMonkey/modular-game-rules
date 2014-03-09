using UnityEngine;
using System.Collections;

namespace ModularRules
{
	// to be overridden with more complex conditions
	public class GameEventCondition : MonoBehaviour
	{
		/// <summary>
		/// True if condition is met
		/// </summary>
		public bool IsTrue
		{
			get;
			protected set;
		}

		void Awake()
		{
			IsTrue = true;
		}
	}
}
