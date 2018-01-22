using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;


[Serializable]
public class CurveStorer{
	public const int max_points=512;
	public Vector2[] point = new Vector2[max_points];
	public float[] dist = new float[max_points];
	public int count;
	public float Length;
	public bool active;

	public void BeginCurve(Vector3 first){
		count = 1;
		active = true;
		Length = 1;
		point [0] = new Vector2 (first.x, first.y);
		dist [0] = 1;

	}

	public void Add(Vector2 NewPoint){
		if (count < max_points) {
			point[count]=NewPoint;
			if (count>0){
				dist[count]=Vector2.Distance(point[count-1], NewPoint);
				Length+=dist[count];
				globV.deck.WindyEffect.Fuel(globV.SetPullOff(Input.mousePosition),1);
			}
			count++;
	
		}
	}

}

[Serializable]
public class GlobT{


	public CurveStorer curve;
	public int TappedStacked;
	public bool SliderUse;
	public bool startedonTable;
	public bool[] selected=new bool[52];
	public int[] selectedNo = new int[52];
	public int selectedCount;
	public bool multitouch;
	public int touchedCard;
	public bool multiMoveReady;
	public bool multiMoveAllowed;
	public bool multiMoveDrag;
	public bool multiMoveActive;
	public bool multiFirstFaced;
	public int multiMoveStartOn;
	public Vector3 StretchStarted;
	public float minAllignGap;
	public int testVal=0;
	public Slider slider;
	public MySmartButtonScript SliderText;
	public bool freeGame;
	public HintControls hinter;
	public bool StackMoving;


	public void EndTouch(int card){

		if (startedonTable) {
						startedonTable = false;
			         if (((card != -1) && (multitouch==false)) || (touchedCard==-1))
								return;
						else
				if (multitouch) { 
				if (!freeGame) return;
								multiMoveReady = true;
								multiMoveAllowed = false;
								multitouch=false;
								StackMoving=false;

						} else {

								Card cardc = globV.deck.card [touchedCard];
								if ((cardc.Stacked) && (freeGame))
										cardc.stacker.FlipStack ();
								else 
										cardc.flagTurn = true;
						}
				} else 
			if ((multiMoveActive) && (freeGame))
		{
			multiMoveActive=false;
			multitouch=false;
			if (multiMoveDrag){
				Card ccard=globV.deck.card[card];
				if ((ccard.WillStack>1) || (StackMoving)){
					int ToInd;
					ToInd=StackMoving ? card :
						ccard.WillStackTo.index;
					Card To=globV.deck.cardObj[ToInd];
					for (int i=0; i<52; i++)
						if (selected[i]){
							if (i!=ToInd)
							globV.deck.card[i].StackMeTo(To);
					else {
							Card tmpc=globV.deck.card[i];
							tmpc.MoveMeTo(new Vector3(To.transform.position.x,
							                                        To.transform.position.y,
						                                        0
						                                        ),0);
							if (selected[tmpc.WillStackTo.index]){
							tmpc.WillStack=-1;
							tmpc.WillStackTo=null;
							}

						}
					}
				} else{
					for (int i=0; i<52; i++)
						if (selected[i]){

							globV.deck.card[i].pulled=false;
						globV.deck.card[i].AssignZ();
						globV.deck.ConstrainCard(i);
					}
							//globV.deck.card[i].SetPull(false);//flagDrop=true;
				}

			} else {
				if (!freeGame) return;
				if ((card!=-1) && (selected[card]==false)) 
					return;
				Vector3 To=globV.SetPullOff(Input.mousePosition);
				float dist=globV.GetMy2DDistance(StretchStarted, To);
				int count=countSelected();
			

				if ((dist<count*minAllignGap) && (!globV.ArrowWay)) return;
				StretchStarted.z=0;
				To.z=0;
				float ang=globV.FuckenAngle(To-StretchStarted);

				int[] selection=new int[count];
				int ind=0;
				for (int i=0; i<52; i++)
					if (selected[i]){
					selection[selectedNo[i]]=i;
					ind++;
				}

				if (!globV.orientation) ang+=Mathf.PI/2;
				if (! globV.ArrowWay)
				Stacker.AllignNbyAng(selection, count, StretchStarted, ang, globV.orientation, dist/count); 
				else {
					Stacker.AllignByLine(selection, count, globV.orientation, curve);
				//	Debug.Log("Arranging "+count+" cards in "+curve.Length+" sm curve");
					curve.active=false;
				}
				//Stacker.StackNToPos(selection, count, StretchStarted, ang);
				

			}
		
				}
		else 
		if ((freeGame) && (touchedCard != -1) && (globV.deck.cardObj[touchedCard].gameObject.activeSelf) && (multitouch==false) && (slider.value>1)) {
			Card ncard=globV.deck.card[touchedCard];

			if ((ncard.Stacked) && (!ncard.pulled) && (!ncard.flagPull)){
				Stacker nstacker=ncard.stacker;
				for (int i=0; i<52; i++)
					selected[i]=false;
				multitouch=false;
				for (int i=0; i<slider.value; i++){
					Card temp=nstacker.cards[nstacker.cardsCount-1-i];
					selected[temp.index]=true;
					selectedNo[temp.index]=i;
				}
				selectedCount=(int)slider.value;
				StackMoving=true;
				slider.gameObject.SetActive(false);
				slider.value=1;
				multiMoveReady = true;
				multiMoveAllowed = false;
				
			}
	}		
	}	





