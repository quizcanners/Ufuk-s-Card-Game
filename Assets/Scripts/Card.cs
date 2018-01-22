
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;



public class Card : MonoBehaviour {

	public GameObject StackPrefab;


	public int index;
	public bool Faced=false;
	public bool Stacked;
	public float StackDistance;
	public Stacker stacker;

	public int WillStack=0;
	private float WillStackDistance=0; 
	public Card WillStackTo;
	public int WillBeStackedTo=0;
	public Card StackedTo;
	public bool StackStarted=false;
	
	public float Z;
	public LayerMask layersForRay;
	public bool pulled=false;
	public float PullDownSpeed;
	public float RotationSpeed;
	public float riseSpeed;
	public int color;
	public int value;
	public Card otherC;
	public GameObject outline;
	public GameObject face;
	public bool PlayTapOnLand;

	private float pointingFade;
	private Vector3 mousePosDis;
	//private Ray ray;
	//private RaycastHit rayhit;
	public bool awake=true;

	public float touchDuration;
	public bool flagTurn=false;
	public bool flagPull=false;
	public bool flagDrop=false;
	public float UnstackCoolOff=0;

	public bool GoingToAPlace=false;
	private Vector3 GoToDest;
	public float GoToRotation=0;
	private int Selected = 0;
	public AudioClip[] flipSounds;
	public AudioClip[] tapSounds;
	public AudioClip[] cutSounds;
	public AudioClip[] selectSounds;
	// Use this for initialization
	public void SetSelected(){
		Selected = 10;
	
	}

	public void PlayMy(AudioClip[] toPlay){
		if ((!globV.wantSounds) || (!gameObject.activeSelf))
			return;
		GetComponent<AudioSource>().clip = toPlay[Random.Range(0,toPlay.Length)];
		GetComponent<AudioSource>().Play ();
	}

	public static float NormalizeAngle (float angle)
	{
		while (angle>=360)
			angle -= 360;
		while (angle<0)
			angle += 360;
		return angle;
	}

	void ManualCollisionCheck(){
		if ((!globV.Tt.freeGame) || (GoingToAPlace) || (StackStarted) || (awake)) return;

		if (!pulled) {
		//	if ((!globV.Tt.freeGame) || (GoingToAPlace) || (StackStarted) || (awake)) return;
			
			int pHeight=0;
			float pDist=100;
			int pInd=-1;
			
			for (int i=0; i<52; i++)
			if ((globV.deck.cardObj [i].gameObject.activeSelf) && (i != index)) {
				Card oCard = globV.deck.card [i];
				if (!oCard.pulled) {
					float distance = globV.GetMy2DDistance (transform.position, globV.deck.cardObj [i].transform.position);
					
					if ((distance < 10) && (!oCard.pulled) &&	(!oCard.GoingToAPlace) 
					    && (!oCard.StackStarted) && (!oCard.awake)&& ((!Stacked) || ((oCard.Stacked) 
					                                                             && (oCard.stacker.cardsCount > stacker.cardsCount)))) 
					{
						if ((oCard.Stacked) && (oCard.stacker.cardsCount>pHeight)){
							pHeight=oCard.stacker.cardsCount;
							pInd=oCard.index;
						}
						else 
						if ((pHeight==0) && (pDist>distance)) {
							pInd=oCard.index;
							pDist=distance;
						}
						
						//	if (WillStackTo == deck.cardObj [i])
						
						
					}
				}
			}
			if (pInd!=-1) 
				StackMeTo (globV.deck.cardObj [pInd]);
			
		}
		else
			for (int i=0; i<52; i++)
			if ((globV.deck.cardObj [i].gameObject.activeSelf) && (i != index)) {
				Card oCard = globV.deck.card [i];
				float distance = globV.GetMy2DDistance (transform.position, globV.deck.cardObj [i].transform.position);
				
				if (oCard.pulled==false){
					
					if (oCard.Stacked == false)
						distance *= 4;
					else
						distance = distance * (104 - (float)oCard.stacker.cardsCount) / (104);
					
					if ((distance < StackDistance) && ((distance < WillStackDistance) || (WillStack <= 2))) {
						//	Debug.Log("Distance: "+ distance + " i: "+ i + " pulled "+ pulled);
						WillStackDistance = distance;
						WillStack = 10;
						if (WillStackTo != globV.deck.cardObj[i]) {
							if (WillStackTo != null)
								otherC.WillBeStackedTo = 0;
							WillStackTo = globV.deck.cardObj[i];
							otherC = WillStackTo.GetComponent<Card> ();
						} 
						otherC.WillBeStackedTo = 100;
					}
				}
			}
	}

