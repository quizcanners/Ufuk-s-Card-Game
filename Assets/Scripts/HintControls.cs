using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class HintControls : MonoBehaviour {

	Text instruction=null;
	public GameObject TextObj;
	public GameObject[] Arrows;
	public Vector3[] ArrowAng;
	bool ExpandingText;
	//string fullMessage;
	public int LetterCount;
	public bool CollapsableHint;
	public float arrowDistance;
	public bool movingToPosition;
	
	public bool tapflag;

	public void PlayAnimation(){
		GetComponent<Animation>().Play ();
	}

	public void TapMe(){
		tapflag = true;
		flagTapped = true;
	}

	public void PointArrow (int i,float ang){
		Vector3 temp=new Vector3();
		temp = transform.position;
		temp.z -= 5;
		temp.x += Mathf.Cos (ang) * arrowDistance;
		temp.y += Mathf.Sin (ang) * arrowDistance;
		Arrows [i].transform.position = temp;
		Arrows [i].transform.rotation = Quaternion.Euler (0, 0, ang / (Mathf.PI * 2) * 360f+45);
		Arrows [i].SetActive (true);

	}

	public void PointArrow(int i, Vector3 dest){
		float x = dest.x - transform.position.x;
		float y = dest.y - transform.position.y;
		PointArrow (i, Mathf.Atan2 (y , x));
        Canvas.ForceUpdateCanvases();
	}

	public void HideArrows(){
		for (int i=0; i<Arrows.Length; i++)
						Arrows [i].SetActive (false);
	}

	public void HintTo (string text, Vector3 pos){
		if (instruction==null)
		instruction = TextObj.GetComponent<Text>();
		if (gameObject.activeSelf==false)
		GetComponent<Animation>().Play ();
		gameObject.SetActive (true);
		transform.position = pos;
		instruction.text = text;//.Substring(0,1);
		//LetterCount = 20;
		//fullMessage = text;
		ExpandingText = false;
		CollapsableHint = false;
		tapflag=false;
		movingToPosition = true;
		HideArrows ();
	}

	public void HintTo (string text){
		HintTo (text, transform.position);
	}

	public void CollapsableHintTo(string text){
		HintTo (text);
		CollapsableHint = true;
		flagTapped = false;
	}

	public void CollapsableHintTo(string text, Vector3 position){
		HintTo (text, position);
		CollapsableHint = true;
		flagTapped = false;
	}

	public void HideMe(){
		gameObject.SetActive (false); 
		CollapsableHint=false;
		flagTapped = false;
	}

	public void ShowMe(){
		gameObject.SetActive (true);
	}




	void getDailyHint(){
		StringReader reader = null; 
		TextAsset puzdata = (TextAsset)Resources.Load("textfile", typeof(TextAsset));
	// puzdata.text is a string containing the whole file. To read it line-by-line:
		if (puzdata!=null)
	reader = new StringReader(puzdata.text);
	if ( reader != null )
	
		//Debug.Log(" puzzles.txt not found or not readable");
	
	//else
	{

			List<string> fileLines;
			

			fileLines = reader.ReadToEnd().Split("\n"[0]).ToList();

			int i=Random.Range(0,fileLines.Count); 

			CollapsableHintTo(fileLines[i]);
				//Debug.Log("-->" + fileLines[i]);
	}
		reader.Close ();
	}

	// Use this for initialization
	void Awake () {
		globV.hinter = this;
		//Debug.Log ("Hinter assigned");
		}

	void Start () {

		instruction = TextObj.GetComponent<Text>();
		//getDailyHint ();
	}
	public bool flagTapped;
	// Update is called once per frame
	void Update () {


		if ((tapflag) && (ExpandingText)) {
			//instruction.text=fullMessage;
			ExpandingText=false;
			tapflag=false;
			return;
		}

		if ((CollapsableHint) && (tapflag)) {
			tapflag=false;
			CollapsableHint=false;
			gameObject.SetActive(false);
		}



	}
}
