using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[Serializable]
public class MySaveData
{
	public bool[] cardBacks;
	public int SelectedBack;
}

[Serializable]
public class StoredCard
{
	public float x,y,z;
	public float ang;
	public bool face;
}

public static class globV{
	public static GlobT Tt;
	public static AllignmentArrow Arrow;
	public static bool drugging=false;
	public static int ColorNo=0; 
	public static int ValueNo=0;
	public static float StackCardThick = 0.9f;
	public static float stackToSpeed=600f;
	public static GameObject DeckInstantiating;
	public static float UnClipDistance=0.1f;
	public static bool MissedTouch=true;
	public static bool ExitCall=false;
	public static MySaveData mySave = new MySaveData();
	public static DeckOf deck;
	public static audioController MyAudio;
	public static CameraTouchScreen mainCamera;
	public static HintControls hinter;
	public static bool wantMusic;
	public static bool wantSounds;
	public static bool wantHD;
	public static bool orientation;
	public static Vector3 mousePos;
	public static int CurrentStageEd;
	public static bool PendingEditorArrangement;
	public static bool ArrowWay;
	public static bool DevBuild=false;

	public static float PiToGrad(float MyPI){
		return (MyPI/(Mathf.PI*2)*360f);
	}

	public static float AngDist (float ang1, float ang2){
		float dist = Mathf.DeltaAngle (PiToGrad(ang1), PiToGrad(ang2));
		return dist;
	}

	public static void TurnAngUp (ref float ang){
		if (Mathf.Abs (AngDist (ang, Mathf.PI)) <= 45)
						ang += Mathf.PI;
	}

    public static float GetMy2DDistance(Card From, Card To)
    {
        Vector2 from = new Vector2(From.transform.position.x, From.transform.position.y);
        Vector2 to = new Vector2(To.transform.position.x, To.transform.position.y);
        return (Vector2.Distance(from, to));
    }

    public static float GetMy2DDistance(Vector3 From, Vector3 To){
		Vector2 from=new Vector2(From.x,From.y);
		Vector2 to=new Vector2(To.x,To.y);
		return (Vector2.Distance(from, to));
	}

	public static float GetMy2DDistance(GameObject From, GameObject To){
		Vector2 from=new Vector2(From.transform.position.x,From.transform.position.y);
		Vector2 to=new Vector2(To.transform.position.x,To.transform.position.y);
	//	float Zdif = Mathf.Abs(From.transform.position.z - To.transform.position.z);
		return (Vector2.Distance(from, to));
	}

	public static Vector3 SetPullOff(Vector2 pos){
		Ray ray;
		RaycastHit rayhit;
		ray = Camera.main.ScreenPointToRay (pos);
		Physics.Raycast (ray, out rayhit);
		return rayhit.point;//mousePosDis = rayhit.point - transform.position;
	}

	public static Vector3 SetPullOff(Vector3 pos){
		Ray ray;
		RaycastHit rayhit;
		ray = Camera.main.ScreenPointToRay (pos);
		Physics.Raycast (ray, out rayhit);
		return rayhit.point;//mousePosDis = rayhit.point - transform.position;
	}

	public static float FuckenAngle(Vector3 one){
		Vector3 dickAss = new Vector3 (0, 1, 0);
		float ang = Vector2.Angle(one, dickAss);
		Vector3 cross = Vector3.Cross(one, dickAss);
		if (cross.z > 0)
			ang = 360 - ang;
		ang = ang * Mathf.PI / 180;
		return ang;
	}

	public static Vector3 displace(Vector3 from, float byDist, float inAngDir){
		return new Vector3 (from.x - byDist * Mathf.Sin (inAngDir),
		                    from.y + byDist * Mathf.Cos (inAngDir),
		                    from.z);
	}
}

