using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class FlockMovement : MovementBehaviour
	{
		public float StandardFlySpeed = 10;
		public float TurnDriftSpeed = 2;
		public float Gravity = 1;

		public float MaxPitchAngle = 50;
		public float MaxRollAngle = 50;

		private float moveSpeed;

		private PlayerCamera playerCamera;

		public override void Load()
		{
			base.Load();

			if (rigidbody == null)
				gameObject.AddComponent(typeof(Rigidbody));
			rigidbody.freezeRotation = true;
			rigidbody.useGravity = false;
			rigidbody.mass = 0.5f;

			playerCamera = GameObject.FindGameObjectWithTag(PlayerCamera.Tag).GetComponent<PlayerCamera>();
		}

		public override void Move(GameEventData eventData, Direction direction)
		{

			float v = ((InputReceived.InputData)eventData.Get(EventDataKeys.InputData).data).inputValue;

			moveSpeed = StandardFlySpeed;

			Vector3 dir = Vector3.zero;
			switch (direction)
			{
				case Direction.FORWARD:
					dir = playerCamera.transform.forward;
					dir.y = 0;
					break;
				case Direction.BACKWARD:
					dir = -playerCamera.transform.forward;
					dir.y = 0;
					break;
				case Direction.LEFT:
					moveSpeed = TurnDriftSpeed;
					dir = -playerCamera.transform.right;
					dir.y = 0;
					break;
				case Direction.RIGHT:
					moveSpeed = TurnDriftSpeed;
					dir = playerCamera.transform.right;
					dir.y = 0;
					break;
				case Direction.UP:
					dir = Vector3.up;
					break;
				case Direction.DOWN:
					dir = Vector3.down;
					break;
			}
						
			rigidbody.AddForce(dir * v * moveSpeed);
			transform.Rotate(Vector3.up, 1 * (direction == Direction.LEFT ? -1 : direction == Direction.RIGHT ? 1 : 0));

			moveSpeed = 0;
		}

		void Update()
		{
			rigidbody.AddForce(Gravity * Vector3.down);
		}
	}
}
