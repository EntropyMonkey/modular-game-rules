using UnityEngine;
using System.Collections;

public class DebugPrint : Reaction
{
	void OnEnable()
	{
		Register();
	}

	void OnDisable()
	{
		Unregister();
	}

	protected override void React(GameEventData eventData)
	{
		Debug.Log("Received event " + eventData.ToString());
	}

	public override void ShowGui()
	{
		GUILayout.Label("log");
	}
}
