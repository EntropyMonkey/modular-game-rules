using UnityEngine;
using System.Collections;

public class guitester : MonoBehaviour
{
	void OnGUI()
	{
		GUI.Button(new Rect(0, 0, 200, 50), "I'm a test! My mom's so proud of me!");
	}
}
