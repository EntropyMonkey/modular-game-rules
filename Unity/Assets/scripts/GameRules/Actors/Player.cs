using UnityEngine;
using System.Collections;

using ModularRules;
using System.Collections.Generic;

public class Player : Actor
{
	public enum MovementBehaviour { SHOOTER }

	public MovementBehaviour StartMovementBehaviour;

	private MovementController movementController;
	private ModularRules.MovementBehaviour shooterMovement;

	void Awake()
	{
		Initialize();

		movementController = AddComponent<MovementController>();
		shooterMovement = AddComponent<ShooterMovement>();
	}

	// Use this for initialization
	void Start()
	{
		switch (StartMovementBehaviour)
		{
			case MovementBehaviour.SHOOTER:
				movementController.ChangeBehaviour(shooterMovement);
				break;
		}
	}

	public T AddComponent<T>() where T : Component
	{
		return gameObject.AddComponent<T>();
	}

	// Update is called once per frame
	void Update()
	{
		UpdateEvents();
	}
}
