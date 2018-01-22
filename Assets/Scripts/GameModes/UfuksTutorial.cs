using UnityEngine;
using System.Collections;

public class UfuksTutorial : MonoBehaviour {
	public bool started;
	public bool initialState;
	public Stacker MainStack;
	public GameObject RestOfCards;
	public GameObject OriginalDeck;
	public DeckOf deck;
	public GameObject CameraPos;
	public UfuksRules origGame;

	public void TurnOff(){
		started = false;
		gameObject.SetActive (false);
	}	

	public GameObject TopLineStart;
	public GameObject LeftLineStart;
	public GameObject RightLineStart;
	public GameObject GrabDeckSpot;
	public GameObject LivesCamera;
	public GameObject ReshuffleCamera;
	public GameObject DefaultCamera;

	public int[] TopLine;
	public int[] LeftLine;
	public int[] RightLine;
	public int[] GrabDeck;

	public HintControls hint;

	public int tutorialStage;

	void SetInitialState(){
		if ((MainStack != null) && (MainStack.cardsCount == 52)) {
			deck.DropFlags();
			Stacker.AllignNbyAng(TopLine, TopLine.Length, TopLineStart.transform.position, Mathf.PI, true , 15); 
			Stacker.AllignNbyAng(RightLine, RightLine.Length, RightLineStart.transform.position, Mathf.PI*1.25f, true , 15); 
			Stacker.AllignNbyAng(LeftLine, LeftLine.Length, LeftLineStart.transform.position, Mathf.PI*0.75f, true , 15); 
			Stacker.StackNToPos(GrabDeck, GrabDeck.Length, OriginalDeck.transform.position, 0);


			
			for (int i=0; i< RightLine.Length; i++)
				deck.card[RightLine[i]].FaceMe(true);

			for (int i=0; i< LeftLine.Length; i++)
				deck.card[LeftLine[i]].FaceMe(true);

			MainStack.StackNonActiveTo (RestOfCards.transform.position);

			hint.HintTo(" Tap the deck to flip top card ", new Vector3(-90, -25, -20));
			tutorialStage=0;
			initialState=true;
			origGame.cset.Discard.first=0;
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

	// Use this for initialization
	void Start () {
	
	}

	float MessageDelay;
	int tapCounter;
	Card card1;
	Card card2;
	// Update is called once per frame
	void Update () {
		if (started) {
						if (initialState) {

				if (MessageDelay>0){
					if (hint.tapflag){
						MessageDelay-=1;
					}
					MessageDelay-=Time.deltaTime;
					return;

				}

				switch (tutorialStage){
				case 0: 
					Card card=deck.card[GrabDeck[GrabDeck.Length-1]];
					if (card.flagTurn){
						card.FaceMe(true);
						card.flagTurn=false;
						hint.HintTo(" Well done, strong tap! ", new Vector3(-90, -25, -20));

						tutorialStage+=1;
						//MessageDelay=4;
					}
						break;
				case 1:
					hint.HintTo(" Now Tap Me! ");
					tutorialStage+=1;
					break;
				case 2:
					if (hint.tapflag==true){
						tutorialStage+=1;
						hint.HintTo(" Again! ");
						tapCounter=0;
					}
					break;

				case 3:
					if (hint.tapflag==true){
						tapCounter--;
						hint.HintTo(" Faster! ");
						if (tapCounter<0){
							tutorialStage+=1;
							hint.HintTo(" WOW, strong finger, hard tap! ");
							//MessageDelay=3;
						}
					}
					break;
				case 4: hint.HintTo(" I think you have tapped a few pixels out of me "); //MessageDelay=6;
					tutorialStage+=1; break;
				case 5: hint.HintTo(" I need to work, you know... "); //MessageDelay=5;
					tutorialStage+=1; break;
				case 6: hint.HintTo(" Now tap this 2 aces ... ", new Vector3(-67, -20, -20)); tutorialStage+=1;
					card1=deck.card[GrabDeck[GrabDeck.Length-1]];
					card2=deck.card[LeftLine[LeftLine.Length-1]];
					hint.PointArrow(0, card1.transform.position);
					hint.PointArrow(1, card2.transform.position);
					card1.flagTurn=false;
					card2.flagTurn=false;
					break;
				case 7: 
					origGame.ProcessOpenCard(card1.index,-1);
					if (origGame.ProcessOpenCard(card2.index,-1)){
					     tutorialStage+=1;
						origGame.ProcessOpenCard(card1.index,-1);
						hint.HintTo(" Now this two 8s ... ",new Vector3(80, -70, -20));
						card1=deck.card[RightLine[RightLine.Length-1]];
						card2=deck.card[LeftLine[LeftLine.Length-2]];
						hint.PointArrow(0, card1.transform.position);
						hint.PointArrow(1, card2.transform.position);
						card1.flagTurn=false;
						card2.flagTurn=false;
					}
					card1.flagTurn=false;
					card2.flagTurn=false;
					break;
				case 8: 
					origGame.ProcessOpenCard(card1.index,-1);
					if (origGame.ProcessOpenCard(card2.index,-1)){
						tutorialStage+=1;
						origGame.ProcessOpenCard(card1.index,-1);
						hint.HintTo(" Now double-tap top one of those two queens! ... ");
						card1=deck.card[RightLine[RightLine.Length-2]];
						card2=deck.card[RightLine[RightLine.Length-3]];
						hint.PointArrow(0, card1.transform.position);
						card1.flagTurn=false;
						card2.flagTurn=false;
					}
					card1.flagTurn=false;
					card2.flagTurn=false;
					break;
				case 9: 

					if (origGame.ProcessOpenCard(card1.index,card2.index)){
						tutorialStage+=1;
						origGame.ProcessOpenCard(card2.index,-1);
						hint.HintTo(" Great! ... Flip the deck. ", new Vector3(-90, -25, -20));
						card1=deck.card[GrabDeck[GrabDeck.Length-2]];
						hint.PointArrow(0, card1.transform.position);
					}
					card1.flagTurn=false;
					card2.flagTurn=false;
					break;
				case 10:
					if (TutCardFlip( card1)) hint.HintTo(" This is not what we need. Hit it again. "); break;
				case 11: 
					if (TutCardFlip( card1)){
						card1.MoveMeTo(GrabDeckSpot.transform.position, 0);
						hint.HintTo(" Next one. ");
						card1=deck.card[GrabDeck[GrabDeck.Length-3]];
						card1.flagTurn=false;
					}
					break;
				case 12: 
					if (TutCardFlip( card1)) hint.HintTo(" Nope! "); break;
				case 13: 
					if (TutCardFlip( card1)){
						card1.StackMeTo(deck.cardObj[GrabDeck[GrabDeck.Length-2]]);
						hint.HintTo(" Next... ");
						card1=deck.card[GrabDeck[GrabDeck.Length-4]];
						card1.flagTurn=false;
					}
					break;
				case 14: 
					if (TutCardFlip( card1)) {hint.HintTo(" Yes, the one! Do it. "); 
						card2=deck.card[LeftLine[LeftLine.Length-3]];
						hint.PointArrow(0, card1.transform.position);
						hint.PointArrow(1, card2.transform.position);
					} break;
				case 15: 
					origGame.ProcessOpenCard(card1.index,-1);
					if (origGame.ProcessOpenCard(card2.index,-1)){
						tutorialStage+=1;
						origGame.ProcessOpenCard(card1.index,-1);
						hint.HintTo(" Now we can access top cards! ... flip it ", new Vector3(0, 50, -20));
						card1=deck.card[TopLine[TopLine.Length-1]];
						card2=deck.card[GrabDeck[GrabDeck.Length-3]];
						hint.PointArrow(0, card1.transform.position);
						card1.flagTurn=false;
						card2.flagTurn=false;
					}
					card1.flagTurn=false;
					card2.flagTurn=false;
					break;
				case 16:
					if (TutCardFlip( card1)) { hint.HintTo(" Perfect! You know what to do... ", new Vector3(35, -20, -20));
						origGame.cset.LivesCounter=1;
						origGame.RecalCulateLives();
					
					} break;
				case 17:
					origGame.ProcessOpenCard(card1.index,-1);
					if (origGame.ProcessOpenCard(card2.index,-1)){
						tutorialStage+=1;
						origGame.ProcessOpenCard(card1.index,-1);
						hint.HintTo(" We are almost done. ");
						card1=deck.card[TopLine[TopLine.Length-2]];
						card2=deck.card[GrabDeck[GrabDeck.Length-2]];
						card1.flagTurn=false;
						card2.flagTurn=false;
					}
					card1.flagTurn=false;
					card2.flagTurn=false;
					break;
				case 18:
					origGame.ProcessOpenCard(card1.index,-1);
					if (origGame.ProcessOpenCard(card2.index,-1)){
						tutorialStage+=1;
						origGame.ProcessOpenCard(card1.index,-1);
						hint.HintTo(" This is it. Last card: +2 extra lifes. ", new Vector3 (270,160, -5));

					}
					card1.flagTurn=false;
					card2.flagTurn=false;
					break;
				case 19:
					origGame.cset.LivesCounter=3;
					origGame.RecalCulateLives();
					globV.mainCamera.MoveMe (LivesCamera);
					tutorialStage+=1;
					MessageDelay=6;
					break;
				case 20:
					globV.mainCamera.MoveMe (ReshuffleCamera);
					ReshuffleButton.SetActive(true);
					tutorialStage+=1;
					MessageDelay=8;
					hint.HintTo(" But some times... most times, we can't get it.", new Vector3 (-100,75, -10));
					break;
				case 21:
					hint.HintTo("This button reshuffles cards but takes one life.");
					hint.PointArrow(0, ReshuffleButton.transform.position);
					tutorialStage+=1;
					MessageDelay=7;
					break;
				case 22: 
					hint.HintTo("Now you are ready, real game...");
					tutorialStage+=1;
					MessageDelay=5;
					break;
				case 23: 
					hint.HideMe();
					PlayerPrefs.SetInt("UfukTut", 1);
					origGame.Tutorial=1;
					origGame.Init();
					globV.mainCamera.MoveMe(DefaultCamera);
					TurnOff();
					break;
				}
			



						} else
								SetInitialState ();
				}
	
	}

	public GameObject ReshuffleButton;

	

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
