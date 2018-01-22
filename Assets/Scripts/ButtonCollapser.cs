using UnityEngine;
using System.Collections;

public class ButtonCollapser : MonoBehaviour {

	public GameObject[] Allbuttons;
	public GameObject[] MainMenuButtons;
	public GameObject[] CardBacks;
	public CameraTouchScreen MainCamera;
	public float ExitDelay;
	public UfuksRules us;
	public DefaultGameController dc;
	public TapHolesGameController th;
	public BloomRoseController br;
	public EntangledCardsController ep;
	public MemoryGuy mg;
	public RoseTutoreal rt;
	public UfuksTutorial ut;
	public DeckOf deck;
	public HintControls hinter;

	public bool MainMenu=true;

	public void ShowMenuUI(){
		for (int i=0; i<MainMenuButtons.Length; i++)
			MainMenuButtons [i].SetActive (true);
		for (int i=0; i<CardBacks.Length; i++)
				CardBacks[i].SetActive (globV.mySave.cardBacks[i]);
		}

	public void CollapseMenuUI(){
		for (int i=0; i<MainMenuButtons.Length; i++)
			MainMenuButtons [i].SetActive (false);
		for (int i=0; i<CardBacks.Length; i++)
			CardBacks[i].SetActive (false);
	}
	
	public void CollapseInGameUI(){
		for (int i=0; i<Allbuttons.Length; i++)
			Allbuttons [i].SetActive (false);
		deck.DropFlags ();
	}

	void CollapseControllers(){
		us.TurnOff ();
		dc.TurnOff ();
		th.TurnOff ();
		br.TurnOff ();
		ep.TurnOff ();
		mg.TurnOff ();
		ut.TurnOff ();
		rt.TurnOff ();
		MainMenu = false;
	}

	void Start(){
		InitMainMenu ();
		}

	public void InitMainMenu(){
		CollapseInGameUI ();
		CollapseControllers ();
		if (hinter.CollapsableHint)
						hinter.ShowMe ();
		ShowMenuUI ();
		dc.Init ();
		MainMenu = true;
		ExitDelay = 1;
		deck.DropFlags ();
	}

	public void CloseMenu(){
		hinter.HideMe ();
		CollapseMenuUI ();
		CollapseControllers ();
		deck.DropFlags ();

	}

	void Update(){
		ExitDelay -= Time.deltaTime;
		if ((Input.GetKey (KeyCode.Escape)) || (globV.ExitCall)) {
			globV.ExitCall=false;
			if (MainMenu==false){
				if (deck.Arranging())
						return;

			}
			if ((MainMenu) && (ExitDelay<=0))
			Application.Quit();
			else
				if (!MainMenu)
				InitMainMenu();
		}
	}



}
