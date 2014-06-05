using UnityEngine;
using System.Collections;

public class StandardActor : Actor
{
	void Update()
	{
		UpdateEvents();
	}

	public override void ShowGui()
	{
		GUILayout.Label("Showing a standard actor");
	}
}
