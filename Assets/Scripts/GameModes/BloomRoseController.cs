using UnityEngine;
using System.Collections;

public class BloomRoseController : MonoBehaviour {

	[System.Serializable]
	public class peddle{
		public int index;
		public Vector3 coordinates;
		public float rotation;
	}

	[System.Serializable]
	public class BloomRose
		{
		public peddle   central;
		public peddle[] around = new peddle[4];
		public peddle[] setAround = new peddle[4];
		public peddle[] outer  = new peddle[8];
		public peddle[] setOuter = new peddle[8];
		public bool CardsArranged;
	} 

	public BloomRose rose;
	

	public GameObject DeckObj;
	public DeckOf deck;
	private bool started=false;
	public GameObject CentralCard;
	public Stacker stack;
	public float aroundDistance;
	public float outerDistance;
	public float setOuterDistance;
	public GameObject ShuffleTo;
	public GameObject DiscardDeck;
	public GameObject HowToPlay;
	public MySmartButtonScript ScoreCount;
	public MySmartButtonScript BestScoreAndCombo;
	public MySmartButtonScript CardsLeft;
	public MySmartButtonScript CurrentCombo;
	public int combo;
	public int score;
	public int BESTcombo;
	public int BESTscore;
	private int FirstInDiscard;
	public bool initialState;
	float bloomDelay;
	Card NextCard;
	bool[] repeats = new bool[13];
	bool FullSpeedGrow=true;

	void updateBestText(){
		BestScoreAndCombo.ChangeMyText ("Best score: " + BESTscore + " Best combo: " + BESTcombo);
		CurrentCombo.ChangeMyText ("Combo: " + combo);
	}

	bool shuffleInit;
	float shuffleDelay;
	void Reshuffle(float delay){
		rose.CardsArranged = false;
		shuffleDelay = delay;
		if (delay > 0) {
			shuffleInit=true;
			shuffleDelay-=Time.deltaTime;
			return;
		}
		shuffleInit = false;
		BESTcombo=PlayerPrefs.GetInt("brBestCombo");
		BESTscore=PlayerPrefs.GetInt("BRbestScore");
		score = 0;
		combo = 0;
		updateBestText ();
		initialState = false;
		deck.ShuffleCards (ShuffleTo,0);
		FirstInDiscard = -1;
		stack = null;
		NextCard = null;
		initialRose = false;
		}

	public void Init(){
		ScoreCount.ShowMe ();
		BestScoreAndCombo.ShowMe ();
		CurrentCombo.ShowMe ();

		gameObject.SetActive (true);
		HowToPlay.SetActive (true);
		DeckObj = globV.DeckInstantiating;
		deck = DeckObj.GetComponent<DeckOf> ();
		started=true;
		Reshuffle (0);
		deck.DropFlags ();
	
	}
	
	public void TurnOff(){
		started = false;
		gameObject.SetActive (false);
	}




	void SetInitial(){
		if ((stack != null) && (stack.cardsCount == 52)) {
			rose.central.index=-1;
			rose.central.coordinates=new Vector3(CentralCard.transform.position.x,CentralCard.transform.position.y,
			                                     CentralCard.transform.position.z-3);
			rose.central.rotation=0; 
			for (int i=0; i<4; i++){

				rose.setAround[i].rotation=-i*Mathf.PI/2;
				Vector3 tmp=globV.displace(CentralCard.transform.position, aroundDistance, rose.setAround[i].rotation);
				rose.setAround[i].coordinates=new Vector3(tmp.x,tmp.y,tmp.z-2.2f);
				rose.setAround[i].index=-1;

				tmp=globV.displace(tmp, setOuterDistance, rose.setAround[i].rotation);
				rose.setOuter[i*2].coordinates=new Vector3(tmp.x, tmp.y,tmp.z-0.3f); ;
				rose.setOuter[i*2].rotation=rose.setAround[i].rotation;
				rose.setOuter[i*2].index=-1;

				rose.around[i].rotation=-Mathf.PI/4-i*Mathf.PI/2;
				tmp=globV.displace(CentralCard.transform.position, aroundDistance, rose.around[i].rotation);
				rose.around[i].coordinates=new Vector3(tmp.x,tmp.y,tmp.z-2);
				rose.around[i].index=-1;



				Vector3 tmp2=globV.displace(tmp, setOuterDistance, rose.around[i].rotation);

				rose.setOuter[i*2+1].coordinates=new Vector3(tmp2.x, tmp2.y,tmp2.z-0.2f); 
				rose.setOuter[i*2+1].rotation=rose.around[i].rotation-Mathf.PI/2;
				rose.setOuter[i*2+1].index=-1;

				tmp=globV.displace(tmp, outerDistance, rose.around[i].rotation);

				for (int j=1; j>=0; j--){
					float tmpAng=rose.around[i].rotation-Mathf.PI/4 + (j==1 ? 0 : Mathf.PI/2);
					Vector3 tmp3=globV.displace(tmp, aroundDistance, tmpAng);
					int dim=i*2 + j;
					rose.outer[dim].coordinates=tmp3;
					rose.outer[dim].rotation=tmpAng;
					rose.outer[dim].index=-1;
				}
				rose.around[i].rotation-=Mathf.PI/2;

			}
		
			initialState=true;
			rose.CardsArranged=false;
			deck.DropFlags();
			LastCard=stack.cards[0].index;
		}
			else
			if (deck.card[0].Stacked==true)
				stack=deck.card[0].stacker;
	
	}

