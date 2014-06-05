using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeactivateObject : Reaction
{
	public Actor ObjectToDeactivate;

	public float Timeout;

	private bool deactivating = false;

	public override RuleData GetRuleInformation()
	{
		RuleData data = base.GetRuleInformation();

		if (data.parameters == null) data.parameters = new List<Param>();

		if (ObjectToDeactivate != null)
		{
			data.parameters.Add(new Param()
			{
				name = "ObjectToDeactivate",
				type = ObjectToDeactivate.GetType(),
				value = ObjectToDeactivate.Id
			});
		}

		data.parameters.Add(new Param()
		{
			name = "Timeout",
			type = Timeout.GetType(),
			value = Timeout
		});

		return data;
	}

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		if (ObjectToDeactivate == null)
		{
			ObjectToDeactivate = Reactor;
		}
	}

	public override void ShowGui()
	{
		GUILayout.Label("deactivate", RuleGUI.ruleLabelStyle);
	}

	protected override void React(GameEventData eventData)
	{
		if (!deactivating)
			StartCoroutine(DeactivateAfter(Timeout));
		deactivating = true;
	}

	IEnumerator DeactivateAfter(float t)
	{
		yield return new WaitForSeconds(t);
		ObjectToDeactivate.gameObject.SetActive(false);
		deactivating = false;
	}
}
