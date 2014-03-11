﻿using UnityEngine;
using System.Collections;
using ModularRules;

namespace ModularRules
{
	public class MoveObject : Reaction
	{
		public enum Direction { FORWARD, BACKWARD, LEFT, RIGHT, UP, DOWN }

		[SerializeField]
		private Direction MoveDirection;

		[SerializeField]
		private Actor reactor;

		void OnEnable()
		{
			if (reactor == null) reactor = transform.parent.gameObject.GetComponent<Actor>();

			ListenedEvent.Register(this);
		}

		void OnDisable()
		{
			ListenedEvent.Unregister(this);
		}

		protected override void React(EventData eventData)
		{
			if (eventData == null) return;

			DataPiece input = eventData.Get(EventDataKeys.InputValue);

			float value = (float)input.data;

			if (reactor as IMovable != null)
				((IMovable)reactor).Move(eventData, MoveDirection);
		}
	}
}
