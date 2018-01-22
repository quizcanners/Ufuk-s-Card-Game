using UnityEngine;
using System.Collections;

public class TapHolesGameController : MonoBehaviour {
	public CameraTouchScreen MainCamera;
	public GameObject CameraPosition;
	public GameObject DeckObj;
	public GameObject Center;
	public GameObject ShuffleTo;
	public DeckOf deck;
	private bool started=false;
	private bool initialState=false;
	public Stacker MainStack;
	public int[] holes1=new int[6];
	public int[] holes2=new int[6];
	public int[] holes = new int[12];
	public float[] coolOff = new float[12];
	public MySmartButtonScript ScoreTxt;
	public MySmartButtonScript TimeTxt;
	public MySmartButtonScript BestScoreText;

	public float StartTurnDelay;
	public float TurnDelay;
	private float TurnTimer;
	public float DifficultyIncrease;
	public int mode;
	public int score;
	private float modeTimer;
	public float MyGameTimer;
	public float DelayPercent;
	public float DelayStabilizer;
	public float MinDelay;

	public void Init(){
		gameObject.SetActive (true);
		gameEnded = false;
		score = 0;
		ScoreTxt.ShowMe ();
		ScoreTxt.ChangeMyText(" "+score);
		MyGameTimer=TimeToGame;
		TimeTxt.ShowMe ();
		TimeTxt.ChangeMyText (" "+MyGameTimer);
		BestScoreText.ShowMe ();
		BestScore=PlayerPrefs.GetInt("HoleTapper");
		BestScoreText.ChangeMyText ("Best score: "+BestScore);

		DeckObj = globV.DeckInstantiating;
		deck = DeckObj.GetComponent<DeckOf> ();
		deck.ShuffleCards (ShuffleTo,0);
		started=true;
		initialState = false;
		MainStack = null;
		MainCamera.MoveMe (CameraPosition);
	}
	
	public void TurnOff(){
		started = false;
		gameObject.SetActive (false);
	}

	void SetInitialState(){
		if ((MainStack != null) && (MainStack.cardsCount == 52)) {



			int ind=51;
			Vector3 tmp=Center.transform.position;
			tmp.x-=90*2;
			tmp.y+=69.3f;
			MainStack.AllignNbyAng(ref ind, ref holes1,6,tmp , 0, false, 65);

			tmp.y-=140f;
			MainStack.AllignNbyAng(ref ind, ref holes2,6,tmp , 0, false, 65);

			for (int i=0; i<6; i++){
				holes[i]=holes1[i];
				holes[i+6]=holes2[i];
			}

			MyGameTimer=TimeToGame;
			score=0;
			TurnDelay=StartTurnDelay;
			TurnTimer=3;
			initialState=true;
			DelayStabilizer=MinDelay-(MinDelay*DelayPercent);
			gameEnded=false;
		}
		else 
			if (deck.card[0].Stacked==true)
				MainStack=deck.card[0].stacker;
	}

	public int no=0;
	public int cnt;
	public float difficulty;
	public int[] rotaner = new int[6];
	private bool direction;
	private bool side;
	// Update is called once per frame


	void TurnCard(int i){
		Card card = deck.card [holes [i]];
		if (card.Faced==false) coolOff [i] = Time.time;
		card.FaceMe(true);
	}

