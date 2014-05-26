using UnityEngine;
using System.Collections;

public class LoadRulesButton : MonoBehaviour
{
	public string Filename = "rules_0";

	[SerializeField]
	ParticleSystem feedbackParticles;

	void Awake()
	{
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
		FindObjectOfType<RuleGenerator>().LoadRules(Filename);

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
