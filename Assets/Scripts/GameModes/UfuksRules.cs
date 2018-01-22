using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public class stackSaver {
	public int first;
	public int count;
	public int[] cards = new int[52];
}

public class UfuksRules : MonoBehaviour {



	public CSet cset=new CSet();

	public GameObject DeckObj;
	public DeckOf deck;
	public Stacker MainStack;
	public Stacker SecondStack;

	public GameObject Discard;
	public GameObject ReshuffleButton;
	public GameObject GrabStack;
	public GameObject ShuffleTo;
	public GameObject SaveStartDeck;
	public GameObject CentralCardPosition;
	public GameObject HowToPlay;
	public MySmartButtonScript CardLeftCount;
	public MySmartButtonScript PairsDisplay;
	public MySmartButtonScript BestScoreText;

	public GameObject[] LivesDisplay;
	public float aroundDistance;
	public float outerDistance;
	private bool started=false;
	bool initialState=false;
	public int BestScore;
	// Use this for initialization



	[System.Serializable]
	public class CSet
	{
		public int[]  centralInd = new int[3];
		public int[,] aroundInd  = new int[4,3];
		public int[,] outerInd   = new int[8,3];
		public int    centralLeft=0;
		public int[]  aroundLeft = new int[4];
		public int[]  outerLeft  = new int[8];
		public bool[] aroundAccessed = new bool[4];
		public bool   centerAccessed=false;
		public int selectedInt=-1;
		public int clearOther;
		public bool mainStack;
		public int PairsCounter;
		public int LivesCounter;
		public stackSaver Grab=new stackSaver();
		public stackSaver MStack=new stackSaver();
		public stackSaver Discard=new stackSaver();
		public bool CardsArranged;
	}

	public int Tutorial;

	void SetInitialState(){
		if ((MainStack != null) && (MainStack.cardsCount==52)) {

			int ind=51;
			cset.MStack.first=MainStack.cards[0].index;
			cset.Grab.first=-1;
			cset.mainStack=true;
			cset.selectedInt=-1;
			cset.Discard.first=-1;
			cset.clearOther=-1;
			cset.centerAccessed=false;
			cset.centralLeft=MainStack.StackNToPos(ref ind,ref cset.centralInd, 3, CentralCardPosition.transform.position , 0f);

			for (int i=0; i<4; i++){
				float ang=Mathf.PI/4-i*Mathf.PI/2;


				Vector3 tmp=globV.displace(CentralCardPosition.transform.position, aroundDistance, ang);
			cset.aroundLeft[i]=MainStack.AllignNbyAng(ref ind, ref cset.aroundInd, i,3,tmp , ang, true,15);
			cset.aroundAccessed[i]=false;

			
				tmp=globV.displace(tmp, outerDistance, ang);

				for (int j=0; j<2; j++){
				float tmpAng=ang-Mathf.PI/4 + (j==1 ? Mathf.PI/2 : 0);
				Vector3 tmp2=globV.displace(tmp, aroundDistance, tmpAng);
					int dim=i*2 + j;
					cset.outerLeft[dim]=MainStack.AllignNbyAng(ref ind, ref cset.outerInd, dim,3, tmp2,tmpAng, true,15);
					for (int h=0; h<3; h++)
						deck.card[cset.outerInd[dim,h]].FaceMe(true);

				}
			}


		
			initialState=true;
			CardLeftCount.ShowMe ();
			CardLeftCount.MyUpdatableValue(52);
		

		}
		else
			if (deck.card[0].Stacked==true)
				MainStack=deck.card[0].stacker;
			

	}
	private bool WaitingForArrangement;

	bool IsStackDone (stackSaver st){
		Card card = deck.card [st.cards [st.count - 1]];
		if ((card.Stacked == true) && (card.GoingToAPlace == false) && (card.StackStarted == false))
						return true;

			return false;
		}

