using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;


public class RuleGUI : MonoBehaviour
{
//	public RuleGenerator ruleGenerator;


	public bool ShowButtons = true;

	public string[] Files;

	public GUISkin CustomSkin;

	GUIStyle markedButtonOriginStyle;
	GUIStyle markedButtonChildStyle;
	GUIStyle buttonStyle;
	GUIStyle areaBackgroundStyle;
	GUIStyle labelSmallStyle;

	bool editMode = false;
	bool loadMode = false;
	bool showDetails = false;

	BaseRuleElement.ActorData currentActor;

	int markedActor = -1;
	int markedEvent = -1;
	int markedReaction = -1;

	bool showOnlyRelevant = true;

	Dictionary<int, List<int>> eventReactionDict = new Dictionary<int,List<int>>();
	Dictionary<int, List<int>> actorEventDict = new Dictionary<int, List<int>>();
	Dictionary<int, List<int>> actorReactionDict = new Dictionary<int, List<int>>();

	List<BaseRuleElement.ActorData> actorData = new List<BaseRuleElement.ActorData>();
	List<BaseRuleElement.EventData> eventData = new List<BaseRuleElement.EventData>();
	List<BaseRuleElement.ReactionData> reactionData = new List<BaseRuleElement.ReactionData>();

	void Awake()
	{
		GetComponent<RuleGenerator>().OnParsedRules += OnParsedRules;

		markedButtonOriginStyle = CustomSkin.GetStyle("markedButtonOrigin");
		markedButtonChildStyle = CustomSkin.GetStyle("markedButtonChild");
		buttonStyle = CustomSkin.GetStyle("button");
		areaBackgroundStyle = CustomSkin.GetStyle("areaBackgroundStyle");
		labelSmallStyle = CustomSkin.GetStyle("labelSmallStyle");
	}

	void OnParsedRules(List<BaseRuleElement.ActorData> originalActorData, List<BaseRuleElement.EventData> originalEventData, List<BaseRuleElement.ReactionData> originalReactionData)
	{
		actorData.Clear();
		eventData.Clear();
		reactionData.Clear();

		// deep copy lists
		for (int i = 0; i < originalActorData.Count; i++)
		{
			actorData.Add(originalActorData[i].DeepCopy());
		}
		for (int i = 0; i < originalEventData.Count; i++)
		{
			eventData.Add(originalEventData[i].DeepCopy());
		}
		for (int i = 0; i < originalReactionData.Count; i++)
		{
			reactionData.Add(originalReactionData[i].DeepCopy());
		}

		// events/reactions and actors/reactions
		eventReactionDict.Clear();
		actorReactionDict.Clear();

		for (int i = 0; i < reactionData.Count; i++)
		{
			BaseRuleElement.ReactionData rData = reactionData[i];
			if (eventReactionDict.ContainsKey(rData.eventId))
			{
				eventReactionDict[rData.eventId].Add(rData.id);
			}
			else
			{
				eventReactionDict.Add(rData.eventId, new List<int>() { rData.id });
			}

			if (actorReactionDict.ContainsKey(rData.actorId))
			{
				actorReactionDict[rData.actorId].Add(rData.id);
			}
			else
			{
				actorReactionDict.Add(rData.actorId, new List<int>() { rData.id });
			}
		}

		// actors/events
		actorEventDict.Clear();
		for (int i = 0; i < eventData.Count; i++)
		{
			BaseRuleElement.EventData eData = eventData[i];
			if (actorEventDict.ContainsKey(eData.actorId))
			{
				actorEventDict[eData.actorId].Add(eData.id);
			}
			else
			{
				actorEventDict.Add(eData.actorId, new List<int>() { eData.id });
			}
		}
	}

