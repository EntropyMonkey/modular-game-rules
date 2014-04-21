using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ModularRules
{
	/// <summary>
	/// These GenerateElements only contain the id for the actor which it represents.
	/// </summary>
	public class GenerateElement : MonoBehaviour
	{
		public int Id;

		[HideInInspector]
		public List<Component> OriginalComponents;

		void Awake()
		{
			OriginalComponents = new List<Component>();
			OriginalComponents.AddRange(GetComponents(typeof(Component)));
		}

		void Update()
		{

		}
	}
}
