using UnityEngine;
using System.Collections;



public class DefaultGameController : MonoBehaviour {

	public GameObject DeckObj;
	public DeckOf deck;
	private bool started=false;
	public GameObject DefCameraPosition;
	public GameObject SecondaryCamera;
	public CameraTouchScreen MainCamera;
	//public GameObject StageEditorObj;

	public void Init(){
		gameObject.SetActive (true);
		DeckObj = globV.DeckInstantiating;
		deck = DeckObj.GetComponent<DeckOf> ();
		started=true;
		MainCamera.MoveMe (SecondaryCamera);
		deck.SparclyEffect.Fuel(Vector3.zero, 3);
		//StageEditorObj.SetActive (true);
	}

	public void Reshuffle(){
		deck.ShuffleCards (globV.DeckInstantiating, 0);
	}

	public void TurnOff(){
		MainCamera.MoveMe (DefCameraPosition);
		started = false;
		gameObject.SetActive (false);
	}

	// Update is called once per frame
	void Update () {
		if (started) {
			for (int i=0; i<52; i++)

								if (deck.cardObj [i].gameObject.activeSelf) {
										Card card = deck.card [i];
										if (card.flagTurn) {
												card.FaceMe (!card.Faced);
					card.PlayMy(card.flipSounds);
												card.flagTurn = false;
										}
										if (card.flagPull) {
					MainCamera.MoveMe(DefCameraPosition);
					//card.PlayMy(card.tapSounds);
												card.SetPull (true);
												card.Unstack ();
												card.flagPull = false;
										}
										if (card.flagDrop) {

												card.SetPull (false);
					card.PlayMy(card.tapSounds);
												if (card.WillStack > 0) {
														if (globV.GetMy2DDistance (deck.cardObj[i].transform.position, card.WillStackTo.transform.position) < card.StackDistance * 2)
																card.StackMeTo (card.WillStackTo);
												} else
						card.AssignZ();
										card.flagDrop=false;
										}
			
								}	
		}
	}



}