	void LoadInitialState(){

		if (WaitingForArrangement) {

			if (
			   ((cset.MStack.count<2) || (cset.MStack.first==-1) ||
			 IsStackDone (cset.MStack)) &&
				((cset.Grab.count<2) || (cset.Grab.first==-1) ||
			 IsStackDone (cset.Grab))
				){
				if (cset.mainStack)
					MainStack=deck.card[cset.MStack.cards[cset.MStack.count-1]].stacker;
			//	if (deck.card[cset.Grab.cards[cset.Grab.count-1]].Stacked==true)
			//		MainStack=deck.card[cset.MStack.cards[cset.MStack.count-1]].stacker;
			initialState=true;
				cset.CardsArranged=false;
			}
			return;
		}


		if ((SecondStack != null) && (SecondStack.cardsCount==52)) {

			Stacker.StackNToPos(cset.centralInd, cset.centralLeft, CentralCardPosition.transform.position , 0f);
			for (int i=0; i<4; i++){
				float ang=Mathf.PI/4-i*Mathf.PI/2;
				
			
				Vector3 tmp=globV.displace(CentralCardPosition.transform.position, aroundDistance, ang);
				Stacker.AllignNbyAng(cset.aroundInd, i,cset.aroundLeft[i],tmp , ang, true,15);
			


				tmp=globV.displace(tmp, outerDistance, ang);
				
				for (int j=0; j<2; j++){
					float tmpAng=ang-Mathf.PI/4 + (j==1 ? Mathf.PI/2 : 0);
					Vector3 tmp2=globV.displace(tmp, aroundDistance, tmpAng);
					int dim=i*2 + j;
					Stacker.AllignNbyAng(cset.outerInd, dim,cset.outerLeft[dim], tmp2,tmpAng, true,15);

					for (int h=0; h<3; h++)
						deck.card[cset.outerInd[dim,h]].FaceMe(true);
				
				}
			}

			stackSaved(ShuffleTo.transform.position, cset.MStack);
			stackSaved(Discard.transform.position, cset.Discard);
			stackSaved(GrabStack.transform.position, cset.Grab);
			for (int i=0; i<cset.Grab.count; i++)
				deck.card [cset.Grab.cards [i]].FaceMe(true);

		
			CardLeftCount.ShowMe ();
			CardLeftCount.MyUpdatableValue(52);
			WaitingForArrangement=true;
			cset.CardsArranged=false;

		}
		else
			if (deck.card[0].Stacked==true)
				SecondStack=deck.card[0].stacker;
	}

	void stackSaved(Vector3 to, stackSaver ss){
		if (ss.first == -1)
						return;
		Card f = deck.card [ss.first];
		f.MoveMeTo (to, 0);
		for (int i=1; i<ss.count; i++)
			deck.card [ss.cards [i]].StackMeTo (deck.cardObj [ss.cards[i-1]]);
	}

	void PairCounterPlus(){
		if (started == false)
						return;
		cset.PairsCounter+=1;
		if ((cset.PairsCounter % 52) ==0) {
			cset.LivesCounter += 2;
					RecalCulateLives();
					deck.TryUnlockCardBack(1);
			deck.MyMessageText.ShowMessage (" 52 pairs: +2 life! ");
				}
		PairsDisplay.ChangeMyText(cset.PairsCounter+"/52");
	}

	public bool ProcessOpenCard(int i, int pre){

		if (cset.clearOther != -1) {
			if (i==cset.clearOther){
				Card card=deck.card[cset.clearOther];
				card.StackMeToTop(deck.cardObj[cset.Discard.first]);
				card.PlayMy(card.cutSounds);
				cset.clearOther=-1;
				return true;
			}

			return false;
		}

		if (deck.cardObj [i].gameObject.activeSelf) {
			Card card = deck.card [i];
			if ((card.flagTurn==true) && (!card.GoingToAPlace) && (!card.StackStarted) && (card.UnstackCoolOff<=0)){
				if (card.Faced==false) {card.FaceMe(true); card.PlayMy(card.flipSounds); cset.selectedInt=i;}
				else {
					//card.PlayMy(globV.MyAudio.ClothScratch);4
					//globV.MyAudio.PlayMySoundSet(globV.MyAudio.ClothScratch);

					if ((cset.selectedInt==-1) || (cset.selectedInt==i)){

						if ((pre!=-1) && (cset.selectedInt==i)){
							Card card2=deck.card[pre];
							if (card.value==card2.value){
								PairCounterPlus();
								cset.clearOther=pre;
								cset.selectedInt=-1;
								if (cset.Discard.first==-1){
									cset.Discard.first=i;
									card.MoveMeTo(Discard.transform.position,0);
								}
								else
									card.StackMeToTop(deck.cardObj[cset.Discard.first]);

								//card.PlayMy(card.cutSounds);
								return true;
							}
						}
						cset.selectedInt=i;
					}
					else 
					{
						Card card2=deck.card[cset.selectedInt];
						if (card.value==card2.value){
							PairCounterPlus();
							cset.clearOther=cset.selectedInt;
							cset.selectedInt=-1;
							if (cset.Discard.first==-1){
								cset.Discard.first=i;
								card.MoveMeTo(Discard.transform.position,0);
							}
							else
								card.StackMeTo(deck.cardObj[cset.Discard.first]);
							return true;
						}
						else
							cset.selectedInt=i;
					}
				}
			}
		
		}

		return false;
	}

