using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// These GenerateElements only contain the id for the actor which it represents.
/// </summary>
public class PlaceholderElement : MonoBehaviour
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