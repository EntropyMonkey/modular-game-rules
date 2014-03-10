using UnityEngine;
using System.Collections;

using ModularRules;
using System.Collections.Generic;

public class Player : Actor, IMovable
{
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		UpdateEvents();
	}

	// implementing the IMovable interface
	public void Move(EventData eventData, MoveObject.Direction direction)
	{

	}
}