	// Update is called once per frame
	void Update () {
		if (started) {
			if (initialState){
// Select from Grab stack


				if (cset.CardsArranged==false){
					if (deck.Arranging()==false)
						cset.CardsArranged=true;
					deck.DropFlags();
					return;
				}

				if (cset.Grab.first!=-1){
					Card card=deck.card[cset.Grab.first];
					if (card.Stacked){
						Stacker stack=card.stacker;
						card=stack.cards[stack.cardsCount-1];
					}
					if ((ProcessOpenCard(card.index, (card.index==cset.Grab.first) ? -1 : card.otherC.index))
					    && (card.index==cset.Grab.first))
						cset.Grab.first=-1;
				}
// Send from MainStack to grab stack
				if (cset.mainStack) {
					Card card=MainStack.cards[MainStack.cardsCount-1];

					bool cfaced=card.Faced;
							if ((!ProcessOpenCard(card.index, -1)) && (card.flagTurn) && (cfaced))
							{
							 if (cset.Grab.first==-1){
									card.MoveMeTo(GrabStack.transform.position,0);
									cset.Grab.first=card.index;
								}
								else
									card.StackMeToTop(deck.cardObj[cset.Grab.first]);
								
							}
					else CardLeftCount.MyUpdatableValue(MainStack.cardsCount);
					
								if (MainStack.cardsCount<2) cset.mainStack=false;

				}
				else 
				if (cset.MStack.first!=-1){
					Card card=deck.card[cset.MStack.first];
		
						bool cfaced=card.Faced;
						if ((!ProcessOpenCard(card.index, -1)) && (cfaced==true) && (card.flagTurn))
						{
							cset.selectedInt=card.index;
							if (cset.Grab.first!=-1)
							card.StackMeTo(deck.cardObj[cset.Grab.first]);
							else
							{
								card.MoveMeTo(GrabStack.transform.position,0);
								cset.Grab.first=card.index;
							}
							cset.MStack.first=-1;
							CardLeftCount.HideMe();
						}



				}
// Select from Center

			if (cset.centerAccessed){
				if (ProcessOpenCard(cset.centralInd[cset.centralLeft-1], -1)){
						cset.centralLeft-=1;
					if (cset.centralLeft==0){
							cset.LivesCounter+=4;
							deck.MyMessageText.ShowMessage (" Great work! +3 life ");
						Reshuffle();
						}
						// Idea! If you get all the cards deck gets reshuffled and you dont loose a try
					}
				}
				else{
// Select from around circle
				for (int i=0; i<4; i++){

					if (cset.aroundLeft[i]>0){

						if (cset.aroundAccessed[i]){
								if (ProcessOpenCard(cset.aroundInd[i,cset.aroundLeft[i]-1], -1)){
									cset.aroundLeft[i]-=1;
									if (cset.aroundLeft[i]==0){
										bool someLeft=false;
										for (int h=0; h<4; h++)
											if (cset.aroundLeft[h]>0) someLeft=true;
										if (someLeft==false) cset.centerAccessed=true;
									}
								}
						} else{
// Select from outer circle	
								int ind=i*2;

								if (cset.outerLeft[ind]>0)
								if (ProcessOpenCard(cset.outerInd[ind,cset.outerLeft[ind]-1],
									                   (cset.outerLeft[ind]>1) ? cset.outerInd[ind,cset.outerLeft[ind]-2] :-1)){
									cset.outerLeft[ind]-=1;
									if ((cset.outerLeft[ind]<1) && (cset.outerLeft[i*2+1]<1))
										cset.aroundAccessed[i]=true;
								}
									ind+=1;	
								if (cset.outerLeft[ind]>0)
									if (ProcessOpenCard(cset.outerInd[ind,cset.outerLeft[ind]-1],
									                   (cset.outerLeft[ind]>1) ? cset.outerInd[ind,cset.outerLeft[ind]-2] :-1)){
										cset.outerLeft[ind]-=1;
										if ((cset.outerLeft[ind]<1) && (cset.outerLeft[i*2]<1))
											cset.aroundAccessed[i]=true;
								}


								}
							
							
						}
				}
					
					
				

				}

// clean flags
				for (int i=0; i<52; i++){
					Card card = deck.card [i];
					card.flagTurn = false;
					card.flagDrop=false;
					card.flagPull=false;
				}
				
				if (globV.MissedTouch){
					globV.MissedTouch=false;
					cset.selectedInt=-1;
				}
				if (cset.selectedInt!=-1)
					deck.card[cset.selectedInt].SetSelected();
			}
			else
				if (LoadedSuccess)
					LoadInitialState();
			else
			SetInitialState ();
		}
	}

