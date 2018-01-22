using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[Serializable]
public class TutorialPiece{
	public int[] line;
	public string[] texts;
}

public class RoseTutoreal : MonoBehaviour {

	public bool started;
	public bool initialState;
	public Stacker MainStack;
	public GameObject RestOfCards;
	public GameObject OriginalDeck;
	public DeckOf deck;
	public GameObject CameraPos;
	public BloomRoseController origGame;
	
	public void TurnOff(){
		started = false;
		gameObject.SetActive (false);
	}	
	
	public GameObject OneLineStart;
	public GameObject TwoLineStart;
	public GameObject GrabDeckSpot;
	public GameObject DefaultCamera;
	
	public int[] LineOrder;
	public int[] LineSame;
	public int[] LineColors;
	public int[] LineArrangement;
	public int[] LineSkipAce;
	public TutorialPiece[] lines;
	int CurrentLine;
	int CurrentHint;

	public HintControls hint;
	
	public int tutorialStage;

	public int perCardOffsetX;

	bool GiveExample (ref int [] list){
		for (int i=0; i< list.Length; i++)
		if (list [i] != -1) {
			deck.card [list [i]].FaceMe (true);
			deck.ForceUnstackCard(list [i]);
		
		}
		Vector3 dest = OneLineStart.transform.position;
		dest.x += perCardOffsetX * (6 - list.Length);
		Stacker.AllignNbyAng(list, list.Length, dest, 0, false , 60);

		deck.StackUnstacked (RestOfCards);
		return true;
	}

	void SetInitialState(){
		if ((MainStack != null) && (MainStack.cardsCount == 52)) {
			
			GiveExample (ref LineOrder);
		//	MainStack.AllignNbyAng(RightLine, RightLine.Length, RightLineStart.transform.position, Mathf.PI*1.25f, true , 15); 
		//	MainStack.AllignNbyAng(LeftLine, LeftLine.Length, LeftLineStart.transform.position, Mathf.PI*0.75f, true , 15); 
		//	MainStack.StackNToPos(GrabDeck, GrabDeck.Length, OriginalDeck.transform.position, 0);
		
			 CurrentLine=0;
			 CurrentHint=0;
			

			
			//hint.HintTo(" Tap the deck to flip top card ", new Vector3(-90, -25, -20));
			tutorialStage=0;
			initialState=true;
		}else if (deck.card[0].Stacked==true)
			MainStack=deck.card[0].stacker;
		
		
	}
	
	public void Init(){
		started = true;
		initialState = false;
		MainStack = null;
		gameObject.SetActive (true);
		deck = globV.DeckInstantiating.GetComponent<DeckOf> ();
		deck.ShuffleCards (OriginalDeck, 0);
		globV.mainCamera.MoveMe (CameraPos);
		
	}
	
	float MessageDelay;
	int tapCounter;
	// Update is called once per frame
	void Update () {
				if (started) {
						if (initialState) {
				
								if (MessageDelay > 0) {
										if (hint.tapflag) {
												MessageDelay -= 1;
										}
										MessageDelay -= Time.deltaTime;
										return;
					
								}
				
								switch (tutorialStage) {
								case 0: 
										hint.HintTo (" Hello! Tap on me.", new Vector3 (0, -25, -20));
										tutorialStage += 1;
										break;
				case 1: 
					if (hint.tapflag==true){
						hint.HintTo (" Let the lesson begin! ");
						tutorialStage += 1;
					}
					break;
				case 2: 
					if (hint.tapflag==true){
					hint.HintTo (" Cards will score if grouped in order.");
					tutorialStage += 1;
					}
					break;
				case 3:
					if (hint.tapflag==true){
						hint.HintTo(" Or if they are the same value.");
						GiveExample (ref LineSame);
						tutorialStage += 1;
					}
					break;
				case 4:
					if (hint.tapflag==true){
						hint.HintTo(" Color does not matter");
						GiveExample (ref LineColors);
						tutorialStage += 1;
					}
					break;
				case 5:
					if (hint.tapflag==true){
						hint.HintTo(" Most awesome");
						GiveExample (ref LineArrangement);
						tutorialStage += 1;
					}
					break;
				case 6:
					if (hint.tapflag==true){
						hint.HintTo(" and most complicated thing ... ");
						tutorialStage += 1;
					}
					break;
				case 7:
					if (hint.tapflag==true){
						hint.HintTo(" They don't have to be arranged,");
						tutorialStage += 1;
					}
					break;
				case 8:
					if (hint.tapflag==true){
						hint.HintTo(" just grouped!");
						tutorialStage += 1;
					}
					break;
				case 9:
					if (hint.tapflag==true){
						hint.HintTo(" Ace and 2 are like 1 & 2 (in order)");
						GiveExample (ref LineSkipAce);
						tutorialStage += 1;
					}
					break;
				case 10:
					if (hint.tapflag==true){
						if (CurrentHint==0)
							GiveExample (ref lines[CurrentLine].line);
						hint.HintTo(lines[CurrentLine].texts[CurrentHint]);
						CurrentHint+=1;
						if (CurrentHint>=lines[CurrentLine].texts.Length){
							CurrentHint=0;
							CurrentLine+=1;
							if (CurrentLine>=lines.Length)
								tutorialStage += 1;
						}

					}
					break;
				case 11:
					if (hint.tapflag){
						hint.HideMe();
						origGame.Init();
						globV.mainCamera.MoveMe(DefaultCamera);
						TurnOff();
					}
					break;
								}
				
				
				//		public TutorialPiece[] lines;
			//	int CurrentLine;
				//int CurrentHint;
				
						} else
								SetInitialState ();
				}
		}
		

	

	
	private bool TutCardFlip(Card card){
		if (card.flagTurn) {
			card.FaceMe (true);
			tutorialStage += 1;
			card.flagTurn = false;
			return true;
		}
		return false;
	}
}
