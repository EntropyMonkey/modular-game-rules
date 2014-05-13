using UnityEngine;
using System.Collections;

public class LoadRulesButton : MonoBehaviour
{
	public string Filename = "rules_0";

	void OnMouseDown()
	{
		FindObjectOfType<ModularRules.RuleGenerator>().LoadRules(Filename);
	}
}
