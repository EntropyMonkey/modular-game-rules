using UnityEngine;
using System.Collections;

using ModularRules;
using System.Collections.Generic;

public enum MovementBehaviour { SIMPLE, FLOCK }

public class Player : Actor
{
	public MovementBehaviour StartMovementBehaviour;

	private MovementController movementController;
	private ModularRules.MovementBehaviour simpleMovement;

	public override BaseRuleElement.RuleData GetRuleInformation()
	{
		BaseRuleElement.ActorData data = base.GetRuleInformation() as BaseRuleElement.ActorData;

		data.parameters = new List<Param>();
		data.parameters.Add(new Param
		{
			name = "StartMovementBehaviour",
			type = StartMovementBehaviour.GetType(),
			value = StartMovementBehaviour
		});

		return data;
	}

	void Awake()
	{
		movementController = gameObject.AddComponent<MovementController>();
		simpleMovement = gameObject.AddComponent<ShooterMovement>();
	}

	// Use this for initialization
	void Start()
	{
		switch (StartMovementBehaviour)
		{
			case MovementBehaviour.SIMPLE:
				movementController.ChangeBehaviour(simpleMovement);
				break;
			case MovementBehaviour.FLOCK:
				movementController.ChangeBehaviour(simpleMovement);
				break;
		}
	}

	// Update is called once per frame
	void Update()
	{
		UpdateEvents();
	}
}
