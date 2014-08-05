using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClickToggleActive : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> toggleObjects;

	[SerializeField]
	ParticleSystem feedbackParticles;

	void Awake()
	{
		if (collider == null)
			gameObject.AddComponent<BoxCollider>();

		if (feedbackParticles)
		{
			feedbackParticles = Instantiate(feedbackParticles, transform.position, Quaternion.identity) as ParticleSystem;
			feedbackParticles.transform.parent = transform;
			feedbackParticles.transform.localPosition = Vector3.zero;
			feedbackParticles.gameObject.layer = LayerMask.NameToLayer("GUI");
			feedbackParticles.enableEmission = false;
		}
	}

	void OnMouseDown()
	{
		foreach (GameObject toggle in toggleObjects)
		{
			toggle.SetActive(!toggle.activeSelf);
		}

		if (feedbackParticles)
		{
			feedbackParticles.Emit(Vector3.zero, Vector3.zero, 5f, 1f, Color.black);
		}
	}

	void OnMouseOver()
	{
		if (feedbackParticles)
		{
			feedbackParticles.Emit(1);
		}
	}
}
