using UnityEngine;
using System.Collections;

public class CardShadower : MonoBehaviour {
    Card shadowThis;
	Card card;
	float shadowFadeDelay;
	// Use this for initialization
	void Start () {
		gameObject.SetActive (false);
	}

	public void ShadowCard(Card dest){
		shadowThis = dest;
        card = dest;
		gameObject.SetActive (true);
		shadowFadeDelay = 0.5f;
		transform.position = new Vector3 (shadowThis.transform.position.x, 
		                                  shadowThis.transform.position.y, Mathf.Max (-10, shadowThis.transform.position.z+1));
		transform.rotation = dest.transform.rotation;
	}
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3 (shadowThis.transform.position.x, 
			                                  shadowThis.transform.position.y, Mathf.Max (-10, shadowThis.transform.position.z+1));
		transform.rotation = shadowThis.transform.rotation;
		if ((shadowThis.transform.position.z > -12) && (card.pulled==false))
						gameObject.SetActive (false);

		if (card.pulled == false) {
			shadowFadeDelay-=Time.deltaTime;
			if (shadowFadeDelay<0)
						gameObject.SetActive (false);
				}
	}
}
