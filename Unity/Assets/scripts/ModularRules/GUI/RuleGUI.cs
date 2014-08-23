using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Reflection;


public class RuleGUI : MonoBehaviour
{
//	public RuleGenerator ruleGenerator;

	private enum RuleGUIState { BEGIN, INGAME, RULES, SAVE, LOAD, ALERT, ADDRULE_1, ADDRULE_2, ADDACTOR };

	public delegate void ActorChanged(string[] newContent, BaseRuleElement.ActorData actor);
	public delegate void ActorRenamed(string[] newContent, BaseRuleElement.ActorData actor, string oldName);

	public ActorChanged OnDeletedActor;
	public ActorChanged OnAddedActor;
	public ActorRenamed OnRenamedActor;

	public string[] ActorNames
	{
		get
		{
			return actorData.ConvertAll(item => item.label).ToArray();
		}
	}

	public string[] Files;

	public GUISkin CustomSkin;

	public static GUIStyle markedButtonOriginStyle;
	public static GUIStyle markedButtonChildStyle;
	public static GUIStyle buttonStyle;
	public static GUIStyle areaBackgroundStyle;
	public static GUIStyle smallLabelStyle;
	public static GUIStyle selectionGridStyle;
	public static GUIStyle popupWindowStyle;

	public static GUIStyle ruleLabelStyle;
	public static GUIStyle ruleEditableStyle;
	public static GUIStyle ruleToggleStyle;
	public static GUIStyle ruleReactionStyle;

	public static GUIStyle ruleIconEditStyle;
	public static GUIStyle ruleIconDelStyle;
	public static GUIStyle ruleIconAddStyle;

	private bool isTesting = false;

	private RuleGUIState LastGuiState { get; set; }
	private RuleGUIState guiState = RuleGUIState.BEGIN;
	private RuleGUIState GuiState
	{
		set
		{
			LastGuiState = guiState;
			guiState = value;
		}
		get
		{
			return guiState;
		}
	}

	string testerCode = "";
	float lastNameChangeTimestamp = 0;

	string saveRulesFilename = "New Rules";
	string fileToDelete = "";

	const int ruleFileSavingDialogId = 0;
	const int alertId = 1;
	const int addRuleId = 2;

	string alertTitle;
	string alertText;
	public delegate void AlertCallback();
	AlertCallback alertOkCallback;
	AlertCallback alertCancelCallback;

	private struct Message { public string text; public float timeout; }
	List<Message> messages = new List<Message>();

	Dictionary<int, List<int>> eventReactionDict = new Dictionary<int,List<int>>();
	Dictionary<int, List<int>> actorEventDict = new Dictionary<int, List<int>>();
	Dictionary<int, List<int>> actorReactionDict = new Dictionary<int, List<int>>();

	List<BaseRuleElement.ActorData> actorData = new List<BaseRuleElement.ActorData>();
	List<BaseRuleElement.EventData> eventData = new List<BaseRuleElement.EventData>();
	List<BaseRuleElement.ReactionData> reactionData = new List<BaseRuleElement.ReactionData>();

	static Dictionary<string, string> vectorParamTemporaries = new Dictionary<string, string>();

	BaseRuleElement.EventData ruleToDelete;
	BaseRuleElement.ActorData actorToDelete;

	int addRule_selectedEventType = 0;
	int addRule_selectedEventId = -1;
	List<int> addRule_selectedReactionTypes = new List<int>();
	int currentSelectedReactionTypeIndex = 0;
	List<Type> eventTypes = new List<Type>();
	List<Type> reactionTypes = new List<Type>();
	string[] eventNames;
	string[] reactionNames;
	BaseRuleElement.EventData addRule_EventData;
	BaseRuleElement.ReactionData addRule_ReactionData;

	void Awake()
	{
		GetComponent<RuleGenerator>().OnGeneratedLevel += OnGeneratedLevel;
		OnRenamedActor += delegate(string[] newContent, BaseRuleElement.ActorData actor, string oldName)
		{
			if (lastNameChangeTimestamp - Time.time < 5)
			{
				Analytics.LogEvent(Analytics.ruleEvent, Analytics.change_name, oldName + " > " + actor.label);
			}
			lastNameChangeTimestamp = Time.time;
		};

		markedButtonOriginStyle = CustomSkin.GetStyle("markedButtonOrigin");
		markedButtonChildStyle = CustomSkin.GetStyle("markedButtonChild");
		buttonStyle = CustomSkin.GetStyle("button");
		areaBackgroundStyle = CustomSkin.GetStyle("areaBackgroundStyle");
		smallLabelStyle = CustomSkin.GetStyle("labelSmallStyle");
		selectionGridStyle = CustomSkin.GetStyle("selectionGridStyle");
		popupWindowStyle = CustomSkin.GetStyle("popupWindowStyle");

		ruleLabelStyle = CustomSkin.GetStyle("ruleLabelStyle");
		ruleEditableStyle = CustomSkin.GetStyle("ruleEditableStyle");
		ruleEditableStyle.normal.textColor = Color.cyan;
		ruleToggleStyle = CustomSkin.GetStyle("ruleToggleStyle");

		ruleReactionStyle = CustomSkin.GetStyle("ruleReactionStyle");
		ruleIconEditStyle = CustomSkin.GetStyle("editIconStyle");
		ruleIconDelStyle = CustomSkin.GetStyle("deleteIconStyle");
		ruleIconAddStyle = CustomSkin.GetStyle("addIconStyle");
	}

