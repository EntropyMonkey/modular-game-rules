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

	private enum RuleGUIState { NORMAL, SAVE, ALERT, ADDRULE_1, ADDRULE_2, ADDACTOR };

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

	public bool ShowButtons = true;

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
	public static GUIStyle ruleReactionStyle;

	public static GUIStyle ruleIconEditStyle;
	public static GUIStyle ruleIconDelStyle;
	public static GUIStyle ruleIconAddStyle;

	bool editMode = false;
	bool loadMode = false;
	bool isTesting = false;

	private RuleGUIState guiState;

	//bool showOnlyRelevant = true;

	string saveRulesFilename = "New Rules";

	const int ruleFileSavingDialogId = 0;
	const int alertId = 1;
	const int addRuleId = 2;

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

		if (guiState == RuleGUIState.ALERT)
		{
			GUILayout.Window(alertId, 
				new Rect(Screen.width * 0.3f, Screen.height * 0.4f, Screen.width * .4f, Screen.height * .2f), 
				AlertWindowFunc, 
				alertTitle, 
				popupWindowStyle);
		}

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
	#endregion

	#region Alert Popup
	void ShowAlertWindow(string title, string text, AlertCallback okCallback, AlertCallback cancelCallback)
	{
		guiState = RuleGUIState.ALERT;
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
			alertText = "";
			alertOkCallback = null;
			alertCancelCallback = null;
			guiState = RuleGUIState.NORMAL;
		}

		GUILayout.FlexibleSpace();

		if (GUILayout.Button("OK", GUILayout.Width(100), GUILayout.Height(70)))
		{
			if (alertOkCallback != null)
				alertOkCallback();

			alertText = "";
			alertOkCallback = null;
			alertCancelCallback = null;
			guiState = RuleGUIState.NORMAL;
		}

		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
		GUI.enabled = false;
	}
	#endregion

	#region Load Rules
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
			if (GUILayout.Button("Load " + file, GUILayout.Height(70), GUILayout.ExpandWidth(true)))
			{
				isTesting = false;
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
	#endregion

	#region Edit Rules
	void EditRules()
	{
		RuleGenerator ruleGenerator = GetComponent<RuleGenerator>();
		if (ShowButtons && !editMode && !loadMode && actorData.Count > 0 && 
			GUI.Button(new Rect(0, Screen.height - 50, 100, 50), "Edit Rules"))
		{
			editMode = true;
			saveRulesFilename = ruleGenerator.CurrentRuleFileName;
			ruleGenerator.PauseEventExecution();
		}

		if (!editMode) return;

		else if (GUI.Button(new Rect(0, Screen.height - 30, 100, 30), "Save Rules"))
		{
			// show pop-up asking for name. if same name as existing ruleset - overwrite
			guiState = RuleGUIState.SAVE;
		}
		else if (GUI.Button(new Rect(110, Screen.height - 30, 100, 30), "Test Changes"))
		{
			isTesting = true;

			// hand over changed data to rulegenerator, but don't save
			ruleGenerator.LoadRules(actorData, eventData, reactionData);

			editMode = false;
			ruleGenerator.StartEventExecution();
		}
		else if (GUI.Button(new Rect(220, Screen.height - 30, 100, 30), "Discard Changes"))
		{
			isTesting = false;

			// delete rules, reload old rule file
			ruleGenerator.LoadRules(ruleGenerator.CurrentRuleFileName);

			editMode = false;
			ruleGenerator.StartEventExecution();
		}

		Rect addRuleWindowRect = new Rect(Screen.width * 0.1f, Screen.height * 0.1f, Screen.width * 0.8f, Screen.height * 0.5f);

		switch(guiState)
		{ 
			case RuleGUIState.NORMAL:
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
		rulesScrollPos = GUILayout.BeginScrollView(rulesScrollPos, GUILayout.Height(Screen.height * 0.85f));

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

							GUILayout.FlexibleSpace();

							if (GUILayout.Button("", ruleIconDelStyle))
							{
								eventReactionDict[r.eventId].Remove(r.id);
								actorReactionDict[r.actorId].Remove(r.id);
								reactionData.Remove(r);
							}

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
					guiState = RuleGUIState.ADDRULE_2; 
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
					guiState = RuleGUIState.ADDRULE_2;
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

	#region Add Rule
	// add new rule
	void AddRule()
	{
		if (guiState == RuleGUIState.NORMAL)
			guiState = RuleGUIState.ADDRULE_1;

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
			guiState = RuleGUIState.NORMAL;
			currentSelectedReactionTypeIndex = 0;
			addRule_selectedReactionTypes.Clear();
			addRule_selectedEventType = 0;
			addRule_selectedEventId = -1;
		}

		GUILayout.FlexibleSpace();

		if (GUILayout.Button("Next", GUILayout.Height(70), GUILayout.Width(100)))
		{
			guiState = RuleGUIState.ADDRULE_2;
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
		if (addRule_selectedEventId > -1)
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
			guiState = RuleGUIState.NORMAL;
			currentSelectedReactionTypeIndex = 0;
			addRule_selectedReactionTypes.Clear();
			addRule_selectedEventType = 0;
			addRule_selectedEventId = -1;
		}
		
		GUILayout.Space(10);

		if ((addRule_selectedEventId == -1 || currentSelectedReactionTypeIndex > 0) && GUILayout.Button("Back", GUILayout.Width(100), GUILayout.Height(70)))
		{
			if (currentSelectedReactionTypeIndex > 0) currentSelectedReactionTypeIndex--;
			else guiState = RuleGUIState.ADDRULE_1;
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
			}

			guiState = RuleGUIState.NORMAL;
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
		if (guiState == RuleGUIState.NORMAL)
			guiState = RuleGUIState.ADDACTOR;

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
			guiState = RuleGUIState.NORMAL;
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

			if (OnAddedActor != null)
				OnAddedActor(ActorNames, newActor);

			guiState = RuleGUIState.NORMAL;
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
		List<int> rIds = eventReactionDict[ruleToDelete.id];

		for (int i = 0; i < rIds.Count; i++)
		{
			reactionData.RemoveAt(reactionData.FindIndex(item => item.id == rIds[i]));
		}

		// delete the event
		eventData.Remove(ruleToDelete);
	}

	void DontDeleteRule()
	{
		ruleToDelete = null;
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

		if (OnDeletedActor != null)
			OnDeletedActor(actorData.ConvertAll(item => item.label).ToArray(), actorToDelete);
	}

	void DontDeleteActor()
	{
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
			guiState = RuleGUIState.NORMAL;
			saveRulesFilename = GetComponent<RuleGenerator>().CurrentRuleFileName;
		}

		GUILayout.FlexibleSpace();

		if (GUILayout.Button("Save", GUILayout.Width(100), GUILayout.Height(70)))
		{
			string filepath = Application.dataPath + @"/Rules/" + saveRulesFilename + ".xml";

			// if same name as other ruleset - ask whether to overwrite or not
			if (File.Exists(filepath))
			{
				ShowAlertWindow("Overwriting..", "Overwrite existing file '" + saveRulesFilename + ".xml'?", SaveRulesCallback, null);
			}
			else
			// just save it
			{
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

		editMode = false;

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
		return GUILayout.Toggle(value, "");
	}

	public static string ShowParameter(string value)
	{
		if (value == "")
			return GUILayout.TextField("", GUILayout.Width(50));
		return GUILayout.TextField(value, ruleEditableStyle); ;
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
	public BaseRuleElement.ActorData GetActorById(int id)
	{
		return actorData.Find(item => item.id == id);
	}

	public BaseRuleElement.ActorData GetActorByLabel(string label)
	{
		return actorData.Find(item => item.label == label);
	}
	#endregion
}
