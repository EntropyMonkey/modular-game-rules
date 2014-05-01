using UnityEngine;
using System.Collections;

using ModularRules;

public class Player : Actor
{
	public string Tag = "Player";

	void Start()
	{
		tag = Tag;
	}

	// Update is called once per frame
	void Update()
	{
		UpdateEvents();
	}
}
