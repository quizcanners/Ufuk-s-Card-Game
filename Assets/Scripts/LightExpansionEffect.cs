using UnityEngine;
using System.Collections;

public class LightExpansionEffect : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	public float speed;
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(transform.position.x, transform.position.y,
		                                   transform.position.z-speed * Time.deltaTime);
		if (transform.position.z<-1500)
			Destroy(gameObject);
	}
}
