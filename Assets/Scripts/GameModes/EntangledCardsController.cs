using UnityEngine;
using System.Collections;

public class EntangledCardsController : MonoBehaviour {

	public GameObject DeckObj;
	public DeckOf deck;
	private bool started=false;
	public GameObject LeftCameraPosition;
	public GameObject RightCameraPosition;
	public GameObject ShuffleTo;
	public GameObject SecondaryStack;
	public GameObject ChangeCameraButton;
	public GameObject AcesStack;
	public CameraTouchScreen MainCamera;
	public bool initialState;
	public Stacker stack;
	const int SideW=5;
	const int SideH=4;

	public void Init(){
		gameObject.SetActive (true);
		ChangeCameraButton.SetActive (true);
		DeckObj = globV.DeckInstantiating;
		deck = DeckObj.GetComponent<DeckOf> ();
		started=true;
		initialState = false;
		stack = null;
		deck.ShuffleCards (ShuffleTo,0);
		//MainCamera.MoveMe (SecondaryCamera);
		
	}
	[System.Serializable]
	public class MySide{
		public int[] ind = new int[SideH*SideW];
	
		public int[] other = new int[SideH*SideW];
	}

	[System.Serializable]
	public class MyTangl
		{
		public MySide[] side = new MySide[2];

		public int freeSpot0;
		public int freeSpot1;
		}

	public MyTangl tangl=new MyTangl();

	public void TurnOff(){
		started = false;
		gameObject.SetActive (false);
	}

	public void TryStackMe (Card card, int first, Vector3 coords){
	 if (first == -1) {
						first = card.index;
						card.MoveMeTo (coords, 0);
				} else
						card.StackMeTo (deck.cardObj [first]);
	}

	public bool TryGetOther(int value){
		if (tangl.freeSpot0 >= SideH * SideW)
						return false;
		value += 6;
		if (value >= 12)
						value -= 12;
		if (otherNeeded [value] != -1)
						return false;
		otherNeeded [value] = tangl.freeSpot0;
		return true;

	}

	public float getAng(int value){
		float angl=value%6;
		angl=angl*(Mathf.PI/6);
		return angl;
	}

	public int[] otherNeeded = new int[12]; 
	public int FirstInSecondaty;
	public int FirstAce;
	void SetInitial(){
		if ((stack != null) && (stack.cardsCount == 52)) {
			tangl.freeSpot0=0;
			tangl.freeSpot1=0;
			FirstInSecondaty=-1;
			FirstAce=-1;

			for (int i=0; i<12; i++) otherNeeded[i]=-1;
			for (int i=0; i<52; i++){
				Card card=stack.cards[i];
				if (card.value==12){
					TryStackMe ( card, FirstInSecondaty, SecondaryStack.transform.position);
				} else
					if (otherNeeded[card.value]!=-1){
					float angl=getAng(card.value);
					card.MoveMeTo(100+(tangl.freeSpot1 % SideW)*70,(tangl.freeSpot1/SideW)*70,-card.index*0.1f,angl);
					tangl.side[1].ind[tangl.freeSpot1]=card.index;
					tangl.side[1].other[tangl.freeSpot1]=otherNeeded[card.value];
					tangl.side[0].other[tangl.side[1].other[tangl.freeSpot1]]=tangl.freeSpot1;
					if (otherNeeded[card.value] % 2 == 1)
						card.FaceMe(true);
					tangl.freeSpot1+=1;
					otherNeeded[card.value]=-1;
					} else
				if (TryGetOther(card.value)) {
					tangl.side[0].ind[tangl.freeSpot0]=card.index;
					float angl=getAng(card.value);
					card.MoveMeTo(-370+(tangl.freeSpot0 % SideW)*70,(tangl.freeSpot0/SideW)*70,-card.index*0.1f,angl);
					if (tangl.freeSpot0 % 2 ==0)
						card.FaceMe(true);
					tangl.freeSpot0+=1;
				}else
					TryStackMe ( card, FirstInSecondaty, SecondaryStack.transform.position);
				

				//card.FaceMe(true);
				//card.MoveMeTo(-300+card.value*60, card.color*70, 0, 0);
			}

			initialState=true; 
		}else
			if (deck.card[0].Stacked==true)
				stack=deck.card[0].stacker;
	}

	// Update is called once per frame
	public int CurSide;
	void Update () {
		if (started) {
				if (initialState){

				MySide side=tangl.side[CurSide];
				//
			for (int i=0; i<SideW*SideH; i++)
				if ((side.ind[i]!=-1) && (side.other[i]!=-1))
				{
				Card card = deck.card [side.ind[i]];
				if (card.flagTurn) {
						CurSide=Mathf.Abs(CurSide-1);

						if (CurSide==0) MainCamera.MoveMe(LeftCameraPosition);
						else 
							MainCamera.MoveMe(RightCameraPosition);

						MySide otherSide=tangl.side[CurSide];
						Card otherCard=deck.card[otherSide.ind[side.other[i]]];
					card.FaceMe (!card.Faced);
					otherCard.FaceMe(!otherCard.Faced);
					card.flagTurn = false;

						for (int j=0; j<SideW*SideH; j++)
							if (otherSide.ind[j]!=-1)
						{
							Card ccard = deck.card [otherSide.ind[j]];
							ccard.flagDrop=false;
							ccard.flagPull=false;
							ccard.flagTurn=false;
						}

				}
				if (card.flagPull) {
					card.SetPull (true);
					card.Unstack ();
					card.flagPull = false;
				}
				if (card.flagDrop) {
					card.SetPull (false);
					if (card.WillStack > 0) {
						if (globV.GetMy2DDistance (deck.cardObj[i], card.WillStackTo) < card.StackDistance * 2)
							card.StackMeTo (card.WillStackTo);
					} else
							card.AssignZ();
					card.flagDrop=false;
				}
				
				}
			
			
			
			}else 
				SetInitial();
			
		}
	}
}
