using UnityEngine;
using System.Collections;

using ModularRules;
using System.Collections.Generic;

public enum MovementBehaviour { SIMPLE, FLOCK }

public class Player : Actor
{
	public MovementBehaviour StartMovementBehaviour;

	private MovementController movementController;
	private SimpleMovement simpleMovement;
	private FlockMovement flockMovement;

	#region RuleInfo
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
		data.components = new List<ComponentData>();
		ComponentData c = new ComponentData()
		{
			type = typeof(SimpleMovement),
			parameters = new List<Param>()
		};
		if (simpleMovement)
		{
			c.parameters.Add(new Param()
			{
				name = "RunSpeed",
				type = simpleMovement.RunSpeed.GetType(),
				value = simpleMovement.RunSpeed
			});
			c.parameters.Add(new Param()
			{
				name = "JumpSpeed",
				type = simpleMovement.JumpSpeed.GetType(),
				value = simpleMovement.JumpSpeed
			});
		}

		if (flockMovement)
		{
			c.parameters.Add(new Param()
			{
				name = "StandardFlySpeed",
				type = flockMovement.StandardFlySpeed.GetType(),
				value = flockMovement.StandardFlySpeed
			});
			c.parameters.Add(new Param()
			{
				name = "Gravity",
				type = flockMovement.Gravity.GetType(),
				value = flockMovement.Gravity
			});
		}
		data.components.Add(c);

		return data;
	}
	#endregion

	void Awake()
	{
		movementController = gameObject.AddComponent<MovementController>();
	}

	// Use this for initialization
	void Start()
	{
		switch (StartMovementBehaviour)
		{
			case MovementBehaviour.SIMPLE:
				simpleMovement = gameObject.AddComponent<SimpleMovement>();
				movementController.ChangeBehaviour(simpleMovement);
				break;
			case MovementBehaviour.FLOCK:
				flockMovement = gameObject.AddComponent<FlockMovement>();
				movementController.ChangeBehaviour(flockMovement);
				break;
		}
	}

	// Update is called once per frame
	void Update()
	{
		UpdateEvents();
	}
}
