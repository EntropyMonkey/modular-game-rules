using UnityEngine;
using System.Collections;
using System;

public class keycodeTester : MonoBehaviour {
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetMouseButtonDown(0))
		{
			string[] names = Enum.GetNames(typeof(KeyCode));

			for (int i = 0; i < names.Length; i++)
			{
				int result = (int)Enum.Parse(typeof(KeyCode), names[i]);
				int resultTwo = Array.FindIndex(names, item => item == KeyCode.A.ToString());
				Debug.Log(result);
				Debug.Log(names[resultTwo]);
			}
		}
	}
}