public class DeckOf : MonoBehaviour {
	public Texture[] CardBacksTextures;
	public Texture[] CardFrontTextures;
	public GameObject OriginalCard;
	public Card[] card;
	public int[] CardOrdering=new int[52];
	public Card[] cardObj;
	public DefaultGameController defController;
	bool shuffling;
	private GameObject ShufflingTo;
	private float shuffleToAng;
	public AudioClip[] GroundPunchNoise;
	public AudioClip[] BlingSound;
	public AudioClip[] ShufStart;
	public AudioClip[] ShufEnd;
	public EffectDie SparclyEffect;
	public EffectDie WindyEffect;
	public MySmartButtonScript MyMessageText;
	public Rect CardConstrains;
	public CardShadower shadow;
	float shuffleDelay;
	public Sprite[] OrientationSprites;
	public Sprite[] ArrowSprites;
	public GameObject OrientationToggler;
	public GameObject ArrowToggler;
	// Use this for initialization


	public int CutByValue (Stacker stack, int MaxVal, Vector3 To, int first){
		for (int i=0; i<stack.cardsCount; i++) {
			Card card;
			card=stack.cards[i];
			if (card.value>MaxVal){
				if (ForceUnstackCard(card.index))
				i--;
				if (first==-1){
					first=card.index;
					card.MoveMeTo(To,0);
				}else card.StackMeTo(cardObj[first]);
			}
		}
		stack.ArrangeYourCards ();
		return first;
	}

	public void ToggleArrow(){
		globV.ArrowWay = !globV.ArrowWay;
		PlayerPrefs.SetInt ("ArrowWay", globV.ArrowWay ? 1 : 0);
		ArrowToggler.GetComponent<Image> ().sprite = ArrowSprites [globV.ArrowWay ? 1 : 0];
	}

	public void ToggleOrientation(){
		globV.orientation = !globV.orientation;
		PlayerPrefs.SetInt ("Orientation", globV.orientation ? 1 : 0);
		OrientationToggler.GetComponent<Image> ().sprite = OrientationSprites [globV.orientation ? 1 : 0];
	}

	public void ArrangeStoredState(Stacker stack, ref StoredCard[] arrange){
		if (arrange.Length > stack.cardsCount)
						return; 
		for (int i=0; i<arrange.Length; i++) {
			Card card=stack.cards[stack.cardsCount-1-i];
			card.MoveMeTo(arrange[i].x, arrange[i].y, arrange[i].z, arrange[i].ang);
			card.FaceMe(arrange[i].face);
			card.GoToRotation=arrange[i].ang;
		}
	}

	public void FillUnstacked(ref StoredCard[] fill  ){
		int[] IndByOrder = new int[52];
		for (int i=0; i<52; i++) 
			IndByOrder[CardOrdering[i]]=i;		
		

		int no = 0;
		for (int i=0; i<52; i++) 
						if (!card [IndByOrder [i]].Stacked) {
								fill [no] = new StoredCard ();
								Vector3 tmp = cardObj [IndByOrder [i]].transform.position;
								fill [no].x = tmp.x;
								fill [no].y = tmp.y;
								fill [no].z = tmp.z;
								fill [no].face = card [IndByOrder [i]].Faced;
								fill [no].ang = card [IndByOrder [i]].GoToRotation;
								no++;
						}
		}

	public int CountUnstacked(){
		int count = 0;
		for (int i=0; i<52; i++)
						if (!card [i].Stacked)
								count++;
		return count;
	}

	public void ShadowCard(int index){
			//card//card
		shadow.ShadowCard (cardObj [index]);
		}

