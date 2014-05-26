using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;


public class RuleGUI : MonoBehaviour
{
	public RuleGenerator ruleGenerator;

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

	void Awake()
	{
		ruleGenerator = GetComponent<RuleGenerator>();

		ruleGenerator.OnParsedRules += OnParsedRules;

		markedButtonOriginStyle = CustomSkin.GetStyle("markedButtonOrigin");
		markedButtonChildStyle = CustomSkin.GetStyle("markedButtonChild");
		buttonStyle = CustomSkin.GetStyle("button");
		areaBackgroundStyle = CustomSkin.GetStyle("areaBackgroundStyle");
		labelSmallStyle = CustomSkin.GetStyle("labelSmallStyle");
	}

	void OnParsedRules()
	{
		// events/reactions and actors/reactions
		eventReactionDict.Clear();
		actorReactionDict.Clear();

		for (int i = 0; i < ruleGenerator.ReactionData.Count; i++)
		{
			BaseRuleElement.ReactionData rData = ruleGenerator.ReactionData[i];
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
		for (int i = 0; i < ruleGenerator.EventData.Count; i++)
		{
			BaseRuleElement.EventData eData = ruleGenerator.EventData[i];
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
				ruleGenerator.LoadRules(file);
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
		if (ShowButtons && !editMode && !loadMode && ruleGenerator.ActorData.Count > 0 && 
			GUI.Button(new Rect(0, Screen.height - 50, 100, 50), "Edit Rules"))
		{
			editMode = true;
			ruleGenerator.PauseEventExecution();
		}

		if (!editMode) return;

		else if (GUI.Button(new Rect(0, Screen.height - 30, 100, 30), "Save"))
		{

		}
		else if (GUI.Button(new Rect(0, Screen.height - 60, 100, 30), "Cancel"))
		{
			editMode = false;
			currentActor = null;
			showDetails = false;
			ruleGenerator.StartEventExecution();
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
		for (int i = 0; i < ruleGenerator.ActorData.Count; i++)
		{
			// draw button
			if (GUILayout.Button(ruleGenerator.ActorData[i].label + "\n\t- " + ruleGenerator.ActorData[i].type, GUILayout.Height(50)))
			{
				currentActor = ruleGenerator.ActorData[i];
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
				BaseRuleElement.EventData eventData = ruleGenerator.EventData[actorEventDict[currentActor.id][i]];
				string buttonLabel = eventData.label;
				if (eventData.parameters != null && eventData.parameters.Count > 0)
				{
					buttonLabel += "\n\t- " + eventData.parameters[0].name + ": " + eventData.parameters[0].value;
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

		for (int i = 0; i < ruleGenerator.ActorData.Count; i++)
		{
			BaseRuleElement.ActorData actorData = ruleGenerator.ActorData[i];

			// choose right style for button
			GUIStyle style = buttonStyle;
			if (markedActor == actorData.id)
				style = markedButtonOriginStyle;
			else if ((markedEvent > -1 && actorEventDict.ContainsKey(actorData.id) &&
				actorEventDict[actorData.id].Contains(markedEvent)) ||
				(markedReaction > -1 && actorReactionDict.ContainsKey(actorData.id) &&
				actorReactionDict[actorData.id].Contains(markedReaction)))
			{
				style = markedButtonChildStyle;
			}

			// draw button
			if (GUILayout.Button(ruleGenerator.ActorData[i].label + "\n\t- " + ruleGenerator.ActorData[i].type, style, GUILayout.Height(50)))
			{
				if (markedActor != ruleGenerator.ActorData[i].id)
					MarkActor(ruleGenerator.ActorData[i]);
				else
					ShowDetails(ruleGenerator.ActorData[i]); // open details window
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
	void DetailWindow()
	{
		GUILayout.BeginVertical();

		GUILayout.Label(currentActor.label + " (" + currentActor.type + ")", GUILayout.Width(Screen.width * 0.3f));
		
		if (GUILayout.Button("Back", GUILayout.Width(100), GUILayout.Height(30)))
		{
			showDetails = false;
		}

		GUILayout.BeginHorizontal();

		parameterScrollPos = GUILayout.BeginScrollView(parameterScrollPos, GUILayout.Width(Screen.width * 0.3f));

		for (int i = 0; i < currentActor.parameters.Count; i++)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(currentActor.parameters[i].name + ": ", labelSmallStyle, GUILayout.MinWidth(100));
			GUILayout.TextField(currentActor.parameters[i].value.ToString(), GUILayout.Width(30));
			GUILayout.EndHorizontal();
		}

		GUILayout.EndScrollView();

		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
	}

	void ScrollviewEvents()
	{
		GUILayout.BeginVertical();
		GUILayout.Label("Events: ");

		// events
		editEventsScrollValue = GUILayout.BeginScrollView(editEventsScrollValue, GUILayout.Width(Screen.width * 0.3f));

		for (int i = 0; i < ruleGenerator.EventData.Count; i++)
		{
			BaseRuleElement.EventData eventData = ruleGenerator.EventData[i];
			// create label
			string buttonLabel = eventData.label;
			if (eventData.parameters != null && eventData.parameters.Count > 0)
			{
				buttonLabel += "\n\t- " + eventData.parameters[0].name + ": " + eventData.parameters[0].value;
			}

			bool markedRelevant = false;

			// choose style
			GUIStyle style = buttonStyle;
			if (markedEvent == eventData.id)
			{
				style = markedButtonOriginStyle;
				markedRelevant = true;
			}
			else if (markedActor == eventData.actorId ||
				(markedReaction > -1 && eventReactionDict.ContainsKey(eventData.id) &&
				eventReactionDict[eventData.id].Contains(markedReaction)))
			{
				style = markedButtonChildStyle;
				markedRelevant = true;
			}

			// draw button
			if ((showOnlyRelevant && markedRelevant || !showOnlyRelevant) && GUILayout.Button(buttonLabel, style, GUILayout.Height(50)))
			{
				if (markedEvent != eventData.id)
					MarkEvent(eventData);
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

		for (int i = 0; i < ruleGenerator.ReactionData.Count; i++)
		{
			BaseRuleElement.ReactionData rData = ruleGenerator.ReactionData[i];

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