	bool processLeaf(peddle spot, peddle prevSpot){
		if ((spot.index == -1) && (prevSpot.index != -1)) {
			spot.index=prevSpot.index;
			prevSpot.index=-1;		
			Card card=deck.card[spot.index];
			card.MoveMeTo(spot.coordinates, spot.rotation);
			return true;
		}
		return false;
	}

	bool processCentralPeddle(peddle spot){
		if ((spot.index == -1) && ((NextCard==null) || (NextCard.index!=LastCard))) {
			Card card=stack.cards[stack.cardsCount-1];
			card.FaceMe(true);
			card.MoveMeTo(spot.coordinates, spot.rotation);
			spot.index=card.index;
			if (((NextCard==null) || (NextCard.index!=LastCard)) && (stack.cardsCount>1)){
			NextCard=stack.cards[stack.cardsCount-2];
			NextCard.FaceMe(true);
			NextCard.flagPull=NextCard.flagDrop=false;
			} else
				if (LastCard!=-1)
				NextCard=deck.card[LastCard];
			return true;
		}
		return false;
	}


	peddle OuterGet(int i){
		if (i > 15)	i -= 16;
		if (i < 0)	i += 16;
		if (i % 2 != 0)
			return rose.outer [i / 2];
		else
			return rose.setOuter [i / 2];
	}

	peddle AroundGet(int i){
		if (i > 7)	i -= 8;
		if (i < 0)	i += 8;
		if (i % 2 != 0)
						return rose.around [i / 2];
				else
						return rose.setAround [i / 2];
	}

	void clearPeddle(bool around, int i){
		peddle tmp;
		if (around) tmp = AroundGet (i);
				 else  tmp = OuterGet (i);

		Card card = deck.card [tmp.index]; 
			tmp.index = -1;
			if (FirstInDiscard==-1){
				FirstInDiscard=card.index;
			card.MoveMeTo (DiscardDeck.transform.position, 0);
			}
			else
				card.StackMeTo(deck.cardObj[FirstInDiscard]);
		card.PlayMy(card.cutSounds);
		
	}


	int[] GetOuterValList(int first, int count){
		int[] tmp = new int[count];
		for (int i=0; i<count; i++)
			tmp [i] = deck.card [OuterGet (first + i).index].value;
		return tmp;
	}

	int[] GetAroundValList(int first, int count){
		int[] tmp = new int[count];
		for (int i=0; i<count; i++)
						tmp [i] = deck.card [AroundGet (first + i).index].value;

		return tmp;
	}

	bool checkAround(){
				int maxInd = -1;
				int maxConsecutive = -1;
		bool AnyFree = false;
				for (int i=0; i<8; i++){
			if (AroundGet(i).index==-1) AnyFree=true;

					if ((AroundGet (i).index != -1) &&
			    		(AroundGet (i + 1).index != -1) &&
			    		(AroundGet (i + 2).index != -1)) {
								int count = 2;
								while ((AroundGet(i+count).index!=-1) && (count<7)) {

					if 	(ConsecutivityCheck(GetAroundValList(i, count+1)) && (count+1 > maxConsecutive)) {
												maxConsecutive = count + 1;
												maxInd = i;
										}
										count++;
								}
						}
		}
		if (maxConsecutive != -1) {
						for (int li=0; li<maxConsecutive; li++)
								clearPeddle (true, maxInd + li);
			score+=maxConsecutive*maxConsecutive;
			combo+=maxConsecutive;
				}
				else {

			for (int i=0; i<8; i++)
				if ((AroundGet(i).index!=-1) &&  (AroundGet(i+1).index!=-1) && (AroundGet(i+2).index!=-1))
			{
			Card one=deck.card[AroundGet(i).index];
			Card two=deck.card[AroundGet(i+1).index];
			Card tree=deck.card[AroundGet(i+2).index];
			if ((one.value==two.value) && (two.value==tree.value)){
					bool four=false;
					if ((AroundGet(i-1).index!=-1) && (deck.card[AroundGet(i-1).index].value==one.value)) {
						clearPeddle(true, i-1);four=true;}
					if ((AroundGet(i+3).index!=-1) && (deck.card[AroundGet(i+3).index].value==one.value)) {
						clearPeddle(true, i+3); four=true;}
				for (int li=0; li<3; li++) clearPeddle(true, i+li);
					score+=3+(four ? 9 : 0);
					combo+=3+(four ? 1 : 0);
					updateScoreNcombo();
					return AnyFree;
			}
			}
		}
		return AnyFree;
	}