	public int countSelected(){
	//	int b=0;
	//	for (int i=0; i<52; i++)
		//				if (selected [i])
		//						b++;
		return selectedCount;
	}

	public void TouchContinue(int card){
		if (startedonTable) {

			Card ccard=globV.deck.card[card];
			if (selected[card]==false)
				ccard.PlayMy(ccard.selectSounds);
		

			if (touchedCard == -1){
				touchedCard = card;
				multiFirstFaced=ccard.Faced;
				selected [card] = true;
				selectedCount=1;
				selectedNo[card]=0;
			}
			else
				if ((touchedCard!=card) && (globV.deck.card[card].Faced==multiFirstFaced)){
						multitouch=true;
				if (!selected[card]){
				selected [card] = true;
					selectedNo[card]=selectedCount;
					selectedCount++;
				}
			}
			
		}
	}




public void StartTouch(int card){

		if (EventSystem.current.IsPointerOverGameObject())
						return;

 if ((multiMoveReady) && (freeGame)) {
						multiMoveActive = true;
						multiMoveReady = false;
						multiMoveDrag = false;
						multiMoveStartOn = card;
						StretchStarted = globV.SetPullOff (Input.mousePosition);
						if (card != -1) {
								touchedCard = card;
								if (selected [card]) {
										for (int i=0; i<52; i++)
												if (selected [i]) {
					//	globV.deck.OrderCardTo (i, selectedNo[i]);
														Card cardd = globV.deck.card [i];
														cardd.flagPull = true;
														cardd.SetOffFromMouse ();
														multiMoveDrag = true;
												}
								} 

						}
			else {
			if (!globV.ArrowWay)
				globV.Arrow.UpdateFrom(StretchStarted);
				else 
					curve.BeginCurve(Input.mousePosition);
			}
				} else 
			if (card == -1) {

						for (int i=0; i<52; i++)
								selected [i] = false;
						selectedCount=0;
						multitouch = false;
						startedonTable = true;
						touchedCard = -1;
						multiMoveReady = false;
						multiMoveActive = false;

				} else {
						touchedCard = card;
			Card ccard=globV.deck.card[card];
			if (ccard.Stacked) TappedStacked=ccard.otherC.index;
			else
				TappedStacked=-1;
		}
	}
}



public class TableScript : MonoBehaviour {

	public GlobT Tt;


	public void TouchSlider(bool first){
			sliderFadeDelay = 2f;
	}

	void OnMouseOver(){
		if ((sliderFadeDelay > 0) && (Input.mousePosition.y < 80))
						return;
				else
						sliderFadeDelay = 0;

		if(Input.GetMouseButtonDown(0))
			Tt.StartTouch(-1);
	}

	void OnMouseUp() {
		Tt.EndTouch (-1);	
		globV.Arrow.HideMe ();
	}

	void Start() {
		globV.Tt = Tt;
	}

	void Markselected(){
		for (int i=0; i<52; i++)
			if (Tt.selected[i])
				globV.deck.card[i].SetSelected();
	}
	public float sliderFadeDelay;



	public void SetFreeGame(bool set){
		Tt.freeGame = set;
	}

	void Update () {
		if (!Tt.freeGame)
						return;

		if ((Tt.multiMoveActive) && (!Tt.multiMoveDrag)) {
			if (!globV.ArrowWay)
			globV.Arrow.UpdateTo(globV.SetPullOff (Input.mousePosition));		
			else 
				if (Tt.curve.active)
			Tt.curve.Add(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			
		}

		if (sliderFadeDelay >= 0) {
			sliderFadeDelay-=Time.deltaTime;
			if (sliderFadeDelay<0)
			Tt.slider.gameObject.SetActive (false);
				}


		if ((Tt.startedonTable) && (Tt.touchedCard != -1)) {
						if (Tt.multitouch)
								Markselected ();
						else
								globV.deck.card [Tt.touchedCard].SetSelected ();
				} else
			if (Tt.multiMoveReady)  
						Markselected ();
				else 
		if ((Tt.touchedCard != -1) && (globV.deck.cardObj [Tt.touchedCard].gameObject.activeSelf)
						&& (!Tt.multitouch) && (Tt.TappedStacked != -1)) {
						Card card = globV.deck.card [Tt.touchedCard];
			if ((card.Stacked==false) || (Tt.TappedStacked != card.otherC.index))
								return;
						if ((card.Stacked) && (!card.pulled) && (!card.flagPull)) {
								card.SetSelected ();
								Tt.slider.gameObject.SetActive (true);
								if (LastHinterValue != (int)Tt.slider.value) {
										LastHinterValue = (int)Tt.slider.value;
					Tt.SliderText.MyUpdatableValue(LastHinterValue);				
					Tt.hinter.CollapsableHintTo (" " + LastHinterValue + "/" + card.stacker.cardsCount, 
					                             new Vector3( card.gameObject.transform.position.x,
					            			card.gameObject.transform.position.y+30, 
					            card.gameObject.transform.position.z-10));
					Tt.hinter.LetterCount=3;
								}
								if (Tt.slider.maxValue != card.stacker.cardsCount) {
										Tt.slider.maxValue = card.stacker.cardsCount;
										Tt.slider.value = 1;
								}
								sliderFadeDelay = 1;
						}


			
				} else {
			Tt.hinter.HideMe();
						sliderFadeDelay = 0;
				}


	}

	public int LastHinterValue;
}