	public bool ForceUnstackCard(int ind){
		Card mycard = card [ind];
		if ((mycard.Stacked) && (cardObj[ind].gameObject.activeSelf==false)) {
			Stacker stacker=mycard.stacker;
			int pos=0;

			for (int i=0; i<stacker.cardsCount; i++){
				if (stacker.cards[i]==cardObj[ind]){
					pos=i; 
					Card up=stacker.cards[i+1];
					if (i!=0){
						up.PlayMy(up.cutSounds);
						up.StackedTo=stacker.cards[i-1];
						up.otherC=stacker.cards[i-1];
					} else
					{
						up.StackedTo=null;
						up.otherC=null;
					}
					i=52;
			}
			}
				for (int i=pos+1; i<stacker.cardsCount; i++){
					stacker.cards[i-1]=stacker.cards[i];	
				}
			stacker.cardsCount-=1;
			stacker.updated=true;
			mycard.Stacked=false;
			cardObj[ind].gameObject.SetActive(true);

			mycard=stacker.cards[stacker.cardsCount-1];
			mycard.Z=-stacker.cardsCount*globV.StackCardThick;
			return true;
			//mycard.Unstack();
		}
		return false;
	}

	public void OrderCardTo (int ind, int h){
		int was = CardOrdering [ind]; 
		CardOrdering [ind] = h;
		if (h > was) 
		for (int i=0; i<52; i++){
			if ((CardOrdering [i] >was) && (CardOrdering [i] <=h)){
				CardOrdering [i] --;
			//if (card[i].Stacked==false)
				//card[i].AssignZ();		
			}
		}
		else
		for (int i=0; i<52; i++){
			if ((CardOrdering [i] <was) && (CardOrdering [i] >=h)){
				CardOrdering [i] ++;
			//	if (card[i].Stacked==false)
				//	card[i].AssignZ();		
			}
		}
	}

	public void OrderCardDown(int ind){
		int was = CardOrdering [ind]; 
		CardOrdering [ind] = 0;
		for (int i=0; i<52; i++){
			if ((CardOrdering [i] < was) && (i != ind)) 
				CardOrdering [i] += 1;
			if (card[i].Stacked==false)
				card[i].AssignZ();		
		}
	}

	public void StackUnstacked(GameObject to){
		int bottom=0;
		for (int i=0; i< 52; i++)
		if ((card [i].Stacked == false) && (card [i].StackStarted == false) && (card[i].GoingToAPlace==false)) {
			bottom=i;
			card[i].MoveMeTo(to.transform.position, 0);
			i=52;
		}
		for (int i=0; i< 52; i++)
			if ((card [i].Stacked == false) && (i!=bottom) && (card [i].StackStarted == false) && (card[i].GoingToAPlace==false)) 
				card[i].StackMeTo(cardObj[bottom]);
	}

	public void StackUnactive(GameObject to){
		int bottom=0;
		for (int i=0; i< 52; i++)
						if ((card [i].StackStarted == false) && (card[i].GoingToAPlace==false)) {
			bottom=i;
			card[i].MoveMeTo(to.transform.position, 0);
			i=52;
		}
		for (int i=0; i< 52; i++)
			if ((i!=bottom) && (card [i].StackStarted == false) && (card[i].GoingToAPlace==false)) 
				card[i].StackMeTo(cardObj[bottom]);
	}

	public void OrderCardUp(int ind){
		int was = CardOrdering [ind]; 
		CardOrdering [ind] = 51;
		for (int i=0; i<52; i++){
						if ((CardOrdering [i] > was) && (i != ind)) 
								CardOrdering [i] -= 1;
			if (card[i].Stacked==false)
				card[i].AssignZ();		
		}
		}

	public void ConstrainCard(int index){
		GameObject cardo = cardObj [index].gameObject;
		if ((cardo.transform.position.x < CardConstrains.xMin) ||
						(cardo.transform.position.x > CardConstrains.xMax) ||
						(cardo.transform.position.y < CardConstrains.yMin) ||
						(cardo.transform.position.y > CardConstrains.yMax)) 

			{
			cardo.transform.position=new Vector3 (
				Mathf.Max (Mathf.Min (cardo.transform.position.x, CardConstrains.xMax), CardConstrains.xMin),
				Mathf.Max (Mathf.Min (cardo.transform.position.y, CardConstrains.yMax), CardConstrains.yMin),
				cardo.transform.position.z);
				}

	}

