using UnityEngine;
using System.Collections;

using ModularRules;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Player : Actor, IMove
{
	[SerializeField]
	private float runSpeed = 10;

	[SerializeField]
	private float jumpSpeed = 20;

	private float moveSpeed;

	public PlayerCamera PlayerCamera
	{
		get;
		set;
	}

	void Awake()
	{
		InitializeActor();
	}

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

		float v = ((InputReceived.InputData)eventData.Get(EventDataKeys.InputData).data).inputValue;

		moveSpeed = runSpeed;

		Vector3 dir = Vector3.zero;
		switch (direction)
		{
			case MoveObject.Direction.FORWARD:
				dir = PlayerCamera.transform.forward;
				dir.y = 0;
				break;
			case MoveObject.Direction.BACKWARD:
				dir = -PlayerCamera.transform.forward;
				dir.y = 0;
				break;
			case MoveObject.Direction.LEFT:
				dir = -PlayerCamera.transform.right;
				dir.y = 0;
				break;
			case MoveObject.Direction.RIGHT:
				dir = PlayerCamera.transform.right;
				dir.y = 0;
				break;
			case MoveObject.Direction.UP:
				moveSpeed = jumpSpeed;
				dir = Vector3.up;
				break;
			case MoveObject.Direction.DOWN:
				dir = Vector3.down;
				break;
		}

		rigidbody.AddForce(dir * v * moveSpeed);
		
		moveSpeed = 0;
	}
}
