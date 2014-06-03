using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Globalization;


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
	GUIStyle selectionGridStyle;
	GUIStyle popupWindowStyle;

	bool editMode = false;
	bool loadMode = false;
	bool showDetails = false;
	bool showRulesSaveDialogue = false;
	bool showAlert = false;

	BaseRuleElement.ActorData currentActor;

	int markedActor = -1;
	int markedEvent = -1;
	int markedReaction = -1;

	bool showOnlyRelevant = true;

	string saveRulesFilename = "New Rules";
	bool saveFileExists;

	const int ruleFileSavingDialogId = 0;
	const int alertId = 1;

	string alertTitle;
	string alertText;
	delegate void AlertCallback();
	AlertCallback alertOkCallback;
	AlertCallback alertCancelCallback;

	Dictionary<int, List<int>> eventReactionDict = new Dictionary<int,List<int>>();
	Dictionary<int, List<int>> actorEventDict = new Dictionary<int, List<int>>();
	Dictionary<int, List<int>> actorReactionDict = new Dictionary<int, List<int>>();

	List<BaseRuleElement.ActorData> actorData = new List<BaseRuleElement.ActorData>();
	List<BaseRuleElement.EventData> eventData = new List<BaseRuleElement.EventData>();
	List<BaseRuleElement.ReactionData> reactionData = new List<BaseRuleElement.ReactionData>();

	Dictionary<string, string> vectorParamTemporaries = new Dictionary<string, string>();

	void Awake()
	{
		GetComponent<RuleGenerator>().OnParsedRules += OnParsedRules;

		markedButtonOriginStyle = CustomSkin.GetStyle("markedButtonOrigin");
		markedButtonChildStyle = CustomSkin.GetStyle("markedButtonChild");
		buttonStyle = CustomSkin.GetStyle("button");
		areaBackgroundStyle = CustomSkin.GetStyle("areaBackgroundStyle");
		labelSmallStyle = CustomSkin.GetStyle("labelSmallStyle");
		selectionGridStyle = CustomSkin.GetStyle("selectionGridStyle");
		popupWindowStyle = CustomSkin.GetStyle("popupWindowStyle");
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
		GUI.enabled = true;

		if (showAlert)
		{
			GUILayout.Window(alertId, 
				new Rect(Screen.width * 0.3f, Screen.height * 0.4f, Screen.width * .4f, Screen.height * .2f), 
				AlertWindowFunc, 
				alertTitle, 
				popupWindowStyle);
		}

		if (showAlert || showRulesSaveDialogue)
			GUI.enabled = false;

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

	#region Alert Popup
	void ShowAlertWindow(string title, string text, AlertCallback okCallback, AlertCallback cancelCallback)
	{
		showAlert = true;
		alertText = text;
		alertOkCallback = okCallback;
		alertCancelCallback = cancelCallback;
	}

	void AlertWindowFunc(int windowId)
	{
		GUI.enabled = true;
		GUILayout.BeginVertical();

		GUILayout.Label(alertText, labelSmallStyle);

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Cancel", GUILayout.Width(100)))
		{
			if (alertCancelCallback != null)
				alertCancelCallback();
			alertText = "";
			alertOkCallback = null;
			alertCancelCallback = null;
			showAlert = false;
		}
		else if (GUILayout.Button("OK", GUILayout.Width(100)))
		{
			if (alertOkCallback != null)
				alertOkCallback();

			alertText = "";
			alertOkCallback = null;
			alertCancelCallback = null;
			showAlert = false;
		}

		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
		GUI.enabled = false;
	}
	#endregion

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

		else if (GUI.Button(new Rect(0, Screen.height - 60, 50, 30), "Save"))
		{
			// show pop-up asking for name. if same name as existing ruleset - overwrite
			showRulesSaveDialogue = true;
		}
		else if (GUI.Button(new Rect(0, Screen.height - 30, 50, 30), "Quit without saving"))
		{
			editMode = false;
			currentActor = null;
			showDetails = false;
			GetComponent<RuleGenerator>().StartEventExecution();
		}

		//Version1GUI();

		LowlevelRuleGUI();

		//HighlevelRuleGUI();

		if (showRulesSaveDialogue)
		{
			GUILayout.Window(ruleFileSavingDialogId,
				new Rect(Screen.width * 0.3f, Screen.height * 0.1f, Screen.width * 0.4f, Screen.height * 0.2f),
				SaveRulesDialogue,
				"Save Rules",
				popupWindowStyle);
		}
	}

	Vector2 rulesScrollPos = Vector2.zero;
	void HighlevelRuleGUI()
	{
		rulesScrollPos = GUILayout.BeginScrollView(rulesScrollPos);

		foreach (BaseRuleElement.EventData eData in eventData)
		{
			GUILayout.Space(10);
			GUILayout.BeginHorizontal(areaBackgroundStyle);

			string intro = eData.guiPrefix + " " + eData.type.ToString();//eData.guiName;
			GUILayout.Label(intro, labelSmallStyle);

			for (int i = 0; i < eData.guiParams.Count; i++)
			{
				eData.guiParams[i] = ShowParameterValue(eData.guiParams[i]);
			}

			BaseRuleElement.ReactionData r;
			if (eventReactionDict.ContainsKey(eData.id))
			{
				List<int> rIds = eventReactionDict[eData.id];

				if (rIds.Count > 0)
				{
					r = reactionData.Find(item => item.id == rIds[0]);
					ShowReactionData(r);
				}

				// TODO
				//if (rIds.Count > 1)
				//{
				//}
			}


			GUILayout.EndHorizontal();
		}

		GUILayout.EndScrollView();
	}

	void ShowReactionData(BaseRuleElement.ReactionData rData)
	{
		string intro = rData.guiPrefix + " " + rData.type.ToString();//rData.guiName;
		GUILayout.Label(intro, labelSmallStyle);

		for (int i = 0; i < rData.guiParams.Count; i++)
		{
			//GUILayout.Label(rData.guiParams[i].guiPrefix, labelSmallStyle);
			rData.guiParams[i] = ShowParameterValue(rData.guiParams[i]);
			//GUILayout.Label(rData.guiParams[i].guiPostfix, labelSmallStyle);
		}
	}

	void Version1GUI()
	{
		MainView();
		ActorView();
	}

	void LowlevelRuleGUI()
	{
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

	void SaveRulesDialogue(int windowId)
	{
		if (showAlert) GUI.enabled = false;

		GUILayout.BeginVertical();
		// choose name

		GUILayout.Label("Choose a name for the ruleset: ");

		saveRulesFilename = GUILayout.TextField(saveRulesFilename, GUILayout.Width(100));

		saveRulesFilename = saveRulesFilename.Replace(" ", "");
		saveRulesFilename = saveRulesFilename.Replace(".", "");
		saveRulesFilename = saveRulesFilename.Replace(",", "");
		saveRulesFilename = saveRulesFilename.Replace(":", "");
		saveRulesFilename = saveRulesFilename.Replace(";", "");
		saveRulesFilename = saveRulesFilename.Replace("<", "");
		saveRulesFilename = saveRulesFilename.Replace(">", "");
		saveRulesFilename = saveRulesFilename.Replace("|", "");
		saveRulesFilename = saveRulesFilename.Replace("!", "");
		saveRulesFilename = saveRulesFilename.Replace("\\", "");
		saveRulesFilename = saveRulesFilename.Replace("/", "");
		saveRulesFilename = saveRulesFilename.Replace("\"", "");
		saveRulesFilename = saveRulesFilename.Replace("'", "");
		saveRulesFilename = saveRulesFilename.Replace("{", "");
		saveRulesFilename = saveRulesFilename.Replace("}", "");
		saveRulesFilename = saveRulesFilename.Replace("[", "");
		saveRulesFilename = saveRulesFilename.Replace("]", "");
		saveRulesFilename = saveRulesFilename.Replace("=", "");
		saveRulesFilename = saveRulesFilename.Replace("(", "");
		saveRulesFilename = saveRulesFilename.Replace(")", "");
		saveRulesFilename = saveRulesFilename.Replace("*", "");
		saveRulesFilename = saveRulesFilename.Replace("&", "");
		saveRulesFilename = saveRulesFilename.Replace("^", "");
		saveRulesFilename = saveRulesFilename.Replace("%", "");
		saveRulesFilename = saveRulesFilename.Replace("#", "");
		saveRulesFilename = saveRulesFilename.Replace("$", "");
		saveRulesFilename = saveRulesFilename.Replace("@", "");
		saveRulesFilename = saveRulesFilename.Replace("?", "");
		saveRulesFilename = saveRulesFilename.Replace("`", "");
		saveRulesFilename = saveRulesFilename.Replace("~", "");
		saveRulesFilename = saveRulesFilename.Replace("+", "");
		saveRulesFilename = saveRulesFilename.Replace("=", "");

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Save", GUILayout.Width(100)))
		{
			string filepath = Application.dataPath + @"/Rules/" + saveRulesFilename + ".xml";

			// if same name as other ruleset - ask whether to overwrite or not
			if (File.Exists(filepath))
			{
				saveFileExists = true;
				ShowAlertWindow("Overwriting..", "Overwrite existing file '" + saveRulesFilename + ".xml'?", SaveRulesCallback, null);
			}
			else
			// just save it
			{
				saveFileExists = false;
				ShowAlertWindow("Saving..", "Save rules to '" + saveRulesFilename + ".xml'?", SaveRulesCallback, null);
			}
		}

		// cancel
		if (GUILayout.Button("Cancel", GUILayout.Width(100)))
		{
			showRulesSaveDialogue = false;
			saveRulesFilename = "New Rules";
		}

		GUILayout.EndHorizontal();

		GUILayout.EndVertical();

		if (showAlert) GUI.enabled = true;
	}

	void SaveRulesCallback()
	{
		List<BaseRuleElement.RuleData> rules = new List<BaseRuleElement.RuleData>();
		rules.AddRange(actorData.ToArray());
		rules.AddRange(eventData.ToArray());
		rules.AddRange(reactionData.ToArray());
		RuleGenerator g = GetComponent<RuleGenerator>();
		g.SaveRules(saveRulesFilename, true, rules);
		g.LoadRules(saveRulesFilename);

		showRulesSaveDialogue = showDetails = showAlert = editMode = false;
		markedActor = markedEvent = markedReaction = -1;

		GUI.enabled = true;
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

			vectorParamTemporaries.Clear();
			markedActor = markedEvent = markedReaction = -1;
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
			GUILayout.Space(10);
			GUILayout.BeginHorizontal(GUILayout.Width(Screen.width * 0.3f));
			GUILayout.Label(parameters[i].name + ": ", labelSmallStyle, GUILayout.Width(150));
			parameters[i] = ShowParameterValue(parameters[i]);
			GUILayout.EndHorizontal();
		}
	}

	BaseRuleElement.Param ShowParameterValue(BaseRuleElement.Param parameter)
	{
		// handle parameter value according to type

		// ACTORS
		if (parameter.type.IsSubclassOf(typeof(Actor)) || parameter.type.IsAssignableFrom(typeof(Actor)))
		{
			// get names
			string[] names = new string[actorData.Count + 1];
			for (int i = 0; i < actorData.Count; i++)
			{
				names[i] = actorData[i].label;
			}
			names[names.Length - 1] = "None"; // add option to have no actor selected

			// show selection grid
			int selected = GUILayout.SelectionGrid((int)parameter.value, names, 2, selectionGridStyle);

			// feed back selected actor
			if (selected == names.Length - 1) // chose "None"
				parameter.value = -1;
			else if (selected < names.Length - 1 && selected >= 0)
				parameter.value = actorData.Find(item => item.label == names[selected]).id;
		}
		// ENUMS
		else if (parameter.type.IsEnum)
		{
			string[] names = Enum.GetNames(parameter.type);
			int selected = GUILayout.SelectionGrid((int)parameter.value, names, 2, selectionGridStyle);
			if (selected < names.Length) // when invisible, this gets some weeeird values
				parameter.value = Enum.Parse(parameter.type, names[selected]);
		}
		// INT
		else if (parameter.value is int)
		{
			string v = GUILayout.TextField(parameter.value.ToString(), GUILayout.Width(100));
			if (v == "")
				parameter.value = 0;
			else
				parameter.value = int.Parse(v);
		}
		// FLOAT
		else if (parameter.value is float)
		{
			string v = GUILayout.TextField(parameter.value.ToString(), GUILayout.Width(100));
			if (v == "")
				parameter.value = 0;
			else
				parameter.value = float.Parse(v);
		}
		// VECTOR3
		else if (parameter.type == typeof(Vector3))
		{
			string show;
			if (vectorParamTemporaries.ContainsKey(parameter.name))
			{
				show = vectorParamTemporaries[parameter.name];
			}
			else
			{
				Vector3 vec = (Vector3)parameter.value;
				show = vec.x + " " + vec.y + " " + vec.z;
				vectorParamTemporaries.Add(parameter.name, show);
			}

			vectorParamTemporaries[parameter.name] = GUILayout.TextField(show);

			if (GUILayout.Button("Set"))
			{
				string[] s = vectorParamTemporaries[parameter.name].Split(' ');
				float parseresult;
				Vector3 vec = (Vector3)parameter.value;
				int i;
				for (i = 0; i < s.Length && i < 3; i++)
				{
					if (float.TryParse(s[i], out parseresult))
					{
						vec[i] = parseresult;
					}
				}
				// handling any number of spaces in string
				for (; i < 3; i++)
				{
					vec[i] = 0;
				}
				vectorParamTemporaries.Remove(parameter.name);
				parameter.value = vec;
			}
		}
		// BOOL
		else if (parameter.value is bool)
		{
			parameter.value = GUILayout.Toggle((bool)parameter.value, "");
		}
		// STRING
		else if (parameter.type == typeof(string))
		{
			parameter.value = GUILayout.TextField(parameter.value.ToString(), GUILayout.Width(100));
		}
		// LIST<STRING>
		else if (parameter.type == typeof(List<string>))
		{
			List<string> values = parameter.value as List<string>;
			List<int> delete = new List<int>();
			GUILayout.BeginVertical();
			for (int i = 0; i < values.Count; i++)
			{
				GUILayout.BeginHorizontal();
				values[i] = GUILayout.TextField(values[i]);
				
				if (GUILayout.Button("Del"))
				{
					delete.Add(i);
				}

				GUILayout.EndHorizontal();
			}

			if (GUILayout.Button("Add"))
			{
				values.Add("");
			}
			GUILayout.EndVertical();

			for (int i = 0; i < delete.Count; i++)
			{
				values.RemoveAt(delete[i]);
			}

			parameter.value = values;
		}

		return parameter;
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