	public void PlayMy(AudioClip[] toPlay){
		if (!globV.wantSounds)
			return;
		GetComponent<AudioSource>().clip = toPlay[UnityEngine.Random.Range(0,toPlay.Length)];
		GetComponent<AudioSource>().Play ();
	}



	public void DropFlags(){
		for (int i=0; i<52; i++) {
			card[i].flagPull=false;
			card[i].flagDrop=false;
			card[i].flagTurn=false;
				}
	}

	public bool Arranging(){
		for (int i=0; i<52; i++)
			if ((card[i].GoingToAPlace) || (card[i].StackStarted))
				return true;
		return false;
	}

	public void PlayBlingSound(float pitch, int no){
		if (!globV.wantSounds)
			return;
		int i;
		if (no<0)
						i = UnityEngine.Random.Range (0, BlingSound.Length);
				else
						i = Mathf.Min (no, BlingSound.Length - 1);
		loud.clip = BlingSound[i];
		loud.pitch = pitch;
		loud.Play ();
	}

	public void PlayMissSound(){
		if (!globV.wantSounds)
			return;
		int i = UnityEngine.Random.Range (0,GroundPunchNoise.Length);
		quiet.clip = GroundPunchNoise[i];
		quiet.Play ();
		globV.MissedTouch = true;
	}

	public void ShuffleCards(GameObject To, float ang){
		PlayMy(ShufEnd);
		ShufflingTo = To;
		shuffleToAng = ang;
			shuffling = true;
			for (int i=0; i<52; i++){
			Vector2 newPosition = UnityEngine.Random.insideUnitCircle * 300;
				card [i].MoveMeTo(newPosition.x, newPosition.y,
			                  -i*globV.UnClipDistance ,UnityEngine.Random.Range (0, Mathf.PI*2f*100f)/100f);
				if (card[i].Faced!=false) card[i].FaceMe(false);
			}
			shuffleDelay=1;
			return;
			
	}

	public void TryUnlockCardBack(int i){
		if (globV.mySave.cardBacks[i]==false){
			globV.mySave.SelectedBack = i;
			ApplyCardBack ();
			globV.mySave.cardBacks[i]=true;
			Save();
			MyMessageText.ShowMessage(" !You have unlocked a new card back! Well done. ");
		}
		}

	public void TryChangeCarBack(int i){
		if (globV.mySave.cardBacks [i] == true) {
			globV.mySave.SelectedBack=i;
			ApplyCardBack();
			Save();
		}
	}

	public void ApplyCardFDront(int i){
		PlayerPrefs.SetInt ("Face", i);
		//for (int i=0; i<52; i++)
		card[0].face.GetComponent<Renderer>().sharedMaterial.mainTexture = CardFrontTextures[i];
		//if (i==1)
		//	card[0].face.GetComponent<Renderer>().sharedMaterial.shader=Shader.Find("Mobile/Diffuse");
		//else 
		//	card[0].face.GetComponent<Renderer>().sharedMaterial.shader=Shader.Find("Transparent/Cutout/Diffuse");
		//	shader1 = Shader.Find("Diffuse");
		//shader2 = Shader.Find("Transparent/Diffuse");
		
	}

	public void ApplyCardBack(){
		//for (int i=0; i<52; i++)
						cardObj [0].GetComponent<Renderer>().sharedMaterial.mainTexture = CardBacksTextures[globV.mySave.SelectedBack];
	
	}

	private void CeepShuffling(){


		shuffleDelay -= Time.deltaTime;
		if (shuffleDelay < 0) {
			PlayMy(ShufStart);
			int bottom = UnityEngine.Random.Range (0, 52);
			if (card [bottom].Stacked){
				shuffleDelay+=1;
				return;
			}
			shuffling=false;
						
			card [bottom].MoveMeTo (ShufflingTo.transform.position, shuffleToAng);
						for (int i=0; i<52; i++)
								if (i != bottom)
										card [i].StackMeTo (cardObj [bottom]);
			
				}
	}