	void updateScoreNcombo(){
		ScoreCount.ChangeMyText ("score: " + score);
		CurrentCombo.ChangeMyText ("combo: " + combo);
	}

	bool checkOuter(){
		int maxInd = -1;
		int maxConsecutive = -1;
		bool AnyFree = false;
		for (int i=0; i<16; i++){
			if (OuterGet (i).index ==-1) AnyFree = true;


			if ((OuterGet (i).index != -1) &&
			    (OuterGet (i + 1).index != -1) &&
			    (OuterGet (i + 2).index != -1)) {
				int count = 2;
				while ((OuterGet(i+count).index!=-1) && (count<15)) {
					
					if (ConsecutivityCheck(GetOuterValList(i, count+1)) && (count+1 > maxConsecutive)) {
						maxConsecutive = count + 1;
						maxInd = i;
					}
					count++;
				}
			}
		}
		if (maxConsecutive != -1) {
			for (int li=0; li<maxConsecutive; li++)
				clearPeddle (false, maxInd + li);
			score+=maxConsecutive*maxConsecutive;
			combo+=maxConsecutive;
		}
		else {
			
			for (int i=0; i<16; i++)
				if ((OuterGet(i).index!=-1) &&  (OuterGet(i+1).index!=-1) && (OuterGet(i+2).index!=-1))
			{
				Card one=deck.card[OuterGet(i).index];
				Card two=deck.card[OuterGet(i+1).index];
				Card tree=deck.card[OuterGet(i+2).index];
				if ((one.value==two.value) && (two.value==tree.value)){
					bool four=false;
					if ((OuterGet(i-1).index!=-1) && (deck.card[OuterGet(i-1).index].value==one.value)) {
						clearPeddle(true, i-1); four=true;}
					if ((OuterGet(i+3).index!=-1) && (deck.card[OuterGet(i+3).index].value==one.value)) {
						clearPeddle(true, i+3); four=true;}
					for (int li=0; li<3; li++) clearPeddle(false, i+li);

					score+=3+(four ? 9 : 0);
					combo+=3+(four ? 1 : 0);
					updateScoreNcombo();
					return AnyFree;
				}
			}
		}
		return AnyFree;
	}




	bool ConsecutivityCheck(int[] values){
		bool AcePresent = false;
		for (int i=0; i<13; i++)
						repeats [i] = false;

		for (int i=0; i<values.Length; i++) {
			if (repeats[values[i]]==true) return false;
			repeats[values[i]]=true;
						if (values [i] == 12)
								AcePresent = true;
				}

		if ((AcePresent) && (values.Length<13)) {
			bool linked=true;
			int val=0;
			while ((linked) && (val<13)){
				linked=false;
				for (int i=0; i<values.Length; i++)
					if (values[i]==val){
					values[i]+=13;
						linked=true;
					i=values.Length;
				}
				val+=1;
			}
		}
		int min=Mathf.Min( values);
		int max=Mathf.Max( values);
		if ((max - min) == values.Length - 1) {
		//	deck.MyMessageText.ShowMessage(values.Length+" values: ");
		//	for (int i=0; i<values.Length; i++)
		//	deck.MyMessageText.ShowMessage(" "+values[i]);
						return true;
				}
		return false;
	}
	public bool initialRose;
	public bool CardPlaced;


