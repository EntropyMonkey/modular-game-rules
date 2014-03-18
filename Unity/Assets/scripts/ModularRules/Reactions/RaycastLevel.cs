using UnityEngine;
using System.Collections;

namespace ModularRules
{
	[RequireComponent(typeof(RaycastHitEvent))]
	public class RaycastLevel : Reaction
	{
		public float raycastDistance = 100;

		public LayerMask raycastLayers = -1;

		void OnEnable()
		{
			ListenedEvent.Register(this);
		}

		void OnDisable()
		{
			ListenedEvent.Unregister(this);
		}

		protected override void React(EventData eventData)
		{
			RaycastHit hitInfo;
			Ray ray = new Ray();

			// handle different kinds of event data
			// MouseInput event

			MouseInput.MouseData mouseData = (MouseInput.MouseData)eventData.Get(EventDataKeys.InputData).data;
			if (mouseData != null && mouseData.inputValue > 0)
			{
				ray = mouseData.rayFromPosition;
			}

			// raycast
			if (ray.direction != Vector3.zero)
			{
				Physics.Raycast(ray, out hitInfo, raycastDistance, raycastLayers.value);

				if (hitInfo.collider != null)
				{
					GetComponent<RaycastHitEvent>().Trigger(
						EventData.Empty.Add(new DataPiece(EventDataKeys.RaycastData) { data = hitInfo })
						.Add(new DataPiece(EventDataKeys.TargetObject) { data = hitInfo.collider.gameObject }));
				}
			}
		}
	}
}