	void OnGUI()
	{
		GUI.skin = CustomSkin;

		if (editMode)
		{
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height), areaBackgroundStyle);
		}
		else
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));

		GUILayout.BeginHorizontal();

		EditRules();

		LoadRules();

		GUILayout.EndHorizontal();

		GUILayout.EndArea();
	}

	void LoadRules()
	{
		if (ShowButtons && !loadMode && !editMode && GUI.Button(new Rect(0, Screen.height - 100, 100, 50), "Load Rules"))
		{
			loadMode = true;
		}

		if (!loadMode) return;

		GUILayout.BeginVertical(GUILayout.Width(Screen.width * 0.3f));

		GUILayout.BeginScrollView(Vector2.zero);

		for (int i = 0; i < Files.Length; i++)
		{
			string file = Path.GetFileNameWithoutExtension(Files[i]);
			if (GUILayout.Button("Load " + file, GUILayout.Height(70)))
			{
				GetComponent<RuleGenerator>().LoadRules(file);
				loadMode = false;
			}
		}

		GUILayout.EndScrollView();

		if (GUILayout.Button("Cancel"))
		{
			loadMode = false;
		}

		GUILayout.EndVertical();
	}

	void EditRules()
	{
		if (ShowButtons && !editMode && !loadMode && actorData.Count > 0 && 
			GUI.Button(new Rect(0, Screen.height - 50, 100, 50), "Edit Rules"))
		{
			editMode = true;
			GetComponent<RuleGenerator>().PauseEventExecution();
		}

		if (!editMode) return;

		//else if (GUI.Button(new Rect(0, Screen.height - 30, 50, 30), "Save"))
		//{

		//}
		else if (GUI.Button(new Rect(0, Screen.height - 30, 50, 30), "Done"))
		{
			editMode = false;
			currentActor = null;
			showDetails = false;
			GetComponent<RuleGenerator>().StartEventExecution();
		}

		//MainView();
		//ActorView();

		if (!showDetails)
		{
			GUILayout.BeginVertical(GUILayout.Height(Screen.height * 0.1f));
			
			showOnlyRelevant = GUILayout.Toggle(showOnlyRelevant, "Show only related events/reactions");

			GUILayout.BeginHorizontal();
			ScrollviewActors();
			ScrollviewEvents();
			ScrollviewReactions();
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}
		else
		{
			DetailWindow();
		}
	}

	#region SecondVersion
	void MainView()
	{
		if (currentActor != null) return;

		editActorsScrollValue = GUILayout.BeginScrollView(editActorsScrollValue);

		GUILayout.FlexibleSpace();
		GUILayout.Label("Choose actor:");
		GUILayout.BeginHorizontal(GUILayout.Height(Screen.height * 0.3f));
		for (int i = 0; i < actorData.Count; i++)
		{
			// draw button
			if (GUILayout.Button(actorData[i].label + "\n\t- " + actorData[i].type, GUILayout.Height(50)))
			{
				currentActor = actorData[i];
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();

		GUILayout.EndScrollView();
	}

	Vector2 eventScrollValue = Vector2.zero;
	void ActorView()
	{
		if (currentActor == null) return;

		GUILayout.BeginHorizontal(GUILayout.Width(Screen.width * 0.3f));
		GUILayout.Label(currentActor.label + "(" + currentActor.type + ")");
		GUILayout.EndHorizontal();

		GUILayout.BeginVertical(GUILayout.Width(Screen.width * 0.3f));
		GUILayout.Label("Events: ");

		eventScrollValue = GUILayout.BeginScrollView(eventScrollValue);

		if (actorEventDict.ContainsKey(currentActor.id))
		{
			for (int i = 0; i < actorEventDict[currentActor.id].Count; i++)
			{
				BaseRuleElement.EventData eData = eventData[actorEventDict[currentActor.id][i]];
				string buttonLabel = eData.label;
				if (eData.parameters != null && eData.parameters.Count > 0)
				{
					buttonLabel += "\n\t- " + eData.parameters[0].name + ": " + eData.parameters[0].value;
				}

				if (GUILayout.Button(buttonLabel))
				{

				}
			}
		}
		GUILayout.EndScrollView();

		GUILayout.EndVertical();
	}
	#endregion

	#region First Version

	Vector2 editActorsScrollValue = Vector2.zero;
	Vector2 editEventsScrollValue = Vector2.zero;
	Vector2 editReactionsScrollValue = Vector2.zero;
	void ScrollviewActors()
	{
		GUILayout.BeginVertical();
		GUILayout.Label("Actors:");
		editActorsScrollValue = GUILayout.BeginScrollView(editActorsScrollValue, GUILayout.Width(Screen.width * 0.3f), GUILayout.Height(Screen.height * 0.7f));

		for (int i = 0; i < actorData.Count; i++)
		{
			BaseRuleElement.ActorData aData = actorData[i];

			// choose right style for button
			GUIStyle style = buttonStyle;
			if (markedActor == aData.id)
				style = markedButtonOriginStyle;
			else if ((markedEvent > -1 && actorEventDict.ContainsKey(aData.id) &&
				actorEventDict[aData.id].Contains(markedEvent)) ||
				(markedReaction > -1 && actorReactionDict.ContainsKey(aData.id) &&
				actorReactionDict[aData.id].Contains(markedReaction)))
			{
				style = markedButtonChildStyle;
			}

			// draw button
			if (GUILayout.Button(actorData[i].label + "\n\t- " + actorData[i].type, style, GUILayout.Height(50)))
			{
				if (markedActor != actorData[i].id)
					MarkActor(actorData[i]);
				else
					ShowDetails(actorData[i]); // open details window
			}
		}

		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	void ShowDetails(BaseRuleElement.ActorData actorData)
	{
		showDetails = true;
		currentActor = actorData;
	}

	Vector2 parameterScrollPos = Vector2.zero;
	Vector2 detailEventScrollPos = Vector2.zero;
	Vector2 detailReactionScrollPos = Vector2.zero;
	void DetailWindow()
	{
		GUILayout.BeginVertical();

		GUILayout.Label(currentActor.label + " (" + currentActor.type + ")", GUILayout.Width(Screen.width * 0.3f));


		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Back", GUILayout.Width(50), GUILayout.Height(30)))
		{
			showDetails = false;
		}

		GUILayout.Label("Events");
		GUILayout.Label("Reactions");
		GUILayout.Label("Parameters");
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();

		GUILayout.Space(50);

		// events
		detailEventScrollPos = GUILayout.BeginScrollView(detailEventScrollPos, GUILayout.Width(Screen.width * 0.3f));

		foreach(BaseRuleElement.EventData e in eventData.FindAll(item => item.actorId == currentActor.id))
		{
			GUIStyle style = buttonStyle;
			if (markedEvent == e.id)
				style = markedButtonOriginStyle;
			else if (markedReaction > -1 && eventReactionDict.ContainsKey(e.id) && eventReactionDict[e.id].Contains(markedReaction))
			{
				style = markedButtonChildStyle;
			}

			if (GUILayout.Button(e.label, style, GUILayout.Height(50)))
			{
				if (markedEvent != e.id)
				{
					markedReaction = -1;
					markedEvent = e.id;
				}
				else
				{
					markedEvent = -1;
				}
			}
		}
		GUILayout.EndScrollView();

		// reactions
		detailReactionScrollPos = GUILayout.BeginScrollView(detailReactionScrollPos, GUILayout.Width(Screen.width * 0.3f));

		foreach (BaseRuleElement.ReactionData r in reactionData.FindAll(item => item.actorId == currentActor.id))
		{
			GUIStyle style = buttonStyle;
			if (markedReaction == r.id)
				style = markedButtonOriginStyle;
			else if (r.eventId == markedEvent)
				style = markedButtonChildStyle;

			if (GUILayout.Button(r.label, style, GUILayout.Height(50)))
			{
				if (markedReaction != r.id)
				{
					markedEvent = -1;
					markedReaction = r.id;
				}
				else
				{
					markedReaction = -1;
				}
			}
		}

		GUILayout.EndScrollView();



		// parameters
		parameterScrollPos = GUILayout.BeginScrollView(parameterScrollPos, GUILayout.Width(Screen.width * 0.3f));


		if (markedEvent == -1 && markedReaction == -1)
		{
			// extra parameters:
			GUI.enabled = false;
			ShowParameter("Id:", currentActor.id.ToString());
			ShowParameter("Type:", currentActor.type.ToString());
			GUI.enabled = true;

			currentActor.label = ShowParameter("Label: ", currentActor.label);

			HorizontalLine();

			ShowParameters(currentActor.parameters);
		}
		else if (markedEvent > -1)
		{
			BaseRuleElement.EventData e = eventData.Find(item => item.id == markedEvent);
			// extra parameters:
			GUI.enabled = false;
			ShowParameter("Id:", markedEvent.ToString());
			ShowParameter("Type:", e.type.ToString());
			GUI.enabled = true;

			e.label = ShowParameter("Label: ", e.label);

			HorizontalLine();

			ShowParameters(e.parameters);
		}
		else if (markedReaction > -1)
		{
			BaseRuleElement.ReactionData r = reactionData.Find(item => item.id == markedReaction);
			// extra parameters:
			GUI.enabled = false;
			ShowParameter("Id:", markedReaction.ToString());
			ShowParameter("Type:", r.type.ToString());
			GUI.enabled = true;

			r.label = ShowParameter("Label: ", r.label);

			HorizontalLine();

			ShowParameters(r.parameters);
		}

		GUILayout.EndScrollView();

		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
	}

	void HorizontalLine()
	{
		GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
	}

	void ShowParameters(List<BaseRuleElement.Param> parameters)
	{
		for (int i = 0; i < parameters.Count; i++)
		{
			ShowParameter(parameters[i]);
		}
	}

	void ShowParameter(BaseRuleElement.Param parameter)
	{
		GUILayout.BeginHorizontal(GUILayout.Width(Screen.width * 0.3f));
		GUILayout.Label(parameter.name + ": ", labelSmallStyle, GUILayout.Width(150));

		// handle parameter value according to type
		if (parameter.type.IsSubclassOf(typeof(Actor)) || parameter.type.IsAssignableFrom(typeof(Actor)))
		{
			//ShowParameter(parameter.name, )
		}
		else if (parameter.type.IsEnum)
		{
			string[] names = Enum.GetNames(parameter.type);
			int selected = GUILayout.SelectionGrid((int)parameter.value, names, 2);
			parameter.value = Enum.Parse(parameter.type, names[selected]);
		}
		else if (parameter.value is int)
		{
			parameter.value = int.Parse(GUILayout.TextField(parameter.value.ToString(), GUILayout.Width(100)));
		}
		else if (parameter.value is float)
		{
			parameter.value = float.Parse(GUILayout.TextField(parameter.value.ToString(), GUILayout.Width(100)));
		}
		else if (parameter.type == typeof(Vector3))
		{
			//Vector3 vec;
			//string[] s = v.Split(' ');
			//vec.x = float.Parse(s[0]);
			//vec.y = float.Parse(s[1]);
			//vec.z = float.Parse(s[2]);
			//newP.value = vec;
		}
		else if (parameter.value is bool)
		{
			parameter.value = GUILayout.Toggle((bool)parameter.value, "");
		}
		else if (parameter.type == typeof(string))
		{
			parameter.value = GUILayout.TextField(parameter.value.ToString(), GUILayout.Width(100));
		}
		else if (parameter.type == typeof(List<string>))
		{
			//string[] s = v.Split(' ');
		}

		GUILayout.EndHorizontal();
	}

	string ShowParameter(string label, string value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label + ": ", labelSmallStyle);
		string result = GUILayout.TextField(value);
		GUILayout.EndHorizontal();

		return result;
	}


	void ScrollviewEvents()
	{
		GUILayout.BeginVertical();
		GUILayout.Label("Events: ");

		// events
		editEventsScrollValue = GUILayout.BeginScrollView(editEventsScrollValue, GUILayout.Width(Screen.width * 0.3f));

		for (int i = 0; i < eventData.Count; i++)
		{
			BaseRuleElement.EventData eData = eventData[i];
			// create label
			string buttonLabel = eData.label;
			if (eData.parameters != null && eData.parameters.Count > 0)
			{
				buttonLabel += "\n\t- " + eData.parameters[0].name + ": " + eData.parameters[0].value;
			}

			bool markedRelevant = false;

			// choose style
			GUIStyle style = buttonStyle;
			if (markedEvent == eData.id)
			{
				style = markedButtonOriginStyle;
				markedRelevant = true;
			}
			else if (markedActor == eData.actorId ||
				(markedReaction > -1 && eventReactionDict.ContainsKey(eData.id) &&
				eventReactionDict[eData.id].Contains(markedReaction)))
			{
				style = markedButtonChildStyle;
				markedRelevant = true;
			}

			// draw button
			if ((showOnlyRelevant && markedRelevant || !showOnlyRelevant) && GUILayout.Button(buttonLabel, style, GUILayout.Height(50)))
			{
				if (markedEvent != eData.id)
					MarkEvent(eData);
				//else
				//	ShowDetails(eData);
			}
		}

		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	void ScrollviewReactions()
	{
		GUILayout.BeginVertical();
		GUILayout.Label("Reactions:");

		// reactions
		editReactionsScrollValue = GUILayout.BeginScrollView(editReactionsScrollValue, GUILayout.Width(Screen.width * 0.3f));

		for (int i = 0; i < reactionData.Count; i++)
		{
			BaseRuleElement.ReactionData rData = reactionData[i];

			// create label
			string buttonLabel = rData.label;
			if (rData.parameters != null && rData.parameters.Count > 0)
			{
				buttonLabel += "\n\t- " + rData.parameters[0].name + ": " + rData.parameters[0].value;
			}

			bool markedRelevant = false;
			// choose style
			GUIStyle style = buttonStyle;
			if (markedReaction == rData.id)
			{
				style = markedButtonOriginStyle;
				markedRelevant = true;
			}
			else if (markedActor == rData.actorId || markedEvent == rData.eventId)
			{
				style = markedButtonChildStyle;
				markedRelevant = true;
			}

			// draw button
			if ((showOnlyRelevant && markedRelevant || !showOnlyRelevant) && GUILayout.Button(buttonLabel, style, GUILayout.Height(50)))
			{
				MarkReaction(rData);
			}
		}

		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	void MarkActor(BaseRuleElement.ActorData origin)
	{
		markedActor = origin.id;
		markedEvent = -1;
		markedReaction = -1;
	}

	void MarkEvent(BaseRuleElement.EventData origin)
	{
		markedEvent = origin.id;
		markedActor = -1;
		markedReaction = -1;
	}

	void MarkReaction(BaseRuleElement.ReactionData origin)
	{
		markedReaction = origin.id;
		markedEvent = -1;
		markedActor = -1;
	}
	#endregion
}