	public void FaceMe(bool up){
	
		if (Faced != up) {
				GoToRotation = -GoToRotation;
			if ((!GoingToAPlace) && (!StackStarted)){
				flipToZ=NormalizeAngle(360-transform.eulerAngles.z);
				FuckYouDestination=transform.eulerAngles;
				FuckYouDestination.z=flipToZ;
				FuckYouDestination.y=up ? 180 : 0;

				//FuckYouDestination.z=GoToRotation*360/Mathf.PI/2;
				//if (flipToZ==360) flipToZ=0;
			}
			else 
			flipToZ=0;
			awake = true;
			Faced = up; 
			if ((Stacked) && (StackedTo != null) && (gameObject.activeSelf)) {
				StackedTo.gameObject.SetActive (true);

				}
	
			//Z=-stacker.cardsCount*globV.StackCardThick-5;		
		}
	}

	public void FaceMe(){
		FaceMe(!Faced);

	}

	public void SetPull(bool pulling){

		if (pulling==true) {
		//	Vector3 temp=transform.GetComponent<BoxCollider> ().size;
		//transform.GetComponent<BoxCollider> ().size = new Vector3(temp.x,temp.y ,200f);
			Z = -PickRaiseHeight-0.1f*globV.deck.CardOrdering [index];
			PlayTapOnLand=true;
			if (globV.Tt.touchedCard==index) 
				globV.deck.ShadowCard(index);
		}
		else 
		{
		//	Vector3 temp=transform.GetComponent<BoxCollider> ().size;
		//	transform.GetComponent<BoxCollider> ().size = new Vector3(temp.x,temp.y ,20f);
			if (pulled==true){
				pulled=false;
			globV.deck.OrderCardUp(index);
				AssignZ();
			globV.deck.ConstrainCard(index);

			}

		}
		globV.drugging=pulling;
		pulled = pulling;
	}

	const float tylingx = 1f / 13f;

	void Start () {
		GoToRotation = 0;
		outline.SetActive (false);
		color = globV.ColorNo;
		value = globV.ValueNo;
		//face.renderer.material.mainTexture = faceColor[color].FaceValue[value];


		Mesh mesh = face.GetComponent<MeshFilter> ().mesh;

		float xstart = tylingx*(value+1);
		float xcut = xstart-tylingx;
		float ystart = 0.25f*(color+1);
		float ycut = ystart-0.25f;

		mesh.uv = new Vector2[] { new Vector2(xstart, ystart), 	new Vector2(xcut, ycut) ,
			new Vector2(xcut, ystart), new Vector2(xstart, ycut)  };


		globV.ValueNo++;
		if (globV.ValueNo > 12) {
		globV.ColorNo+=1;
			globV.ValueNo=0;
		}

		awake = true;
		SetPull(false);
	}

	public float PickRaiseHeight=23;


	public void SetOffFromMouse(){
		mousePosDis = globV.SetPullOff(Input.mousePosition) - transform.position;
	}


