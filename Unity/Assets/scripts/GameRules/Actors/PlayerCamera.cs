using UnityEngine;
using System.Collections;
using ModularRules;

public class PlayerCamera : Actor
{
	public static string Tag = "PlayerCamera";

	void Start()
	{
		tag = Tag;
	}
}
