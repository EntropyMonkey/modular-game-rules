using UnityEngine;
using System.Collections;

public class WinLoseGame : Reaction
{

	public GameEnd Ending = GameEnd.WIN;
	public Actor WinningActor = null;
	public Actor LosingActor = null;

	ActorDropDown winningDropDown;
	ActorDropDown losingDropDown;
	DropDown endingDropDown;
	
	bool showOverlay;
	const int overlayId = 0;

	public override void Initialize(RuleGenerator generator)
	{
		base.Initialize(generator);

		generator.OnActorGOChanged += delegate(ActorData data, Actor newActor, RuleGenerator ruleGenerator)
		{
			if (WinningActor != null && WinningActor.Id == data.id)
			{
				WinningActor = newActor;
			}

			if (LosingActor != null && LosingActor.Id == data.id)
			{
				LosingActor = newActor;
			}
		};

		int selected = 0;

		string[] endingNames = System.Enum.GetNames(typeof(GameEnd));
		endingDropDown = new DropDown(0, endingNames);

		if (WinningActor != null)
			selected = System.Array.FindIndex(generator.Gui.ActorNames, item => item == WinningActor.Label);

		winningDropDown = new ActorDropDown(selected, 
			generator.Gui.ActorNames, 
			ref generator.Gui.OnAddedActor, 
			ref generator.Gui.OnRenamedActor, 
			ref generator.Gui.OnDeletedActor);

		selected = 0;
		if (LosingActor != null)
			selected = System.Array.FindIndex(generator.Gui.ActorNames, item => item == LosingActor.Label);

		losingDropDown = new ActorDropDown(selected,
			generator.Gui.ActorNames,
			ref generator.Gui.OnAddedActor,
			ref generator.Gui.OnRenamedActor,
			ref generator.Gui.OnDeletedActor);
	}

	protected override void React(GameEventData eventData)
	{
		// show gui overlay: player blup won! when pressing ok or timeout done, restart game
		showOverlay = true;
	}

	public override RuleData GetRuleInformation()
	{
		ReactionData rule = base.GetRuleInformation() as ReactionData;

		if (rule.parameters == null)
			rule.parameters = new System.Collections.Generic.List<Param>();

		rule.parameters.Add(new Param()
			{
				name = "Ending",
				type = Ending.GetType(),
				value = Ending
			});

		rule.parameters.Add(new Param()
			{
				name = "WinningActor",
				type = WinningActor.GetType(),
				value = WinningActor.Id
			});

		rule.parameters.Add(new Param()
		{
			name = "LosingActor",
			type = LosingActor.GetType(),
			value = LosingActor.Id
		});
		
		return rule;
	}

	public override void ShowGui(RuleData ruleData)
	{
		GUILayout.BeginHorizontal();

		GUILayout.Label("The game ends as a", RuleGUI.ruleLabelStyle);

		// ending dropdown
		int index = endingDropDown.Draw();
		if (index > -1)
		{
			string result = endingDropDown.Content[index].text;
			Ending = (GameEnd)System.Enum.Parse(typeof(GameEnd), result);
		}

		switch(Ending)
		{
			case GameEnd.WIN:
				GUILayout.Label("for", RuleGUI.ruleLabelStyle);

				// winnerdropdown
				int resultIndex = winningDropDown.Draw();
				if (resultIndex > -1)
				{
					int resultId = Reactor.RuleGenerator.Gui.GetActorDataByLabel(winningDropDown.Content[resultIndex].text).id;

					WinningActor = Reactor.RuleGenerator.GetActor(resultId);
					ChangeParameter("WinningActor", ruleData.parameters, resultId);
				}

				break;

			case GameEnd.LOSE:
				GUILayout.Label("for", RuleGUI.ruleLabelStyle);

				// loserdropdown
				resultIndex = losingDropDown.Draw();
				if (resultIndex > -1)
				{
					int resultId = Reactor.RuleGenerator.Gui.GetActorDataByLabel(losingDropDown.Content[resultIndex].text).id;

					LosingActor = Reactor.RuleGenerator.GetActor(resultId);
					ChangeParameter("LosingActor", ruleData.parameters, resultId);
				}
				break;
		}
		GUILayout.EndHorizontal();
	}

	void OnGUI()
	{
		if (showOverlay)
		{
			GUILayout.Window(overlayId, 
				new Rect(Screen.width * 0.3f, Screen.height * 0.3f, Screen.width * 0.3f, Screen.height * 0.3f), 
				Overlay,
				"",
				RuleGUI.popupWindowStyle);

		}
	}

	void Overlay(int windowId)
	{
		GUILayout.BeginVertical();

		if (Ending == GameEnd.WIN)
			GUILayout.Label(WinningActor.Label + " wins!");

		else if (Ending == GameEnd.LOSE)
			GUILayout.Label(LosingActor.Label + " loses!");

		else if (Ending == GameEnd.DRAW)
			GUILayout.Label("The game was a draw!");

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();

		GUILayout.FlexibleSpace();
		if (GUILayout.Button("OK", GUILayout.Height(70), GUILayout.Width(100)))
		{
			showOverlay = false;
			Reactor.RuleGenerator.Reload();
		}
		GUILayout.FlexibleSpace();

		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
	}
}
