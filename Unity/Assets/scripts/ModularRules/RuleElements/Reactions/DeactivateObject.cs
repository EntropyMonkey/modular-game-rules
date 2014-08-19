using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeactivateObject : Reaction
{
	public enum TargetObject { EVENT_TARGET, ACTOR };

	public Actor ObjectToDeactivate;
	private GameObject gameObjectToDeactivate;

	public float Timeout;

	private bool deactivating = false;

	private ActorDropDown actorDropDown;
	private DropDown targetObjectDropDown;

	public TargetObject targetObject;

	private RuleGenerator generator;

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
				name = "targetObject",
				type = targetObject.GetType(),
				value = targetObject
			});

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

		this.generator = generator;

		if (ObjectToDeactivate == null)
		{
			ObjectToDeactivate = Reactor;
		}

		actorDropDown = new ActorDropDown(
			System.Array.FindIndex(generator.Gui.ActorNames, item => item == Reactor.Label),
			generator.Gui.ActorNames,
			ref generator.Gui.OnAddedActor, ref generator.Gui.OnRenamedActor, ref generator.Gui.OnDeletedActor);

		targetObjectDropDown = new DropDown(
			(int)targetObject,
			System.Enum.GetNames(typeof(TargetObject)));

	}

	void OnEnable()
	{
		Register();
	}

	void OnDisable()
	{
		Unregister();
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.Label("deactivate", RuleGUI.ruleLabelStyle);

		targetObject = (TargetObject)targetObjectDropDown.Draw();

		if (targetObject == TargetObject.EVENT_TARGET)
		{
		}
		else
		{
			int resultIndex = actorDropDown.Draw();
			if (resultIndex > -1)
			{
				int resultId = generator.Gui.GetActorDataByLabel(actorDropDown.Content[resultIndex].text).id;
				(ruleData as ReactionData).actorId = resultId;
				ChangeParameter("ObjectToDeactivate", (ruleData as ReactionData).parameters, resultId);
				generator.ChangeActor(this, resultId);
				ObjectToDeactivate = Reactor;
			}
		}

		GUILayout.Label("after", RuleGUI.ruleLabelStyle);

		Timeout = RuleGUI.ShowParameter(Timeout);
		ChangeParameter("Timeout", (ruleData as ReactionData).parameters, Timeout);

		GUILayout.Label("seconds", RuleGUI.ruleLabelStyle);
	}

	protected override void React(GameEventData eventData)
	{
		if (targetObject == TargetObject.EVENT_TARGET)
			gameObjectToDeactivate = eventData.Get(EventDataKeys.TargetObject).data as GameObject;
		else
			gameObjectToDeactivate = ObjectToDeactivate.gameObject;

		if (!deactivating)
			StartCoroutine(DeactivateAfter(Timeout));
		deactivating = true;
	}

	IEnumerator DeactivateAfter(float t)
	{
		yield return new WaitForSeconds(t);
		gameObjectToDeactivate.SetActive(false);
		deactivating = false;
	}
}
