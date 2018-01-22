using UnityEngine;
using System.Collections;

public class EffectDie : MonoBehaviour {

	float time=1;
	public int Amount;
	public float duration;

	void Start(){
		//transform.particleEmitter.maxEmission=0;
		//transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z-30);
	}

	public void Fuel (Vector3 coord, float percent){
		if (!globV.wantHD)
						return;
		transform.position = new Vector3(coord.x,coord.y, -10);
		transform.GetComponent<ParticleEmitter>().maxEmission = Amount*percent;
		time = duration;
	}

	public void Fuel (GameObject coordinates, float percent){
		transform.position = new Vector3(coordinates.transform.position.x,coordinates.transform.position.y, -20);
		transform.GetComponent<ParticleEmitter>().maxEmission = Amount*percent;
		time = duration;
	}

	// Update is called once per frame
	void Update () {
		if (transform.GetComponent<ParticleEmitter>().maxEmission > 0) {
						time -= Time.deltaTime;
						if (time < 0){
				transform.GetComponent<ParticleEmitter>().maxEmission=0;

			}
				}
	}
}
