using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MySmartButtonScript : MonoBehaviour {

	Text instruction;
	public bool ShowingMessage;
	public float MessageSpeed;
	public void ShowMessage(string myText){
		ShowMe ();
		if ((ShowingMessage) && (transform.position.x > 0)) {
						instruction.text = instruction.text + " " + myText;		
				} else {
						ShowingMessage = true;
						transform.position = new Vector3 (500, 260, 0);
						instruction.text = myText;
						GetComponent<Rigidbody>().velocity = new Vector3 (-MessageSpeed, 0, 0);
				}
	}

void Update(){
		if ((transform.position.x < -1000) && (ShowingMessage)) {
						ShowingMessage = false;
						HideMe ();
				}
	}


public void Start(){
		instruction = GetComponent<Text>();
	}
	int previous=-1;
	public void MyUpdatableValue(int i){
		if (!instruction) instruction = GetComponent<Text>();
		if (previous != i) {
			instruction.text = "["+i+"]";
			previous=i;
		}
	}


	public void ChangeMyText(int text){
		instruction.text = ""+text;
	}

	public void ChangeMyText(string text){
		instruction.text = text;
	}

	public void HideMe(){
		instruction = GetComponent<Text>();
		instruction.text=" ";
		gameObject.SetActive (false);
	}
	public void ShowMe(){
		instruction = GetComponent<Text>();
		gameObject.SetActive (true);
	
	}
}