	#region On Generated Level
	void OnGeneratedLevel(
		List<BaseRuleElement.ActorData> originalActorData, 
		List<BaseRuleElement.EventData> originalEventData, 
		List<BaseRuleElement.ReactionData> originalReactionData,
		string filename)
	{
		if (isTesting) return; // dont overwrite testing data with data from file

		saveRulesFilename = filename;

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

		// sort events
		eventData.Sort(new BaseRuleElement.OrderComparer<BaseRuleElement.EventData>());

		// sort actors
		actorData.Sort(new BaseRuleElement.OrderComparer<BaseRuleElement.ActorData>());

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
	#endregion

	#region OnGUI
	void OnGUI()
	{
		GUI.skin = CustomSkin;
		GUI.enabled = true;

		if (GuiState == RuleGUIState.BEGIN)
		{
			GUILayout.Window(alertId,
				new Rect(Screen.width * 0.3f, Screen.height * 0.4f, Screen.width * .4f, Screen.height * .2f),
				EnterPersonalCodeWindow,
				"Begin Test",
				popupWindowStyle);
		}
		else if (GuiState == RuleGUIState.ALERT)
		{
			GUILayout.Window(alertId, 
				new Rect(Screen.width * 0.3f, Screen.height * 0.4f, Screen.width * .4f, Screen.height * .2f), 
				AlertWindowFunc, 
				alertTitle, 
				popupWindowStyle);
		}

		if (GuiState == RuleGUIState.RULES)
		{
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height), areaBackgroundStyle);
		}
		else
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));

		GUILayout.BeginHorizontal();

		if (GuiState == RuleGUIState.INGAME)
		{
			GUILayout.FlexibleSpace();
			Counter.ShowCountersInGame();
			ShowMessages();
		}

		EditRules();

		LoadRules();

		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (Analytics.Running && Analytics.LastTask + 1 < 4)
		{
			if (Analytics.CurrentTask == 0 && GUILayout.Button("Start Task " + (Analytics.LastTask + 1), GUILayout.Height(50), GUILayout.Width(100)) )
			{
				Analytics.StartTask(Analytics.LastTask + 1);
			}
			else if (Analytics.CurrentTask != 0 && GUILayout.Button("End Task " + Analytics.CurrentTask, GUILayout.Height(50), GUILayout.Width(100)))
			{
				Analytics.EndTask();
			}

			GUILayout.Space(5);
		}

		if (Analytics.LastTask + 1 >= 4)
		{
			GUILayout.Label("No more tasks!");
		}

		if (GUILayout.Button("Quit", GUILayout.Height(50), GUILayout.Width(100)))
		{
			Analytics.EndSession();
			Application.Quit();
		}
		GUILayout.EndHorizontal();

		GUILayout.EndArea();
	}
	#endregion

	#region Enter Code
	void EnterPersonalCodeWindow(int windowId)
	{
		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		GUILayout.Label("Please enter the code from your email:", smallLabelStyle);

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		testerCode = ShowParameter(testerCode);

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (testerCode != "" && testerCode.Length == 4 && GUILayout.Button("Start Testing", GUILayout.Height(50)))
		{
			GuiState = RuleGUIState.INGAME;

			Analytics.StartSession(testerCode);

			GetComponent<RuleGenerator>().LoadRules("rules_0");
		}

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
	}

	#endregion

	#region Alert Popup
	public void ShowAlertWindow(string title, string text, AlertCallback okCallback, AlertCallback cancelCallback)
	{
		GuiState = RuleGUIState.ALERT;
		alertTitle = title;
		alertText = text;
		alertOkCallback = okCallback;
		alertCancelCallback = cancelCallback;
	}

	void AlertWindowFunc(int windowId)
	{
		GUI.enabled = true;
		GUILayout.BeginVertical();

		GUILayout.Label(alertText, smallLabelStyle);

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(70)))
		{
			if (alertCancelCallback != null)
				alertCancelCallback();
			else
				GuiState = LastGuiState;
			alertText = "";
			alertOkCallback = null;
			alertCancelCallback = null;
		}

		GUILayout.FlexibleSpace();

		if (GUILayout.Button("OK", GUILayout.Width(100), GUILayout.Height(70)))
		{
			if (alertOkCallback != null)
				alertOkCallback();
			else
				GuiState = LastGuiState;

			alertText = "";
			alertOkCallback = null;
			alertCancelCallback = null;
		}

		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
		GUI.enabled = false;
	}
	#endregion

	#region Ingame Message
	public void ShowMessage(string message, int seconds)
	{
		messages.Add(new Message { text = message, timeout = seconds });
	}

	private void ShowMessages()
	{
		GUILayout.BeginArea(new Rect(Screen.width * 0.3f, Screen.height * 0.2f, Screen.width * 0.7f, Screen.height * 0.7f));

		List<Message> newMessages = new List<Message>();
		for (int i = 0; i < messages.Count; i++)
		{
			Message m = messages[i];
			GUILayout.Label(m.text, ruleLabelStyle);
			m.timeout -= Time.deltaTime;
			if (m.timeout > 0)
			{
				newMessages.Add(m);
			}
		}
		messages.Clear();
		newMessages.ForEach(item => messages.Add(item));

		GUILayout.EndArea();
	}
	#endregion

	#region Load Rules
	void LoadRules()
	{
		if (GuiState == RuleGUIState.INGAME && GUI.Button(new Rect(0, Screen.height - 105, 100, 50), "Re/Load Rules"))
		{
			GuiState = RuleGUIState.LOAD;
		}

		if (GuiState != RuleGUIState.LOAD) return;

		GUILayout.BeginVertical(GUILayout.Width(Screen.width * 0.3f));

		GUILayout.BeginScrollView(Vector2.zero);

		for (int i = 0; i < Files.Length; i++)
		{
			GUILayout.BeginHorizontal(GUILayout.Height(70));
			string file = Path.GetFileNameWithoutExtension(Files[i]);
			if (GUILayout.Button("Load " + file, GUILayout.Height(50), GUILayout.ExpandWidth(true)))
			{
				isTesting = false;
				GetComponent<RuleGenerator>().LoadRules(file);

				GuiState = RuleGUIState.INGAME;
			}
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("", ruleIconDelStyle))
			{
				fileToDelete = file;
				ShowAlertWindow("Delete File?", "Sure you want to delete " + file + "?", deleteRuleFile, cancelDeleteRuleFile);
			}
			GUILayout.FlexibleSpace();
			GUILayout.Space(5);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		GUILayout.EndScrollView();

		if (GUILayout.Button("Cancel", GUILayout.Height(70), GUILayout.Width(100)))
		{
			GuiState = RuleGUIState.INGAME;
		}

		GUILayout.EndVertical();
	}
	#endregion

	#region DeleteFileCallbacks
	void deleteRuleFile()
	{
		if (fileToDelete != "")
		{
			GetComponent<RuleGenerator>().DeleteFile(fileToDelete);
		}
		GuiState = RuleGUIState.INGAME;
	}

	void cancelDeleteRuleFile()
	{
		GuiState = RuleGUIState.INGAME;
		fileToDelete = "";
	}
	#endregion

	#region Edit Rules
	void EditRules()
	{
		RuleGenerator ruleGenerator = GetComponent<RuleGenerator>();

		if (GuiState == RuleGUIState.INGAME && actorData.Count > 0 && 
			GUI.Button(new Rect(0, Screen.height - 50, 100, 50), "Edit Rules"))
		{
			GuiState = RuleGUIState.RULES;
			saveRulesFilename = ruleGenerator.CurrentRuleFileName;
			ruleGenerator.PauseEventExecution();
			Analytics.LogEvent(Analytics.gameEvent, Analytics.edit_rules, "");
		}
		if (GuiState == RuleGUIState.INGAME && actorData.Count > 0 &&
			GUI.Button(new Rect(105, Screen.height - 50, 100, 50), "Respawn Actors"))
		{
			RespawnActors();
		}

		if (GuiState == RuleGUIState.RULES)
		{
			PlayerCamera.SetViewportsToRecalculateOnLoad();

			if (GUI.Button(new Rect(0, Screen.height - 30, 100, 30), "Save Rules"))
			{
				// show pop-up asking for name. if same name as existing ruleset - overwrite
				GuiState = RuleGUIState.SAVE;
			}
			else if (GUI.Button(new Rect(110, Screen.height - 30, 100, 30), "Test Changes"))
			{
				isTesting = true;
				GuiState = RuleGUIState.INGAME;

				// hand over changed data to rulegenerator, but don't save
				ruleGenerator.LoadRules(actorData, eventData, reactionData);

				ruleGenerator.StartEventExecution();

				Analytics.LogEvent(Analytics.gameEvent, Analytics.test_game, "");
			}
			else if (GUI.Button(new Rect(220, Screen.height - 30, 100, 30), "Discard Changes"))
			{
				isTesting = false;

				GuiState = RuleGUIState.INGAME;

				// delete rules, reload old rule file
				ruleGenerator.LoadRules(ruleGenerator.CurrentRuleFileName);

				ruleGenerator.StartEventExecution();

				Analytics.LogEvent(Analytics.gameEvent, Analytics.discard_changes, "");
			}
		}

		Rect addRuleWindowRect = new Rect(Screen.width * 0.1f, Screen.height * 0.1f, Screen.width * 0.8f, Screen.height * 0.5f);

		switch(GuiState)
		{ 
			case RuleGUIState.RULES:
				HighlevelRuleGUI();
				break;
			case RuleGUIState.SAVE:
				GUILayout.Window(ruleFileSavingDialogId,
					new Rect(Screen.width * 0.3f, Screen.height * 0.1f, Screen.width * 0.4f, Screen.height * 0.2f),
					SaveRulesDialogue,
					"Save Rules",
					popupWindowStyle);
				break;
			case RuleGUIState.ADDRULE_1:
				GUILayout.Window(addRuleId,
					addRuleWindowRect,
					AddRule_ChooseEvent,
					"Add Rule - Choose Event",
					popupWindowStyle);
				break;
			case RuleGUIState.ADDRULE_2:
				GUILayout.Window(addRuleId,
					addRuleWindowRect,
					AddRule_ChooseReaction,
					"Add Rule - Choose Reaction",
					popupWindowStyle);
				break;
			case RuleGUIState.ADDACTOR:
				GUILayout.Window(addRuleId,
					addRuleWindowRect,
					AddActor_ChooseActor,
					"Add Actor - Choose Type",
					popupWindowStyle);
				break;

		}
	}

	Vector2 rulesScrollPos = Vector2.zero;
	void HighlevelRuleGUI()
	{
		rulesScrollPos = GUILayout.BeginScrollView(rulesScrollPos, GUILayout.MaxHeight(Screen.height * 0.8f));

		GUILayout.Label("Rules:");

		foreach (BaseRuleElement.EventData eData in eventData)
		{
			GUILayout.Space(10);

			GUILayout.BeginHorizontal(areaBackgroundStyle, GUILayout.ExpandWidth(false));

			// display event
			if (eData.OnShowGui != null)
			{
				//GUILayout.BeginHorizontal(ruleEventStyle);
				eData.OnShowGui(eData);
				//GUILayout.EndHorizontal();
			}

			// show all reactions to this event
			BaseRuleElement.ReactionData r;
			if (eventReactionDict.ContainsKey(eData.id))
			{
				List<int> rIds = eventReactionDict[eData.id];

				GUILayout.BeginVertical(GUILayout.ExpandWidth(false));

				if (rIds.Count > 0)
				{
					for (int i = 0; i < rIds.Count; i++)
					{
						r = reactionData.Find(item => item.id == rIds[i]);

						GUILayout.BeginVertical();
						if (r.OnShowGui != null)
						{
							GUILayout.BeginHorizontal(ruleReactionStyle, GUILayout.ExpandWidth(false));
							r.OnShowGui(r);

							if (GUILayout.Button("", ruleIconDelStyle))
							{
								DeleteReaction(r);
							}
							GUILayout.FlexibleSpace();

							GUILayout.EndHorizontal();
							GUILayout.Space(2);
						}

						GUILayout.EndVertical();
					}
				}

				GUILayout.BeginHorizontal(ruleReactionStyle, GUILayout.ExpandWidth(false));
				if (GUILayout.Button("Add Reaction"))
				{
					LoadEventAndReactionTypes();
					addRule_selectedEventId = eData.id;
					addRule_selectedReactionTypes.Add(0);
					GuiState = RuleGUIState.ADDRULE_2; 
				}
				GUILayout.EndHorizontal();

				GUILayout.EndVertical();
			}
			else
			{
				if (GUILayout.Button("Add Reaction"))
				{
					LoadEventAndReactionTypes();
					addRule_selectedEventId = eData.id;
					addRule_selectedReactionTypes.Add(0);
					GuiState = RuleGUIState.ADDRULE_2;
				}

				GUILayout.FlexibleSpace();
			}

			if (GUILayout.Button("", ruleIconDelStyle))
			{
				// delete confirmation
				ShowAlertWindow("Delete rule?", "Are you sure you want to delete this rule?", DeleteRule, DontDeleteRule);
				ruleToDelete = eData;
			}

			GUILayout.EndHorizontal();
		}

		GUILayout.Space(10);

		GUILayout.BeginHorizontal(areaBackgroundStyle, GUILayout.ExpandWidth(false));
		if (GUILayout.Button("Add Rule", GUILayout.Height(70), GUILayout.Width(100)))
		{
			AddRule();
		}
		GUILayout.EndHorizontal();



		HorizontalLine();

		GUILayout.Label("Actors:");

		foreach (BaseRuleElement.ActorData aData in actorData)
		{
			GUILayout.Space(10);
			GUILayout.BeginHorizontal(areaBackgroundStyle);

			if (aData.OnShowGui != null)
				aData.OnShowGui(aData);

			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("", ruleIconDelStyle))
			{
				ShowAlertWindow("Delete actor?", "Are you sure you want to delete this actor?", DeleteActor, DontDeleteActor);
				actorToDelete = aData;
			}

			GUILayout.EndHorizontal();
		}

		GUILayout.Space(10);
		GUILayout.BeginHorizontal(areaBackgroundStyle, GUILayout.ExpandWidth(false));
		if (GUILayout.Button("Add Actor", GUILayout.Height(70), GUILayout.Width(100)))
		{
			AddActor();
		}
		GUILayout.EndHorizontal();

		GUILayout.EndScrollView();
	}
	#endregion

	#region Respawn
	public void RespawnActors()
	{

		foreach (Actor a in GetComponent<RuleGenerator>().GetGenActors())
			a.Respawn();
	}
	#endregion

	#region Add Rule
	// add new rule
	void AddRule()
	{
		if (GuiState == RuleGUIState.RULES)
			GuiState = RuleGUIState.ADDRULE_1;

		LoadEventAndReactionTypes();
	}

	void LoadEventAndReactionTypes(bool reload = false)
	{
		if ((eventTypes.Count == 0 && reactionTypes.Count == 0) || reload)
		{
			Type[] types = RuleParserLinq.GetTypesInCurrentAssembly();
			foreach (Type t in types)
			{
				bool dontUse = false;
				object[] attributes = t.GetCustomAttributes(true);
				for (int i = 0; i < attributes.Length; i++)
				{
					if (attributes[i] is DontShowInRuleGUIAttribute)
					{
						dontUse = true;
						break;
					}
				}

				if (dontUse) continue;

				if (!eventTypes.Contains(t) && t.IsSubclassOf(typeof(GameEvent)))
				{
					eventTypes.Add(t);
				}
				else if (!reactionTypes.Contains(t) && t.IsSubclassOf(typeof(Reaction)))
				{
					reactionTypes.Add(t);
				}
			}

			eventNames = eventTypes.ConvertAll(item => item.Name).ToArray();
			reactionNames = reactionTypes.ConvertAll(item => item.Name).ToArray();
		}
	}

	// choose event
	void AddRule_ChooseEvent(int windowId) 
	{
		GUILayout.Label("Choose Event");

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		
		addRule_selectedEventType = GUILayout.SelectionGrid(addRule_selectedEventType, 
			eventNames, 3, selectionGridStyle);

		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Cancel", GUILayout.Height(70), GUILayout.Width(100)))
		{
			GuiState = RuleGUIState.RULES;
			currentSelectedReactionTypeIndex = 0;
			addRule_selectedReactionTypes.Clear();
			addRule_selectedEventType = 0;
			addRule_selectedEventId = -1;
		}

		GUILayout.FlexibleSpace();

		if (GUILayout.Button("Next", GUILayout.Height(70), GUILayout.Width(100)))
		{
			GuiState = RuleGUIState.ADDRULE_2;
			if (addRule_selectedReactionTypes.Count == 0)
				addRule_selectedReactionTypes.Add(0);
		}

		GUILayout.EndHorizontal();
	}

	void AddRule_ChooseReaction(int windowId)
	{
		GUILayout.Label("Choose Reaction " + (currentSelectedReactionTypeIndex + 1));

		GUILayout.FlexibleSpace();
	
		GUILayout.BeginHorizontal();

		// if adding to an existing event
		if (addRule_selectedEventId > -1 && addRule_selectedEventId < eventData.Count)
		{
			// get index of event type name, set the right event type int
			addRule_selectedEventType = eventTypes.FindIndex(item => item == eventData[addRule_selectedEventId].type);
		}

		int curSelReactionIndex = currentSelectedReactionTypeIndex;
		addRule_selectedReactionTypes[curSelReactionIndex] = GUILayout.SelectionGrid(addRule_selectedReactionTypes[curSelReactionIndex],
			reactionNames, 3, selectionGridStyle);

		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Cancel", GUILayout.Height(70), GUILayout.Width(100)))
		{
			GuiState = RuleGUIState.RULES;
			currentSelectedReactionTypeIndex = 0;
			addRule_selectedReactionTypes.Clear();
			addRule_selectedEventType = 0;
			addRule_selectedEventId = -1;
		}
		
		GUILayout.Space(10);

		if ((addRule_selectedEventId == -1 || currentSelectedReactionTypeIndex > 0) && GUILayout.Button("Back", GUILayout.Width(100), GUILayout.Height(70)))
		{
			if (currentSelectedReactionTypeIndex > 0) currentSelectedReactionTypeIndex--;
			else GuiState = RuleGUIState.ADDRULE_1;
		}

		GUILayout.FlexibleSpace();

		if (addRule_selectedEventId > -1 && currentSelectedReactionTypeIndex < addRule_selectedReactionTypes.Count - 1)
		{
			if (GUILayout.Button("Next", GUILayout.Height(70), GUILayout.Width(100)))
				currentSelectedReactionTypeIndex++;
		}
		else if (GUILayout.Button("Add another reaction", GUILayout.Height(70)))
		{
			currentSelectedReactionTypeIndex++;
			addRule_selectedReactionTypes.Add(0);
		}

		GUILayout.Space(10);

		string finishButtonText = "Add " + (addRule_selectedReactionTypes.Count) + " reaction(s)";
		if (GUILayout.Button(finishButtonText, GUILayout.Width(100), GUILayout.Height(70)))
		{
			RuleGenerator generator = GetComponent<RuleGenerator>();

			BaseRuleElement.EventData newEvent;
			if (addRule_selectedEventId == -1)
			{
				newEvent = new BaseRuleElement.EventData()
				{
					type = RuleParserLinq.ReflectOverSeveralNamespaces(eventNames[addRule_selectedEventType], RuleParserLinq.ExtraNamespaces),
					id = generator.GetEventId(),
					actorId = -1
				};

				newEvent.label = newEvent.type.ToString();
				newEvent.parameters = new List<BaseRuleElement.Param>();

				eventData.Add(newEvent);

				// sort evetn data
				eventData.Sort(new BaseRuleElement.OrderComparer<BaseRuleElement.EventData>());

				generator.AddEventToScene(newEvent);

				// add to lookup dict
				if (actorEventDict.ContainsKey(newEvent.actorId))
				{
					actorEventDict[newEvent.actorId].Add(newEvent.id);
				}
				else
				{
					actorEventDict.Add(newEvent.actorId, new List<int>() { newEvent.id });
				}

				Analytics.LogEvent(Analytics.ruleEvent, Analytics.add_event, newEvent.label);
			}
			else
			{
				// only adding new reaction, no event
				newEvent = eventData.Find(item => item.id == addRule_selectedEventId);
			}

			// add all new reactions
			for (int i = 0; i < addRule_selectedReactionTypes.Count; i++)
			{
				BaseRuleElement.ReactionData newReaction = new BaseRuleElement.ReactionData()
				{
					type = RuleParserLinq.ReflectOverSeveralNamespaces(reactionNames[addRule_selectedReactionTypes[i]], RuleParserLinq.ExtraNamespaces),
					id = generator.GetReactionId(),
					actorId = 0, // HACK should be a global actor, not just actor 0
					eventId = newEvent.id
				};
				newReaction.label = newReaction.type.ToString();
				newReaction.parameters = new List<BaseRuleElement.Param>();

				reactionData.Add(newReaction);

				generator.AddReactionToScene(newReaction);


				// add to lookup dicts
				if (eventReactionDict.ContainsKey(newReaction.eventId))
				{
					eventReactionDict[newReaction.eventId].Add(newReaction.id);
				}
				else
				{
					eventReactionDict.Add(newReaction.eventId, new List<int>() { newReaction.id });
				}

				if (actorReactionDict.ContainsKey(newReaction.actorId))
				{
					actorReactionDict[newReaction.actorId].Add(newReaction.id);
				}
				else
				{
					actorReactionDict.Add(newReaction.actorId, new List<int>() { newReaction.id });
				}

				Analytics.LogEvent(Analytics.ruleEvent, Analytics.add_reaction, newReaction.label);
			}

			GuiState = RuleGUIState.RULES;
			currentSelectedReactionTypeIndex = 0;
			addRule_selectedReactionTypes.Clear();
			addRule_selectedEventType = 0;
			addRule_selectedEventId = -1;
		}

		GUILayout.EndHorizontal();

	}
	#endregion

	#region AddActor
	void AddActor()
	{
		if (GuiState == RuleGUIState.RULES)
			GuiState = RuleGUIState.ADDACTOR;

		LoadActorTypes(true);
	}

	List<Type> actorTypes = new List<Type>();
	string[] actorNames;
	int addActor_selectedNameIndex = 0;

	void LoadActorTypes(bool reload = false)
	{
		if (actorTypes.Count == 0 || reload)
		{
			Type[] types = RuleParserLinq.GetTypesInCurrentAssembly();
			foreach (Type t in types)
			{
				bool dontUse = false;
				object[] attributes = t.GetCustomAttributes(true);
				for (int i = 0; i < attributes.Length; i++)
				{
					if (attributes[i] is DontShowInRuleGUIAttribute)
					{
						dontUse = true;
						break;
					}
				}

				if (dontUse) continue;

				if (!actorTypes.Contains(t) && t.IsSubclassOf(typeof(Actor)))
				{
					actorTypes.Add(t);
				}
			}

			actorNames = actorTypes.ConvertAll(item => item.Name).ToArray();
		}
	}

	void AddActor_ChooseActor(int windowId)
	{
		GUILayout.Label("Choose Actor Type");

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();

		addActor_selectedNameIndex = GUILayout.SelectionGrid(addActor_selectedNameIndex, actorNames, 2, RuleGUI.selectionGridStyle);

		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Cancel", GUILayout.Height(70), GUILayout.Width(100)))
		{
			GuiState = RuleGUIState.RULES;
			addActor_selectedNameIndex = 0;
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Add", GUILayout.Height(70), GUILayout.Width(100)))
		{
			RuleGenerator generator = GetComponent<RuleGenerator>();

			BaseRuleElement.ActorData newActor = new BaseRuleElement.ActorData()
			{
				type = RuleParserLinq.ReflectOverSeveralNamespaces(actorNames[addActor_selectedNameIndex], RuleParserLinq.ExtraNamespaces),
				prefab = "",
				id = generator.GetActorId()
			};

			newActor.label = newActor.type.ToString();
			newActor.parameters = new List<BaseRuleElement.Param>();

			actorData.Add(newActor);
			generator.AddActorToScene(newActor);
			generator.GetActor(newActor.id).Initialize(generator);

			Analytics.LogEvent(Analytics.ruleEvent, Analytics.add_actor, newActor.label);

			if (OnAddedActor != null)
				OnAddedActor(ActorNames, newActor);

			// sort actor data
			actorData.Sort(new BaseRuleElement.OrderComparer<BaseRuleElement.ActorData>());

			GuiState = RuleGUIState.RULES;
			addActor_selectedNameIndex = 0;
		}

		GUILayout.EndHorizontal();
	}
	#endregion

	#region Rename Actor
	public void Rename(int id, string newLabel)
	{
		BaseRuleElement.ActorData actor = actorData.Find(item => item.id == id);
		string oldLabel = actor.label;
		actor.label = newLabel;

		if (OnRenamedActor != null)
		{
			OnRenamedActor(ActorNames, actor, oldLabel);
		}
	}
	#endregion

	#region Deleting Rules/Actors
	void DeleteRule()
	{
		// delete all reactions

		if (eventReactionDict.ContainsKey(ruleToDelete.id))
		{
			List<int> rIds = eventReactionDict[ruleToDelete.id];

			for (int i = 0; i < rIds.Count; i++)
			{
				reactionData.RemoveAt(reactionData.FindIndex(item => item.id == rIds[i]));
			}
		}

		// delete the event
		eventData.Remove(ruleToDelete);

		GuiState = RuleGUIState.RULES;

		Analytics.LogEvent(Analytics.ruleEvent, Analytics.delete_rule, ruleToDelete.label);
	}

	void DontDeleteRule()
	{
		GuiState = RuleGUIState.RULES;
		ruleToDelete = null;
	}

	void DeleteReaction(BaseRuleElement.ReactionData reaction)
	{
		eventReactionDict[reaction.eventId].Remove(reaction.id);
		actorReactionDict[reaction.actorId].Remove(reaction.id);
		reactionData.Remove(reaction);

		Analytics.LogEvent(Analytics.ruleEvent, Analytics.delete_reaction, reaction.label);
	}

	// only deletes actor from current gui data set
	void DeleteActor()
	{
		// set all actor references to global invisible actor
		if (actorEventDict.ContainsKey(actorToDelete.id))
		{
			foreach (int id in actorEventDict[actorToDelete.id])
			{
				BaseRuleElement.EventData e = eventData.Find(item => item.id == id);
				e.actorId = -1;
			}
			actorEventDict[actorToDelete.id].Clear();
			actorEventDict.Remove(actorToDelete.id);
		}

		if (actorReactionDict.ContainsKey(actorToDelete.id))
		{
			foreach (int id in actorReactionDict[actorToDelete.id])
			{
				BaseRuleElement.ReactionData r = reactionData.Find(item => item.id == id);
				r.actorId = -1;
			}
			actorReactionDict[actorToDelete.id].Clear();
			actorReactionDict.Remove(actorToDelete.id);
		}

		// delete actor from data
		actorData.Remove(actorToDelete);

		GuiState = RuleGUIState.RULES;

		Analytics.LogEvent(Analytics.ruleEvent, Analytics.delete_actor, actorToDelete.label);

		if (OnDeletedActor != null)
			OnDeletedActor(actorData.ConvertAll(item => item.label).ToArray(), actorToDelete);
	}

	void DontDeleteActor()
	{
		GuiState = RuleGUIState.RULES;
		actorToDelete = null;
	}
	#endregion

	#region Save Rules
	void SaveRulesDialogue(int windowId)
	{
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

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();


		// cancel
		if (GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(70)))
		{
			GuiState = RuleGUIState.RULES;
			saveRulesFilename = GetComponent<RuleGenerator>().CurrentRuleFileName;
		}

		GUILayout.FlexibleSpace();

		if (GUILayout.Button("Save", GUILayout.Width(100), GUILayout.Height(70)))
		{
			string filepath = Application.dataPath + @"/Rules/" + saveRulesFilename + ".xml";

			// if same name as other ruleset - ask whether to overwrite or not
			if (File.Exists(filepath))
			{
				GuiState = RuleGUIState.RULES; // HACK to go back to rules view after done
				ShowAlertWindow("Overwriting..", "Overwrite existing file '" + saveRulesFilename + ".xml'?", SaveRulesCallback, null);
			}
			else
			// just save it
			{
				GuiState = RuleGUIState.RULES; // HACK to go back to rules view after done
				ShowAlertWindow("Saving..", "Save rules to '" + saveRulesFilename + ".xml'?", SaveRulesCallback, null);
			}
		}

		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
	}

	void SaveRulesCallback()
	{
		isTesting = false;

		List<BaseRuleElement.RuleData> rules = new List<BaseRuleElement.RuleData>();
		rules.AddRange(actorData.ToArray());
		rules.AddRange(eventData.ToArray());
		rules.AddRange(reactionData.ToArray());
		RuleGenerator g = GetComponent<RuleGenerator>();
		g.SaveRules(saveRulesFilename, true, rules);
		g.LoadRules(saveRulesFilename);

		GuiState = RuleGUIState.INGAME;

		g.StartEventExecution();

		GUI.enabled = true;
	}
	#endregion

	#region GUI Helper
	public static void HorizontalLine(float width = 1, float padding = 5)
	{
		GUILayout.Space(padding);
		GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(width) });
		GUILayout.Space(padding);
	}

	public static void VerticalLine(float width = 1, float padding = 5)
	{
		GUILayout.Space(padding);
		GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandHeight(true), GUILayout.Width(width) });
		GUILayout.Space(padding);
	}
	#endregion

	#region Showing Parameters
	void ShowParameters(List<BaseRuleElement.Param> parameters)
	{
		for (int i = 0; i < parameters.Count; i++)
		{
			GUILayout.Space(10);
			GUILayout.BeginHorizontal(GUILayout.Width(Screen.width * 0.3f));
			GUILayout.Label(parameters[i].name + ": ", smallLabelStyle, GUILayout.Width(150));
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
	
	public static int ShowParameter(int value)
	{
		int result;
		string v = GUILayout.TextField(value.ToString(), ruleEditableStyle);
		if (v == "")
			result = 0;
		else
			result = int.Parse(v);

		return result;
	}

	public static float ShowParameter(float value)
	{
		string v = GUILayout.TextField(value.ToString(), ruleEditableStyle);
		if (v == "")
			value = 0;
		else
			value = float.Parse(v);

		return value;
	}

	public static Vector3 ShowParameter(Vector3 value, string name)
	{
		string show;
		if (vectorParamTemporaries.ContainsKey(name))
		{
			show = vectorParamTemporaries[name];
		}
		else
		{
			Vector3 vec = (Vector3)value;
			show = vec.x + " " + vec.y + " " + vec.z;
			vectorParamTemporaries.Add(name, show);
		}

		GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

		vectorParamTemporaries[name] = GUILayout.TextField(show, ruleEditableStyle, GUILayout.ExpandWidth(false));

		if (GUILayout.Button("Set", GUILayout.Width(30), GUILayout.ExpandWidth(false)))
		{
			string[] s = vectorParamTemporaries[name].Split(' ');
			float parseresult;
			Vector3 vec = (Vector3)value;
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
			vectorParamTemporaries.Remove(name);
			value = vec;
		}

		GUILayout.EndHorizontal();

		return value;
	}

	public static bool ShowParameter(bool value)
	{
		return GUILayout.Toggle(value, "", ruleToggleStyle);
	}

	public static string ShowParameter(string value)
	{
		if (value == "" || value == null)
			return GUILayout.TextField("", GUILayout.Width(50));
		return GUILayout.TextField(value, ruleEditableStyle);
	}

	public static List<string> ShowParameter(List<string> value)
	{
		List<int> delete = new List<int>();
		GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
		for (int i = 0; i < value.Count; i++)
		{
			value[i] = ShowParameter(value[i]);

			if (GUILayout.Button("", ruleIconDelStyle))
			{
				delete.Add(i);
			}

			if (i < value.Count - 1)
				GUILayout.Label(",", ruleLabelStyle);
		}

		GUILayout.Space(10);

		if (GUILayout.Button("Add"))
		{
			value.Add("");
		}
		GUILayout.EndHorizontal();

		for (int i = 0; i < delete.Count; i++)
		{
			value.RemoveAt(delete[i]);
		}

		return value;
	}

	string ShowParameter(string label, string value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label + ": ", smallLabelStyle);
		string result = GUILayout.TextField(value);
		GUILayout.EndHorizontal();

		return result;
	}
#endregion

	#region Data Accessors
	public BaseRuleElement.ActorData GetActorDataById(int id)
	{
		return actorData.Find(item => item.id == id);
	}

	public BaseRuleElement.ActorData GetActorDataByLabel(string label)
	{
		return actorData.Find(item => item.label == label);
	}
	#endregion
}
