using UnityEngine;
using System.Collections;

namespace ModularRules
{
	public class ShooterMovement : MovementBehaviour
	{
		public float runSpeed = 10;
		public float jumpSpeed = 20;

		private float moveSpeed;

		private PlayerCamera playerCamera;

		public override void Load()
		{
			base.Load();

			if (rigidbody == null)
				gameObject.AddComponent(typeof(Rigidbody));
			rigidbody.freezeRotation = true;

			playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<PlayerCamera>();
		}

		public override void Move(GameEventData eventData, Direction direction)
		{

			float v = ((InputReceived.InputData)eventData.Get(EventDataKeys.InputData).data).inputValue;

			moveSpeed = runSpeed;

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
					dir = -playerCamera.transform.right;
					dir.y = 0;
					break;
				case Direction.RIGHT:
					dir = playerCamera.transform.right;
					dir.y = 0;
					break;
				case Direction.UP:
					moveSpeed = jumpSpeed;
					dir = Vector3.up;
					break;
				case Direction.DOWN:
					dir = Vector3.down;
					break;
			}

			rigidbody.AddForce(dir * v * moveSpeed);

			moveSpeed = 0;
		}
	}
}
