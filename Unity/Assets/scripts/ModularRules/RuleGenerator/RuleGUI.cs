using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;


public class RuleGUI : MonoBehaviour
{
//	public RuleGenerator ruleGenerator;

	List<BaseRuleElement.ActorData> actorData;
	List<BaseRuleElement.EventData> eventData;
	List<BaseRuleElement.ReactionData> reactionData;

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
		GetComponent<RuleGenerator>().OnParsedRules += OnParsedRules;

		markedButtonOriginStyle = CustomSkin.GetStyle("markedButtonOrigin");
		markedButtonChildStyle = CustomSkin.GetStyle("markedButtonChild");
		buttonStyle = CustomSkin.GetStyle("button");
		areaBackgroundStyle = CustomSkin.GetStyle("areaBackgroundStyle");
		labelSmallStyle = CustomSkin.GetStyle("labelSmallStyle");
	}

	void OnParsedRules(List<BaseRuleElement.ActorData> actorData, List<BaseRuleElement.EventData> eventData, List<BaseRuleElement.ReactionData> reactionData)
	{
		// deep copy lists

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

		//else if (GUI.Button(new Rect(0, Screen.height - 30, 50, 30), "Save"))
		//{

		//}
		else if (GUI.Button(new Rect(0, Screen.height - 30, 50, 30), "Done"))
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

		foreach(BaseRuleElement.EventData e in ruleGenerator.EventData.FindAll(item => item.actorId == currentActor.id))
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

		foreach (BaseRuleElement.ReactionData r in ruleGenerator.ReactionData.FindAll(item => item.actorId == currentActor.id))
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
			ShowParameter("Id:", currentActor.id);
			ShowParameter("Type:", currentActor.type);
			GUI.enabled = true;

			ShowParameter("Label: ", currentActor.label);

			HorizontalLine();

			ShowParameters(currentActor.parameters);
		}
		else if (markedEvent > -1)
		{
			BaseRuleElement.EventData e = ruleGenerator.EventData.Find(item => item.id == markedEvent);
			// extra parameters:
			GUI.enabled = false;
			ShowParameter("Id:", markedEvent);
			ShowParameter("Type:", e.type);
			GUI.enabled = true;

			ShowParameter("Label: ", e.label);

			HorizontalLine();

			ShowParameters(e.parameters);
		}
		else if (markedReaction > -1)
		{
			BaseRuleElement.ReactionData r = ruleGenerator.ReactionData.Find(item => item.id == markedReaction);
			// extra parameters:
			GUI.enabled = false;
			ShowParameter("Id:", markedReaction);
			ShowParameter("Type:", r.type);
			GUI.enabled = true;

			ShowParameter("Label: ", r.label);

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
			ShowParameter(parameters[i].name, parameters[i].value);
		}
	}

	void ShowParameter(string label, object value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label + ": ", labelSmallStyle);
		GUILayout.TextField(value.ToString(), GUILayout.Width(100));
		GUILayout.EndHorizontal();
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
