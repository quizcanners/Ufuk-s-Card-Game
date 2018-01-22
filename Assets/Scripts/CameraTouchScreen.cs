using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraTouchScreen : MonoBehaviour {
	private GameObject Mypos;
	public DeckOf Deck;
	private bool moving;
	public float CameraMoveSpeed;
	public float ScratchDistanceDelay;
	float scratchDistance;
	Vector2 TouchStartPosition;
	public float scratchDistFade;

	public void MoveMe(GameObject To){
		Mypos = To;
		moving = true;
		}

	void Start (){
		globV.mainCamera = this;
	}

	public bool RaycastCard (int ind){

		RaycastHit hit; 
		Vector3[] points=new Vector3[4];
		points [3] =points [2] =points [1] =points [0] = 
			globV.deck.cardObj [ind].transform.position - Camera.main.transform.position;

		points [0].x -= 20; points [1].x += 20; points [2].y -= 20; points [3].y += 20;
		for (int i=0; i<4; i++)
			if ((!Physics.Raycast(Camera.main.transform.position, points [i], out hit))
			       || (hit.collider.gameObject.GetComponent<Card>().index!=ind))
				return false;
		
		return true;

	}

	void Update () {

		if (moving) {
			float portion=Mathf.Min(1, CameraMoveSpeed*Time.deltaTime);		
			transform.position=Vector3.Lerp(transform.position, Mypos.transform.position, portion);
			transform.rotation = Quaternion.Lerp(transform.rotation, Mypos.transform.rotation,   portion);
			
			
			if (Vector3.Distance(transform.position,Mypos.transform.position)<0.1)
				moving=false;
		}

		if (EventSystem.current.IsPointerOverGameObject ()) {
			if (Input.touchCount > 0) 
					TouchStartPosition=Input.GetTouch(0).position;
			return;
		}

		if (Input.touchCount > 0) 
		{
		// Gat cards in gaps
			if (Input.GetTouch(0).phase == TouchPhase.Began){
				TouchStartPosition=Input.GetTouch(0).position;
			} else {
				Vector2 pos=Input.GetTouch(0).position;
				Vector2 gap=pos-TouchStartPosition;
				float dist=Mathf.Sqrt(gap.x*gap.x+gap.y*gap.y);
				dist/=5;
				gap/=dist;

				Ray ray; 
				RaycastHit hit;
				

				for (int i=1; i<dist-1; i++){
					ray = Camera.main.ScreenPointToRay( TouchStartPosition+gap*i );
					if ( Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "Card")
					{	
						//Debug.Log("Card collision works!!!");
						Card tmp=hit.collider.GetComponent<Card>();
						globV.Tt.TouchContinue(tmp.index);
					}
				}
			//	TouchStartPosition=Input.GetTouch(0).position;



			}


			for(int i = 0; i < Input.touchCount; i++){

					Ray ray = Camera.main.ScreenPointToRay( Input.GetTouch(i).position );
					RaycastHit hit;
					
					if ( Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "Card")
					{
						Card tmp=hit.collider.GetComponent<Card>();
						if (Input.GetTouch (i).phase == TouchPhase.Began){
							tmp.flagPull=true;  
							tmp.touchDuration=Time.time;
						} else
							if (Input.GetTouch(i).phase == TouchPhase.Ended){
								tmp.flagDrop=true;
								if (Time.time-tmp.touchDuration<0.2f)
									tmp.flagTurn=true;

						}

					}else{
					 if (Input.GetTouch (i).phase == TouchPhase.Began){
						globV.MyAudio.PlayMySoundSet(globV.MyAudio.TapCloth);
						//TouchStartPosition=Input.GetTouch(i).position;
						scratchDistance=0;
					}
					else if (scratchDistance>=ScratchDistanceDelay){
						scratchDistance=0;
						globV.MyAudio.PlayMySoundSet(globV.MyAudio.ClothScratch);
					} else 
					{
						scratchDistance*=(1f-scratchDistFade*Time.deltaTime);
						scratchDistance+=Vector2.Distance(TouchStartPosition,Input.GetTouch(i).position);

					}

					Deck.WindyEffect.Fuel(hit.point,1);

					if (hit.collider.gameObject.tag == "PopUp")
						hit.collider.gameObject.SetActive(false);
				}
				

				TouchStartPosition=Input.GetTouch(i).position;



			}
		}




	}
}
