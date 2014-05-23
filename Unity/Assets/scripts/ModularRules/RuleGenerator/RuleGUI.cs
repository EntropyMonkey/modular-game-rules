using UnityEngine;
using System.Collections;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using ModularRules;

//namespace ModularRules
//{
	public class RuleGUI : MonoBehaviour
	{
		[HideInInspector]
		public RuleGenerator ruleGenerator;

		public bool ShowButtons = true;

		public GUIStyle GuiStyle;
		
		bool editMode = false;
		bool loadMode = false;

		void Awake()
		{
			ruleGenerator = GetComponent<RuleGenerator>();
		}

		void OnGUI()
		{
			string filepath = Application.dataPath + @"/Rules/";
			string[] files = Directory.GetFiles(filepath, "rules_*.xml");

			if (editMode)
			{
				if (GUI.Button(new Rect(0, 50, 200, 50), "Save Rules As New", GuiStyle))
				{
					ruleGenerator.SaveRules(ruleGenerator.CurrentRuleFileName, false);
				}

				if (GUI.Button(new Rect(0, 100, 200, 50), "Save Rules As Current", GuiStyle))
				{
					ruleGenerator.SaveRules(ruleGenerator.CurrentRuleFileName, true);
				}

#if UNITY_EDITOR
				if (GUI.Button(new Rect(0, 200, 200, 50), "Add selected actor", GuiStyle))
				{
					Actor selected = Selection.activeTransform.gameObject.GetComponent(typeof(Actor)) as Actor;
					if (selected)
					{
						selected.Initialize(ruleGenerator);
						selected.InitializeReactions();
						selected.InitializeEvents();
					}
				}
#endif

				//if (GUI.Button(new Rect()))
			}

			if (loadMode)
			{
				int x = 0; int width = 150;
				for (int i = 0; i < files.Length; i++)
				{
					string file = Path.GetFileNameWithoutExtension(files[i]);
					if (GUI.Button(new Rect(x, 0, width, 50), "Load " + file))
					{
						ruleGenerator.LoadRules(file);
						loadMode = false;
					}
					x += width;
				}
			}

			if (GuiStyle != null && ShowButtons && !editMode && !loadMode && GUI.Button(new Rect(0, 0, 100, 50), "Edit Rules", GuiStyle))
			{
				editMode = true;
			}

			if (GuiStyle != null && ShowButtons && !loadMode && !editMode && GUI.Button(new Rect(0, 50, 100, 50), "Load Rules", GuiStyle))
			{
				loadMode = true;
			}
		}
	}
//}
