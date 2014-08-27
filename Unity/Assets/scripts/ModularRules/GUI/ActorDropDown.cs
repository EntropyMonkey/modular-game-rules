using UnityEngine;
using System.Collections;

public class ActorDropDown : DropDown
{
	public ActorDropDown(int selected, string[] content, 
		ref RuleGUI.ActorChanged onActorAdded, ref RuleGUI.ActorRenamed onActorRenamed, ref RuleGUI.ActorChanged onActorDeleted, 
		float width = 100, float minHeight = 100) :
		
		base(selected, content, width, minHeight)
	{
		onActorDeleted += OnDeletedActor;
		onActorAdded += OnAddedActor;
		onActorRenamed += OnRenamedActor;
	}

	void OnAddedActor(string[] newContent, BaseRuleElement.ActorData newActor)
	{
		string[] newNames = new string[Content.Length + 1];

		// copy old names
		int i = 0;
		for (; i < Content.Length; i++)
			newNames[i] = Content[i].text;

		// add new name
		newNames[i++] = newActor.label;

		ChangeContent(newNames);
	}

	void OnRenamedActor(string[] newContent, BaseRuleElement.ActorData changedActor, string oldName)
	{
		for (int i = 0; i < Content.Length; i++)
		{
			if (Content[i].text == oldName)
			{
				Content[i].text = changedActor.label;
				break;
			}
		}
	}

	void OnDeletedActor(string[] newContent, BaseRuleElement.ActorData oldActor)
	{
		string[] newNames = new string[newContent.Length]; // there should be only one actor with the old label in the list
		int oldSelected = System.Array.FindIndex(Content, item => item.text == oldActor.label);
		if (oldSelected == -1)
			extended = true;

		int j = 0;
		for (int i = 0; i < Content.Length; i++)
		{
			if (Content[i].text != oldActor.label)
			{
				newNames[j] = Content[i].text;
				j++;
			}
		}

		if (oldSelected >= 0 && oldSelected < Content.Length && Selected > oldSelected)
		{
			string oldName = Content[oldSelected].text;
			Selected = System.Array.FindIndex(newNames, item => item == oldName);
		}

		ChangeContent(newNames);
	}
}