	public GameObject Table;
	public void SetHD(bool to){
		//	
		//else 

		if (to == false) {
					cardObj [0].GetComponent<Renderer> ().sharedMaterial.shader = Shader.Find ("Mobile/Diffuse");
					card [0].face.GetComponent<Renderer> ().sharedMaterial.shader = Shader.Find ("Mobile/Diffuse");
			Table.GetComponent<Renderer> ().sharedMaterial.shader = Shader.Find ("Legacy Shaders/Diffuse");
				} else {
			cardObj [0].GetComponent<Renderer> ().sharedMaterial.shader = Shader.Find ("Legacy Shaders/Transparent/Cutout/Diffuse");
			card [0].face.GetComponent<Renderer> ().sharedMaterial.shader = Shader.Find ("Legacy Shaders/Transparent/Cutout/Diffuse");
					Table.GetComponent<Renderer>().sharedMaterial.shader=Shader.Find("Legacy Shaders/Diffuse Detail");
				}
		}

	public GameObject HDTogler;
	public void ToggleHD(){
		HDTogler.SetActive (globV.wantHD);
		globV.wantHD = !globV.wantHD;
		PlayerPrefs.SetInt ("wantHD", globV.wantHD ? 1 : -1);
		SetHD(globV.wantHD);
	}



	AudioSource loud, quiet;
	void Start () {
		globV.deck = this; 
		for (int i=0; i<52; i++) {
			cardObj[i] = Instantiate (OriginalCard, transform.position, transform.rotation).GetComponent<Card>();
			card [i] = cardObj[i];
			card [i].index=i;
			CardOrdering[i]=i;
				}
		globV.DeckInstantiating = gameObject;
		ShuffleCards (gameObject,0);

		defController.Init();

		AudioSource[] tmp= GetComponents<AudioSource>();
		loud = tmp[0]; quiet = tmp[1]; 

		Load ();

		bool[] temp= globV.mySave.cardBacks;

		globV.mySave.cardBacks = new bool[CardBacksTextures.Length];

		for (int i=0; i<temp.Length; i++)
						globV.mySave.cardBacks [i] = temp [i];

		globV.mySave.cardBacks [5] = true;
		globV.mySave.cardBacks [0] = true;
						ApplyCardBack ();

		globV.orientation=PlayerPrefs.GetInt ("Orientation")==1  ? true : false;
		globV.ArrowWay=PlayerPrefs.GetInt ("ArrowWay")==1  ? true : false;

		OrientationToggler.GetComponent<Image> ().sprite = OrientationSprites [globV.orientation ? 1 : 0];
		ArrowToggler.GetComponent<Image> ().sprite = ArrowSprites [globV.ArrowWay ? 1 : 0];
		globV.wantHD=PlayerPrefs.GetInt ("wantHD")!=-1 ? true : false;
		ApplyCardFDront (PlayerPrefs.GetInt ("Face"));
		HDTogler.SetActive (!globV.wantHD);
		SetHD(globV.wantHD);

	}

	// Update is called once per frame
	void Update () {
		if (shuffling == true)
			CeepShuffling ();
		Ray ray;
		RaycastHit rayhit;
		ray = Camera.main.ScreenPointToRay (Input.mousePosition);	
		Physics.Raycast (ray, out rayhit, 1000, card[0].layersForRay);
		globV.mousePos = rayhit.point;
	
	}

	public string cardBackSaveLocation;

	public void Save(){
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create(Application.persistentDataPath + cardBackSaveLocation);

		bf.Serialize (file,globV.mySave);
		file.Close();
	}

	public void Load(){
		if (File.Exists (Application.persistentDataPath + cardBackSaveLocation)) {
						BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + cardBackSaveLocation, FileMode.Open);
						globV.mySave = (MySaveData)bf.Deserialize (file);
						file.Close ();
				} else {
			globV.mySave.cardBacks=new bool[CardBacksTextures.Length];
			PlayerPrefs.SetInt ("WantMusic", 1);
			PlayerPrefs.SetInt ("WantSounds", 1);
				}
	}

}