	public void RecalCulateLives(){
		for (int i=0; i<3; i++){
			if (cset.LivesCounter > i){
			LivesDisplay [i].SetActive (true);
				deck.SparclyEffect.Fuel(LivesDisplay[i],100);
			}else
				LivesDisplay [i].SetActive (false);
			}
	}

	public void Reshuffle(){
		if (started == false)
						return;
		cset.LivesCounter -= 1;
		cset.CardsArranged = false;
		if (cset.LivesCounter < 0) {
			deck.MyMessageText.ShowMessage (" Game over, reshuffling... ");
			if (cset.PairsCounter>BestScore) {
				BestScore=cset.PairsCounter;
				PlayerPrefs.SetInt ("BestScoreUfukGame",cset.PairsCounter);	
				deck.MyMessageText.ShowMessage (" New best score: "+ cset.PairsCounter);
			}
			UpdateBestScore ();
			cset.LivesCounter=2;
			cset.PairsCounter=0;
			PairsDisplay.ChangeMyText(0+"/52");

		}

		RecalCulateLives ();
		deck.ShuffleCards (ShuffleTo,0);
		CardLeftCount.HideMe();
		initialState=false;
		MainStack = null;
		LoadedSuccess = false;
	}
	public void TurnOff(){
		if (cset.CardsArranged==true)
		Save ();
		started = false;
		gameObject.SetActive (false);

	}	

	void UpdateBestScore(){
		BestScoreText.ChangeMyText ("Best score: "+BestScore +"/52");
        try
        {
            IndiexpoAPI_WebGL.SendScore(BestScore);
        } catch ( Exception ex)
        {
            Debug.Log(ex.Message);
        }
	}


	public void StartTutorial(){

		tutorial.Init ();
		TurnOff ();
	}

	public UfuksTutorial tutorial;
	public void Init(){
		Tutorial=PlayerPrefs.GetInt("UfukTut");
		DeckObj = globV.DeckInstantiating;
		deck = DeckObj.GetComponent<DeckOf> ();

		cset.selectedInt = -1;
		cset.clearOther = -1;
		cset.Discard.first = -1;
		cset.Discard.count = 0;

		if (Tutorial==0) {
			StartTutorial();
			return; 	
				}


		cset.LivesCounter = 2;
		PairsDisplay.ShowMe ();
		BestScoreText.ShowMe ();

		gameObject.SetActive (true);
		HowToPlay.SetActive (true);


		ReshuffleButton.SetActive (true);
		started=true;
		initialState=false;
		MainStack = null;
		LoadedSuccess = false;
		cset.CardsArranged = false;
		Load ();
		BestScore=PlayerPrefs.GetInt ("BestScoreUfukGame");
	
		if (LoadedSuccess == false) {
						deck.ShuffleCards (ShuffleTo, 0);
				}
				else
						deck.ShuffleCards (SaveStartDeck, 0);

		UpdateBestScore ();
		PairsDisplay.ChangeMyText(cset.PairsCounter+"/52");
		RecalCulateLives ();
	}

	public void StackToStore(stackSaver ss){
	
		if ((ss.first != -1) && (deck.card [ss.first].Stacked)) {
			Stacker stc = deck.card [ss.first].stacker;
			ss.count = stc.cardsCount;
			for (int i=0; i<stc.cardsCount; i++)
				ss.cards[i] = stc.cards [i].GetComponent<Card> ().index;
		} else
			ss.count = 0;

	}

	public void Save(){

		if ((started == false) || (initialState == false))
						return; 
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create(Application.persistentDataPath + "/UfuksGameSave.dat");

		StackToStore(cset.Discard);
		StackToStore(cset.MStack);
		StackToStore(cset.Grab);

		bf.Serialize (file,cset);
		file.Close();
		deck.MyMessageText.ShowMessage (" Game saved ");
	}

	public bool LoadedSuccess;

	public void Load(){

		if (File.Exists (Application.persistentDataPath + "/UfuksGameSave.dat")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/UfuksGameSave.dat", FileMode.Open);
			cset = (CSet)bf.Deserialize (file);
			file.Close ();
			deck.MyMessageText.ShowMessage (" Game loaded ");
			WaitingForArrangement=false;
			if (cset.CardsArranged==false)
				LoadedSuccess=false;
			else
			LoadedSuccess=true;


		} 
	}


}