	void Update () {
		if (started) {

			if ((initialState) && (!gameEnded)) {

				TurnTimer-=Time.deltaTime;
				MyGameTimer-=Time.deltaTime;
				modeTimer-=Time.deltaTime;
				if (TurnTimer<0){


					if (mode==0){
						int count=Random.Range(1,(int)difficulty);
						for (int i=0; i<count; i++){
							no=Random.Range(0,12);
							TurnCard(no);
						//deck.card[holes[no]].FaceMe(true);
						}
						TurnDelay*=DelayPercent;
						TurnDelay+=DelayStabilizer;
						difficulty+=DifficultyIncrease;
						difficulty=Mathf.Min(difficulty, 4f);
						TurnTimer=TurnDelay;
						if (modeTimer<0){
							modeTimer=2;
							mode=Random.Range(0,7);
							if (mode==2) {no=Random.Range(0,11);
								if (no<5) direction=true;
								else
									direction=false;
								modeTimer=2f;
							}
							if (mode==5){
								cnt=(Random.Range(0,3))+2;
								int gap=12/cnt;
								for (int i=0; i<cnt; i++){
								rotaner[i]=Random.Range(gap*i,gap+gap*i);
								}
								no=0;
								modeTimer=6;
							}
							if (mode==6){
									no=Random.Range(0,6);
									if (no>3) direction=true;
									else
										direction=false;
								}

							

						}
					}
					else{

						if (mode==1){
						int i=Random.Range(0,11);
							TurnCard(i);
							TurnCard(i+1);
						//deck.card[holes[i]].FaceMe(true);
						//deck.card[holes[i+1]].FaceMe(true);
							TurnTimer=TurnDelay;
					}
						else
							if (mode==2){
							if (direction){
								no++;
								if (no>11) no=0;
							}
							else{
								no--;
								if (no<0) no=11;
							}
							//deck.card[holes[no]].FaceMe(true);
							TurnCard(no);
						
							TurnTimer=0.3f;
						}
						else
							if (mode==3){
							int i=Random.Range(0,6);
							TurnCard(i);
							//deck.card[holes1[i]].FaceMe(true);
							i=Random.Range(0,7);
							if (i!=6)
								TurnCard(i+6);
							//deck.card[holes2[i]].FaceMe(true);
							TurnTimer=TurnDelay;
						}
						else
						if (mode==4){
							int i=Random.Range(0,6);
							TurnCard(i);
							TurnCard(i+6);
							//deck.card[holes1[i]].FaceMe(true);
							//deck.card[holes2[i]].FaceMe(true);
							TurnTimer=TurnDelay;
						}
						else
						if (mode==5){
							if (deck.card[holes[rotaner[no]]].Faced==false){
								no++;
								if (no>=cnt) no=0;
								TurnCard(rotaner[no]);
								//deck.card[holes[rotaner[no]]].FaceMe(true);
							
							}
							TurnTimer=0.002f;
						}
						else
						if (mode==6){

							if (direction){
								no++;
								if (no>5) no=0;
							} 
							else{
								no--;
								if (no<0) no=5;
							}

							if (side)
								TurnCard(no);
								//deck.card[holes1[no]].FaceMe(true);
							else
								TurnCard(no+6);
								//deck.card[holes2[no]].FaceMe(true);
							side=!side;
							TurnTimer=TurnDelay;
						}

						if (modeTimer<0){
							mode=0;
							modeTimer=3;
						}

					}

				}


			for (int i=0; i<12; i++){
				Card card = deck.card [holes[i]];
				if (card.flagPull) {
						if (card.Faced ){

							float scoreUp=Mathf.Min(100,10/(Time.time-coolOff[i]));
							//globV.MyAudio.PlayMySound(globV.MyAudio.NoteClips[i]);
							//deck.PlayBlingSound(Mathf.Min(scoreUp/pitchDivider,1),i);
							card.PlayMy(card.cutSounds);
							deck.SparclyEffect.Fuel(deck.cardObj[holes[i]].transform.position,scoreUp);
						card.FaceMe(false);
							score+=(int)scoreUp;
							if (score>ScoreToUnlockCardBack){
								deck.TryUnlockCardBack(3);
							}
						ScoreTxt.ChangeMyText(" "+score);
						MyGameTimer+=TurnDelay/2;
						}else
						{
							MyGameTimer-=1;
							TimeTxt.ChangeMyText("!!"+(int)MyGameTimer + " -1!!");
						}
					}
					card.flagPull=false;	
			}
			} else
				if ((!gameEnded) || (gameEnded && globV.hinter.flagTapped))
				SetInitialState();




			}
		}
	public float pitchDivider;
	private float TimeShow;
	public int BestScore;
	public bool gameEnded;
	public int TimeToGame=20;
	void FixedUpdate(){
		if ((started) && (initialState) && (!gameEnded)) {
						TimeShow -= Time.deltaTime;
						if (TimeShow < 0) {
								TimeShow = 1;
								TimeTxt.ChangeMyText (" " + (int)MyGameTimer);
				if (MyGameTimer<0){

					gameEnded=true;
					ScoreTxt.ChangeMyText(" "+score);
					MyGameTimer=TimeToGame;
					TimeTxt.ChangeMyText (" "+MyGameTimer);
					BestScoreText.ChangeMyText ("Best score: "+BestScore);
					deck.ShuffleCards (ShuffleTo,0);
					initialState = false;
					MainStack = null;

					if (score>BestScore){
						PlayerPrefs.SetInt("HoleTapper",score);
						BestScore=score;
					
						globV.hinter.CollapsableHintTo( "New Top Score: " + score,new Vector3( 0, 0 , -10));

					} else 
						globV.hinter.CollapsableHintTo( "Score: " + score,new Vector3( 0, 0 , -10));
					score=0;
					//globV.ExitCall=true;

				}
						}
				}
	}

	public int ScoreToUnlockCardBack;

	}

