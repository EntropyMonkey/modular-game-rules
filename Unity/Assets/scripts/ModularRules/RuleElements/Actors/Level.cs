using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : Actor
{
	public static string Tag = "Level";

	void Start()
	{
		tag = Tag;
	}

	void Update()
	{
		UpdateEvents();
	}
}
