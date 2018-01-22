using UnityEngine;
using System.Collections;

public class AllignmentArrow : MonoBehaviour {
	public Vector3 From;
	public Vector3 To;
	public GameObject Arrow;
	// Use this for initialization

	public void ShowMe(){
		gameObject.SetActive (true);
	}

	public void HideMe(){
		gameObject.SetActive (false);
	}
	public float SizeCoef;
	public void UpdateArrow(){
		Vector3 Diff= To-From;
		float ang = Mathf.Atan2 (Diff.y, Diff.x);
		transform.rotation=Quaternion.Euler (0, 0, ang / (Mathf.PI * 2) * 360f);
		ShowMe ();
		float dist=globV.GetMy2DDistance(From, To);
		transform.localScale = new Vector3 (dist/SizeCoef,1,1);
		//Arrows [i].transform.rotation = Quaternion.Euler (0, 0, ang / (Mathf.PI * 2) * 360f+45);
		//	PointArrow (i, Mathf.Atan2 (y , x));
	}

	public void UpdateFrom(Vector3 NewFrom){
		From = NewFrom;
		transform.position = From;// new Vector3( From.x, From.y, -2);
		UpdateArrow ();
	}

	public void UpdateTo (Vector3 NewTo){
		To = NewTo;
		UpdateArrow ();
	}

	void Start () {
		globV.Arrow = this;
		HideMe ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