    public static bool mouseDown;
    public static float mouseDownTime;
    public static Vector3 downPosition = Vector3.zero;
	void OnMouseOver(){
		if (EventSystem.current.IsPointerOverGameObject())
			return;
		
		if (globV.drugging==false) 
		pointingFade += 0.1f;

        if (Input.GetMouseButtonDown(0)) {
            mouseDown = true;
            mouseDownTime = Time.time;
            downPosition = transform.position;
        }
        
        if (Input.GetMouseButtonUp(0))  {
            downPosition -= transform.position;
            downPosition.z = 0;
            if (mouseDown && ((Time.time - mouseDownTime < 0.05f) || ((Time.time - mouseDownTime < 0.3f) && (downPosition.magnitude<10))) )
                flagTurn = true;
            mouseDown = false;
        }

		if (Input.GetMouseButtonDown (0) && (!StackStarted)) {
			Ray ray;
			RaycastHit rayhit;
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);	
			Physics.Raycast (ray, out rayhit, 1000, layersForRay);
			mousePosDis=rayhit.point-transform.position;
	
			flagPull = true;

			globV.Tt.StartTouch(index);
		} 

		globV.Tt.TouchContinue (index);


		}
	void OnMouseUp() {
		flagDrop = true;
		globV.Tt.EndTouch (index);
	}

	private float flipToZ;
	public float TempZ;
	public Vector3 FuckYouDestination;

	void RotateToY(){
		if ((GoingToAPlace) || (StackStarted))
						return;

	//	Vector3 to = transform.eulerAngles;
	//	to.y=Yi;
		if (flipToZ == 0) {
			FuckYouDestination = transform.eulerAngles;
			FuckYouDestination.y= Faced ? 180 : 0;
				}
		float dist =Quaternion.Angle( transform.rotation, Quaternion.Euler(FuckYouDestination));
		TempZ=Z-(100f-Mathf.Abs(90f-dist))*2;
	//	Debug.Log("Dist: "+ dist);
		if (dist > 0.05f) {
			float portion=Time.deltaTime * RotationSpeed;
			Quaternion _targetRotation = Quaternion.Euler(FuckYouDestination);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, portion);
			}
				else {
			transform.eulerAngles = FuckYouDestination;
			awake=false;

		}
	}
	/*
	void OnTriggerStay(Collider other) {
		//if (globV.Tt.touchedCard!=index) return;
		if  (other.tag=="Card") {

			Card oCard=other;
			if (oCard.pulled) return;
			float distance=globV.GetMy2DDistance(gameObject, other.gameObject);

			if (pulled == false) {

			if ( (distance<5) && (globV.Tt.freeGame) && 
				       (!oCard.GoingToAPlace) && (!oCard.StackStarted) && (!oCard.awake)
				    && (!GoingToAPlace) && (!StackStarted) && (!awake)
				    && ((!Stacked) || ((oCard.Stacked ) && (oCard.stacker.cardsCount>stacker.cardsCount)))
				    )
				{
					if (WillStackTo==other.gameObject) StackMeTo(other.gameObject);
				}
				else
 
				return;
				
			}

			if ((oCard.Faced!=Faced) && (distance>40)) return;

			if (oCard.Stacked==false) distance*=4;
			else distance=distance*(100f-(float)oCard.stacker.cardsCount)/100f;

			if ((distance<StackDistance) && ((distance<WillStackDistance) || (WillStack<=2))){
				WillStackDistance=distance;
				WillStack=10;
				if (WillStackTo!=other.gameObject){
					if (WillStackTo!=null)
						otherC.WillBeStackedTo=0;
					WillStackTo=other.gameObject;
					otherC=WillStackTo;
				} 
		

				otherC.WillBeStackedTo=100;
			}
		
		}

		
	}
	*/

	public void MoveMeTo(Vector3 pos, float ang ){
		StackStarted = false;	

		SetPull(false);
		GoingToAPlace = true;

		globV.TurnAngUp (ref ang);
		GoToRotation = ang;
		globV.deck.OrderCardUp (index);
		GoToDest = new Vector3(pos.x,pos.y,Z);
	}

	public void MoveMeTo(float x, float y, float z , float ang ){
		StackStarted = false;	
		SetPull(false);

		GoingToAPlace = true;

		globV.TurnAngUp (ref ang);
		GoToRotation = ang;
		globV.deck.OrderCardUp (index);
		GoToDest = new Vector3(x,y,Z);
	}

	public void StackMeToTop(Card card){
		GoingToAPlace = false;
		SetPull(false);
		StackStarted = true;
		otherC = card.GetComponent<Card> ();
		if (otherC.Stacked) {
			card=otherC.stacker.cards[otherC.stacker.cardsCount-1];
			otherC = card;
		}
		if (otherC.Faced!=Faced) FaceMe(!Faced);
		WillStackTo = card;
		//UnstackCoolOff = 0.00001f;
	}

	public void StackMeTo(Card card){
		GoingToAPlace = false;
		SetPull(false);
		StackStarted = true;
        otherC = card;
		if (otherC.Stacked) {
			Card temp=otherC.stacker.cards[otherC.stacker.cardsCount-1];
			if (temp.Faced!=Faced) FaceMe(!Faced);
		}
			else
		if (otherC.Faced!=Faced) FaceMe(!Faced);
		WillStackTo = card;
	}

	public void Unstack(){
		if ((Stacked) && (StackedTo)) {
			PlayMy(cutSounds);
			Stacked=false;
			StackedTo.gameObject.SetActive(true);
			stacker.cardsCount-=1;
			stacker.RecalcHeight();
			AssignZ();
		}
	}

	void TryPlayLand(){
		if ((PlayTapOnLand) && (!pulled) && (gameObject.activeSelf)) {
			PlayTapOnLand=false;
			PlayMy(tapSounds);
		}
	}

	public void AssignZ(){
		if ((Stacked == false) && (pulled==false)) {

						Z = -0.01f - 0.19f * globV.deck.CardOrdering [index];
			TempZ=Z;
				}
	}

	void Update () {

		if (UnstackCoolOff > 0) {
			UnstackCoolOff-=Time.deltaTime;
			return;
		}

		if (GoingToAPlace) {

						Unstack ();

						float dist = Vector3.Distance (transform.position, GoToDest);
						float fraction = Mathf.Min (1f, (globV.stackToSpeed / dist) * Time.deltaTime);

						Vector3 stto = GoToDest;
						if (dist > 50)
								stto.z -= dist / 2;

						transform.position = Vector3.Lerp (transform.position,
			                                   stto,
			                                   fraction);

						FuckYouDestination.x = 0;
						FuckYouDestination.z = GoToRotation / (Mathf.PI * 2) * 360f;
						FuckYouDestination.y = Faced ? 180 : 0;
						Quaternion RotTo = Quaternion.Euler (FuckYouDestination);//temp);

						transform.rotation = Quaternion.Lerp (transform.rotation, RotTo, fraction);

						if (dist < 5) {
								GoingToAPlace = false;
								transform.position = GoToDest;
						} 
				} else 
						ManualCollisionCheck ();

		if (StackStarted) {

			pulled=false;
			if (Stacked){
			Unstack();
				return;
			}

			float dist=Vector3.Distance(transform.position,WillStackTo.transform.position);
			float fraction=(globV.stackToSpeed/dist)*Time.deltaTime;
			fraction=Mathf.Min (1.0f,fraction);
			Vector3 stto=WillStackTo.transform.position;
			if (dist>50) stto.z-=dist/1.5f;
			awake=false;
			//stto.z-=1;
			//TempZ=-40;//stto.z;
			//	Z=-40;//stto.z;
			transform.position = Vector3.Lerp (transform.position,
			                                   stto,
			                                   fraction);

			FuckYouDestination=WillStackTo.transform.rotation.eulerAngles;
			FuckYouDestination.y=Faced ? 180 : 0;
			FuckYouDestination.x=0;
			//WillStackTo.transform.rotation
			transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.Euler(FuckYouDestination), Time.time * fraction);

			if ((dist<5) && (otherC.StackStarted==false) && (otherC.GoingToAPlace==false) && (otherC.awake==false))
			{
				Stacked=true;
				StackStarted=false;
				pulled=false;
				WillStackTo.gameObject.SetActive(false);
				otherC.pulled=false;
				TryPlayLand();
				if (otherC.Stacked) {
					stacker=otherC.stacker;
					WillStackTo=stacker.cards[stacker.cardsCount-1];
					WillStackTo.gameObject.SetActive(false);
					otherC=WillStackTo;
					otherC.stacker.cardsCount+=1;
					transform.rotation=Quaternion.Euler(FuckYouDestination);//WillStackTo.transform.rotation;
				} else {

                    stacker = (Instantiate(StackPrefab, WillStackTo.transform.position, WillStackTo.transform.rotation)).GetComponent<Stacker>();
                    stacker.transform.position=new Vector3(transform.position.x, transform.position.y,-0.01f);
                    stacker.transform.eulerAngles=new Vector3 (0,0,Faced ? -transform.eulerAngles.z : transform.eulerAngles.z);
					stacker.cardsCount=2;
					stacker.cards[0]=WillStackTo;
					otherC.Stacked=true;
					otherC.stacker=stacker;
					otherC.WillStackTo=null;
					otherC.StackedTo=null;
					globV.deck.OrderCardDown(otherC.index);
					otherC.Z=-0.1f;

				}
				StackedTo=WillStackTo;
				GoToRotation=otherC.GoToRotation;
				Z=-stacker.cardsCount*globV.StackCardThick;
				stacker.cards[stacker.cardsCount-1]=this;
				stacker.RecalcHeight();
				transform.position=new Vector3(stacker.transform.position.x, stacker.transform.position.y, 
				                               Z);
			}
		}

						if (pulled) {
								WillBeStackedTo=0;
								/*Ray ray;
								RaycastHit rayhit;
								ray = Camera.main.ScreenPointToRay (Input.mousePosition);	
								Physics.Raycast (ray, out rayhit, 1000, layersForRay);*/
								float lz = transform.position.z;
								transform.position = globV.mousePos - mousePosDis;//rayhit.point - mousePosDis;
								transform.position = new Vector3 (transform.position.x, transform.position.y, lz);
								pointingFade += 0.1f;
			
						}

		if (Selected>0) {
			pointingFade += 0.1f;
			Selected-=1;
		}


		if (WillBeStackedTo > 0) {
						pointingFade=Mathf.Min(pointingFade,1.0f);
						outline.GetComponent<Renderer>().material.SetColor ("_TintColor", new Color (0, 0, 1, pointingFade));
						outline.SetActive (true);
						pointingFade *= 0.85f;
						WillBeStackedTo -= 1;
			if (WillBeStackedTo>90) pointingFade+=0.25f;
				} else {
						if (WillStack > 0) {
								outline.GetComponent<Renderer>().material.SetColor ("_TintColor", new Color (1, 1, 1, 1));
								outline.SetActive (true);
								pointingFade = 1f;
								WillStack -= 1;

						} else {
								if (pointingFade > 0.01) {
										pointingFade=Mathf.Min(pointingFade,1.0f);
										outline.GetComponent<Renderer>().material.SetColor ("_TintColor", new Color (0.2f, 0.9f, 0.2f, pointingFade));
										outline.SetActive (true);
										pointingFade *= 0.9f;
								} else
										outline.SetActive (false);
						}
				}

						if (awake) {//&& (!StackStarted) && (!GoingToAPlace)) {
						
								RotateToY();
				} else
						TempZ = Z;

		if ((Mathf.Abs(transform.position.z - TempZ)>0.1) && (!StackStarted) && (!GoingToAPlace)) {
			float portion=Mathf.Min (Time.deltaTime * PullDownSpeed,1);
			transform.position = Vector3.Lerp (transform.position, new Vector3 (transform.position.x, transform.position.y, TempZ),
			                                   portion);
				}
	//	else 
				//if (Stacked) 
				//	StackedTo.SetActive (false);
	
			TryPlayLand();

	}
}