	void Update () {
		if (started) {
			if (shuffleInit)
				Reshuffle(shuffleDelay);
			else
			if (initialState){


				bloomDelay-=Time.deltaTime;
				bool updated=false;
				if ((bloomDelay<0) && ((initialRose==false) || (CardPlaced==true) || (FullSpeedGrow))){
					CardPlaced=false;
					bool thisBloom=false;
				for (int i=0; i<4; i++){
						if (processLeaf(rose.outer[i*2], rose.around[i]) ||
						    processLeaf(rose.outer[i*2+1], rose.around[i]))
							thisBloom=true;
						    if (processLeaf(rose.around[i], rose.central)) 
							thisBloom=true;
						if (thisBloom)
							break;
				}

					if (thisBloom){
						updated=true;
					
					}

				if ((processCentralPeddle(rose.central)==false) && (thisBloom==false))
						initialRose=true;
					else 
						initialRose=false;
					bloomDelay=0.5f;
				}


				if (LastCard!=-1){

					if ((NextCard!=null) && (NextCard.flagPull) && (initialRose)) {
					NextCard.SetPull (true);
					NextCard.Unstack ();
				}
					if (NextCard!=null)
					NextCard.flagPull=false;

					if ((NextCard!=null) &&(NextCard.flagDrop)) {

					NextCard.flagDrop=false;
					NextCard.SetPull (false);
						updated=true;
					
					float dist=999;//=Vector3.Distance(transform.position,WillStackTo.transform.position);

					peddle nearest=null;
					for(int i=0; i<4; i++)
					if (rose.setAround[i].index==-1)
					{
						float tmp=Vector3.Distance(rose.setAround[i].coordinates, NextCard.transform.position);
						if (tmp<dist){
							dist=tmp;
							nearest=rose.setAround[i];
						}
						    }

					for(int i=0; i<8; i++)
					if (rose.setOuter[i].index==-1)
					{
						float tmp=Vector3.Distance(rose.setOuter[i].coordinates, NextCard.transform.position);
						if (tmp<dist){
							dist=tmp;
							nearest=rose.setOuter[i];
						}
					}

if ((nearest!=null) && (dist<StackDistance)){

							CardPlaced=true;
							if (combo>BESTcombo){
								BESTcombo=combo;
								PlayerPrefs.SetInt("brBestCombo", combo);
								deck.MyMessageText.ShowMessage (" New high combo: " + combo+ " cards ");
								updateBestText();
							}
							combo=0;

							NextCard.PlayMy(NextCard.tapSounds);
					NextCard.MoveMeTo(nearest.coordinates, nearest.rotation);
	
					nearest.index=NextCard.index;
					if (NextCard.index!=LastCard){

							if (deck.card[LastCard].Stacked){
					NextCard=stack.cards[stack.cardsCount-1];
					NextCard.FaceMe(true);
					NextCard.flagPull=NextCard.flagDrop=false;
						} else
							{
								NextCard=deck.card[LastCard];
								NextCard.FaceMe(true);
								NextCard.flagPull=NextCard.flagDrop=false;
							}

						} else
						{
							LastCard=-1;
							NextCard=null;
								if (combo>BESTcombo){
									BESTcombo=combo;
									PlayerPrefs.SetInt("brBestCombo", combo);
									deck.MyMessageText.ShowMessage (" New high combo: " + combo+ " cards ");
									updateBestText();
								}

							
						}


						updated=true;
						
					} else {
						if (NextCard.index==LastCard){
							NextCard.MoveMeTo(ShuffleTo.transform.position,0);
								NextCard.FaceMe(true);
						}
							else
						{

								NextCard.StackMeTo(stack.cards[stack.cardsCount-1]);//deck.cardObj[LastCard]);
								NextCard.FaceMe(true);
						}
					}
				}
			}
				else{	
					if (score>BESTscore){
						BESTscore=score;
						
						PlayerPrefs.SetInt("BRbestScore", score );
						updateBestText();
						
					}
					
					Reshuffle (3);
					return;
				}
			
				if (updated){
					bool AnyFree=checkAround();
					AnyFree=checkOuter() ? true : AnyFree;
					if (AnyFree==false)
					{
						if (score>BESTscore){
							BESTscore=score;
							deck.MyMessageText.ShowMessage (" New BEST SCORE! "+ score);
							PlayerPrefs.SetInt("BRbestScore", score );
							updateBestText();
							
						}else
							deck.MyMessageText.ShowMessage (" No more moves ");
						
						Reshuffle (3);
						return;
					}
					if (score>scoreToUnlockCardBack)
						deck.TryUnlockCardBack(2);
						ScoreCount.ChangeMyText ("score: " + score);
					CurrentCombo.ChangeMyText ("combo: " + combo);
					updated=false;
				}

			}
			else 
				SetInitial();

		}
	}
	public int scoreToUnlockCardBack;
	public float StackDistance;
	public int LastCard;
}
