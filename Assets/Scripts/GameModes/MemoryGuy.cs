using UnityEngine;
using System.Collections;

using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class MemoryGuy : MonoBehaviour {



	//[System.Serializable]
	public class MemGrid
	{
		public int[] dts = new int[52];
		public float[] dly= new float[52];
		public int[] downCards = new int[17];
		public Card[] downScripts = new Card[17];//h-1];
		public float downTime;
		public int   downCount;
		public int[] LastGrNo=new int[2];
		public int[] LastInd=new int[2];
		public int FirstDiscard;
		public int LastCardInStack;
		public int downClickedInd;
		public int downClickedNo;
		public int Difficulty;
		public int CardsCollected;
	} 

	static int w=7;
	static int h=4;

	public float MinDifficulty;
	public float MaxDifficulty;
	public float DifficultyRisePortion;
	public int stage;

	public MemGrid grid = new MemGrid();
	public GameObject DeckObj;
	public DeckOf deck;
	private bool started=false;
	//public GameObject DefCameraPosition;
	public GameObject CameraPos;
	public GameObject ShuffleTo;
	public GameObject LeftTop;
	public GameObject DiscardTo;
	public GameObject CutPile;
	public CameraTouchScreen MainCamera;
	public bool initialState;
	public Stacker stack;
	public float ShowDuration;
	//public MySmartButtonScript stageData;

	public void Init(){
		gameObject.SetActive (true);
		DeckObj = globV.DeckInstantiating;
		deck = DeckObj.GetComponent<DeckOf> ();
		started=true;
		MainCamera.MoveMe (CameraPos);
		deck.SparclyEffect.Fuel(Vector3.zero, 3);
		initialState = false;
		deck.ShuffleCards (ShuffleTo,0);
		stack = null;
		stage = globV.CurrentStageEd=PlayerPrefs.GetInt ("MemGuyStage");
			//PlayerPrefs.GetInt ("EditingStage");//
		//stageData.ShowMe ();
	//	stageData.ChangeMyText ("stage: " + stage);

	}
	
	public void Reshuffle(){
		stack = null;
		initialState = false;
		deck.ShuffleCards (ShuffleTo, 0);
	}
	
	public void TurnOff(){
		//MainCamera.MoveMe (DefCameraPosition);
		started = false;
		gameObject.SetActive (false);
	}

	StoredCard[] Arrangement;

	public int Load(){
	
		TextAsset asset = Resources.Load("MemGuy"+ "/" + stage) as TextAsset;
		//Debug.Log ("Loaded: "+asset);
		if (asset!=null){
	//	Stream s = new MemoryStream(asset.bytes);
		//BinaryReader br = new BinaryReader(s);


	//	string	 path = Application.dataPath + "/Stages/"+"MemGuy"+ "/" + stage + ".lr";

		//if (File.Exists (path)) {
			//Debug.Log ("path exists");
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream(asset.bytes);
			//FileStream file = File.Open (path, FileMode.Open);
			Arrangement = (StoredCard[])bf.Deserialize (ms);
		//	file.Close ();
			return Arrangement.Length;
		} 
		return 0;
	}

	void SetInitial(){
		if ((stack != null) && (stack.cardsCount == 52)) {
			int ind=51;
			grid.FirstDiscard = -1;

			for (int i=0; i<52; i++)
				grid.dts[i]=-1;
			int load=Load();
			if (load==0)
			for (int i=0; i<h; i++){
				Vector3 tmp=LeftTop.transform.position;
				tmp.y-=i*70;
				stack.AllignNbyAng(ref ind, ref grid.dts,i*w,w, tmp , 0, false,70);
			}
			else {
				if (load<40){
					int valMax=(load/4)+1;
					deck.CutByValue (stack, valMax, CutPile.transform.position,grid.FirstDiscard);
				}

				globV.deck.ArrangeStoredState (stack, ref Arrangement);
				for (int i=0; i<load; i++){
					grid.dts[i]=stack.cards[stack.cardsCount-i-1].index;
					grid.dly[i]=3;
				}
			}



			grid.LastInd[0]=-1;
			grid.LastInd[1]=-1;

			grid.LastCardInStack =stack.cards[0].index;
			grid.downClickedInd = -1;
			grid.downTime=0;
			grid.CardsCollected=0;
			deck.DropFlags();
			grid.downCount=0;
			initialState=true;
			arranging=true;
			downTimePerCard=MinDifficulty;
			for (int i=0; i<stage; i++)
				downTimePerCard-=(downTimePerCard-MaxDifficulty)*DifficultyRisePortion;




				}
		else 
			if (deck.card[0].Stacked==true)
				stack=deck.card[0].stacker;
	}

	void FinaliseDiscard (int ind){
		grid.CardsCollected += 1;
		//if (grid.LastCardInStack!=-1)	
			grid.downTime -= downTimePerCard/2;
		Card card = deck.card [ind];
		card.FaceMe (true);
		if (grid.FirstDiscard == -1) {
						grid.FirstDiscard = ind;
						card.MoveMeTo (DiscardTo.transform.position, 0);
				} else {
			card.StackMeToTop(deck.cardObj[grid.FirstDiscard]);		
		}


	}

	void DiscardGridCard (int no){
		if (grid.LastInd[0]!=-1) deck.card [grid.LastInd[0]].FaceMe (false);
		if (grid.LastInd[1]!=-1) deck.card [grid.LastInd[1]].FaceMe (false);
		grid.LastInd[0] = -1;
		grid.LastInd[1] = -1;
		int ind = grid.dts[no];
		grid.dts[no]=-1;
		FinaliseDiscard (ind);
	}
	public void DiscardDownCards(int no){
		FinaliseDiscard (grid.downScripts[no].index);
		grid.downCards [no] = -1;
		grid.downTime -= downTimePerCard;
	}
	
	public void ReorganizeDown(){
		for (int i=0; i<grid.downCount-1; i++) 
		if (grid.downCards [i] == -1) {
			grid.downCards[i]=grid.downCards[i+1];
			grid.downScripts[i]=grid.downScripts[i+1];
			grid.downCards[i+1]=-1;
		}
		grid.downCount--;

		if ((grid.downCount<=0) && (grid.LastCardInStack==-1)) {
			Reshuffle ();
			stage+=1;
			if (stage>=10)
				deck.TryUnlockCardBack(4);
			PlayerPrefs.SetInt ("MemGuyStage",stage);
		//	stageData.ChangeMyText ("stage: " + stage);
		}

	}
	void FlipGroup(int no){
		if (deck.card [grid.dts[no]].awake)
			return;
		int CardNo = 0;
		if (grid.LastInd [0] != -1) {
			CardNo++;
			if (grid.LastInd [1] != -1) {	
				CardNo=0;
				deck.card [grid.LastInd[0]].FaceMe (false);
				deck.card [grid.LastInd[1]].FaceMe (false);
				grid.LastInd [0]= -1;
				grid.LastInd [1]= -1;
				grid.downTime += downTimePerCard-(0.15f*downTimePerCard*grid.downCount);
			}
		}



		int ind=grid.dts[no];
		if ((CardNo==1) && (grid.LastInd[0]!=ind) && (deck.card[ind].value == deck.card[grid.LastInd[0]].value)) {
			DiscardGridCard(no);
			DiscardGridCard(grid.LastGrNo[0]);
			return;
		} else {
			grid.LastGrNo[CardNo] = no;
			grid.LastInd [CardNo] =  ind;
			if (TryDownNGridMatch(CardNo)) return;
		}



		ind=grid.dts[no];

			Card card = deck.card [ind];
			grid.dly[no]=ShowDuration;
			card.FaceMe (true);
	}
	// Update is called once per frame
	public float downTimePerCard;



	public bool TryDownNGridMatch(int CardNo){
		if ((grid.LastInd[CardNo]!=-1) && (grid.downClickedInd!=-1) && (grid.downScripts[grid.downClickedNo].value==deck.card[grid.LastInd[CardNo]].value)){
			DiscardGridCard (grid.LastGrNo[CardNo]);
			DiscardDownCards(grid.downClickedNo);
			ReorganizeDown();
			grid.downClickedNo=-1;
			grid.downClickedInd=-1;
			grid.LastInd[CardNo]=-1;
			return true;
		}
		return false;
	}

	public void ManageDown(){
		for (int i=0; i<grid.downCount; i++) {
			if (grid.downScripts[i].flagTurn){

				grid.downScripts[i].flagTurn=false;
				if (grid.downScripts[i].index == grid.downClickedInd)
					grid.downTime+=2;
				
				else
			    if ((grid.downClickedInd!=-1)  && (grid.downScripts[i].value==deck.card[grid.downClickedInd].value))
				{
					DiscardDownCards(i);
					DiscardDownCards(grid.downClickedNo);
					ReorganizeDown();
					ReorganizeDown();
					grid.downClickedNo=-1;
					grid.downClickedInd=-1;
					return;
				} else
				{
					grid.downClickedInd=grid.downScripts[i].index;
					grid.downClickedNo=i;
					if (TryDownNGridMatch(0)) return;
					if (TryDownNGridMatch(1)) return;
				}

			}

			grid.downScripts[i].MoveMeTo(new Vector3(ShuffleTo.transform.position.x + 
			                                         (grid.downTime/downTimePerCard - i+1)*90
			                                         , ShuffleTo.transform.position.y - 80 , ShuffleTo.transform.position.z), 0);
		}

		}

	public void SendCardDown(){
		int ind;
		if (stack != null) {
						ind= stack.cards [stack.cardsCount - 1].index;		
						if (stack.cardsCount < 3)
								stack = null;
				} else
		if (grid.LastCardInStack != -1) {
						ind = grid.LastCardInStack;		
						grid.LastCardInStack = -1;		
				} else
				return;

		Card card = deck.card [ind];
		grid.downScripts [grid.downCount] = card;
		grid.downCards [grid.downCount] = ind;	
		grid.downCount++;
	
		card.FaceMe (true);
	}
	public int MaxCardsDown;

	void TryLoose(){
		if (grid.downTime/downTimePerCard > MaxCardsDown) {
			Reshuffle ();
			if (grid.CardsCollected>10)
			stage=Mathf.Max(0,stage-1);
			PlayerPrefs.SetInt ("MemGuyStage",stage);
			//stageData.ChangeMyText ("stage: " + stage);
			globV.hinter.CollapsableHintTo(" Missed too many cards ", new Vector3( 0, 0, -200));
		}
	}

	bool arranging;
	void Update () {

	if (started) {
			if (initialState){
				if (arranging)
					if (deck.Arranging()) return;
				else if (!globV.hinter.gameObject.activeSelf)
				{arranging=false;
					deck.DropFlags();
				}
				else return;

					grid.downTime+=Time.deltaTime;
				if (grid.downTime<downTimePerCard) grid.downTime+=Time.deltaTime*(20-grid.downCount*2);

				if (grid.downTime/downTimePerCard>grid.downCount){
					SendCardDown();
					TryLoose();
				}
					ManageDown();
				// Grid management
				if (grid.LastInd[0]!=-1)
					deck.card[grid.LastInd[0]].SetSelected();
				if (grid.LastInd[1]!=-1)
					deck.card[grid.LastInd[1]].SetSelected();
				if (grid.downClickedInd!=-1)
					grid.downScripts[grid.downClickedNo].SetSelected();

				for (int i=0; i<52; i++){
					int ind;

					ind=grid.dts[i];
				
				if ((ind!=-1) && (deck.cardObj [ind].gameObject.activeSelf)) {
						Card card = deck.card [ind];
					if (card.Faced){
						grid.dly[i]-=Time.deltaTime;
						if (grid.dly[i]<0)
								card.FaceMe(false);
						}
					
				if (card.flagTurn) {
						card.PlayMy (card.flipSounds);
						card.flagTurn = false;

						if (globV.mainCamera.RaycastCard(ind))
						FlipGroup(i);
				}
			}
				}
			}
	else 
		SetInitial();
}

	}
}
