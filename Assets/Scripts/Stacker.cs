using UnityEngine;
using System.Collections;

public class Stacker : MonoBehaviour {
	public int cardsCount;
	public GameObject[] sides;
	public Card[] cards;
	public bool updated=true;
	public GameObject RadialEffect;
	public float UnstackDelay;

	public void FlipStack(){
        Card to = cards [cardsCount - 1];
		Card card = to.GetComponent<Card> ();
		card.MoveMeTo (transform.position, 0);
		card.FaceMe (!card.Faced);
		for (int i=cardsCount-2; i>=0; i--) {
			Card card2=cards [i].GetComponent<Card> ();
						card2.StackMeTo (cards [i + 1]);
				}

	}

	public void StackNonActiveTo (Vector3 destination ){
		Card underCard=null;

		for (int i=cardsCount-1; i>=0; i--) {
			Card card=cards[i];
			if ((card.GoingToAPlace==false) && (card.StackStarted==false)){
				underCard=cards[i];
				card.MoveMeTo(destination,0);
				break;
			}
		}

		for (int i=cardsCount-1; i>=0; i--) {
			Card card=cards[i];
			if ((card.GoingToAPlace==false) && (card.StackStarted==false)){
				card.StackMeTo(underCard);
			}
		}

	
	}


	public static void StackNToPos(int[] inds, int n, Vector3 dest , float ang){
		Card card = globV.deck.card[inds[0]];
		card.MoveMeTo (dest.x, dest.y, dest.z, ang);
		for (int i=1; i<n; i++) {
			card = globV.deck.card[inds[i]];
			card.StackMeTo(globV.deck.cardObj [inds [i-1]]);
		}
	}

	public int StackNToPos(ref int ind, ref int[] inds, int n, Vector3 dest , float ang){
		n = Mathf.Min (n, ind + 1);
		int to = ind;
		Card card = cards [ind].GetComponent<Card> ();
		card.MoveMeTo (dest.x, dest.y, dest.z, ang);
		card.UnstackCoolOff=UnstackDelay;
		inds [0] = card.index; ind--; 
		
		for (int i=1; i<n; i++) {
			card = cards [ind].GetComponent<Card> ();
			card.StackMeTo(cards[to-i+1]);
			card.UnstackCoolOff=UnstackDelay;
			inds[i]=card.index; ind--;
		}
		return n;
	}

	public static void AllignByLine(int[] inds, int n, bool vert, CurveStorer curve){
		int i = 0;
		int pnp = 1;
		float gap = curve.Length / (n);
		float dist = curve.dist[pnp];
		float ang=0;
		while (i<n) {

			float portion=curve.dist[pnp];
			if (portion==0) portion=1;
			else 
			portion=(portion+dist)/portion;
			//if (portion<0)
			//Debug.Log("Portion "+ i+" =" +portion);
			Vector2 point=curve.point[pnp-1]+(curve.point[pnp]-curve.point[pnp-1])*portion;
			Vector2 disp=curve.point[pnp]-curve.point[pnp-1];
			float ang2=Mathf.Atan2 (disp.y, disp.x)+(vert ? Mathf.PI/2 : 0);
			if (i==0) 
				ang=ang2;
			else ang=Mathf.LerpAngle(ang*180/Mathf.PI,ang2*180/Mathf.PI,0.4f)/180*Mathf.PI;
			Vector3 fin=globV.SetPullOff(point);
				Card card = globV.deck.card [inds [i]];
			card.MoveMeTo (fin.x, fin.y, 0, ang); 
				globV.deck.OrderCardUp (inds [i]);

			dist+=gap;

			while ((dist>=0) || (curve.dist[pnp]==0)){
				if (pnp>=curve.count-1){
					//Debug.Log("Outside of curve count");
					break;
				}
				pnp++;
				dist-=curve.dist[pnp];

			}

			i++;
		}


	}

	public static void AllignNbyAng(int[] inds, int n, Vector3 dest , float ang, bool vert,float dist){
		for (int i=0; i<n; i++)
			if (inds[i]!=-1)
		{
			Card card = globV.deck.card [inds[i]];
			card.MoveMeTo (dest.x+Mathf.Cos(ang+(vert ? Mathf.PI/2 : 0))*dist*i,
			               dest.y+Mathf.Sin(ang+(vert ? Mathf.PI/2 : 0))*dist*i, 
			               0,card.Faced ? -ang : ang); 
			globV.deck.OrderCardUp(inds[i]);
		}
	}

	public int AllignNbyAng(ref int ind, ref int[] inds, int from, int n, Vector3 dest , float ang, bool vert,float dist){
		n = Mathf.Min (n, ind + 1);
		for (int i=0; i<n; i++) {
			Card card = cards [ind].GetComponent<Card> ();
			card.MoveMeTo (dest.x+Mathf.Cos(ang+(vert ? Mathf.PI/2 : 0))*dist*i,
			               dest.y+Mathf.Sin(ang+(vert ? Mathf.PI/2 : 0))*dist*i, 
			               0,ang);
			card.UnstackCoolOff=UnstackDelay;
			inds[i+from]=card.index; ind--;
		}
		return n;
	}

	public int AllignNbyAng(ref int ind, ref int[] inds, int n, Vector3 dest , float ang, bool vert,float dist){
		n = Mathf.Min (n, ind + 1);
		for (int i=0; i<n; i++) {
			Card card = cards [ind].GetComponent<Card> ();
			card.MoveMeTo (dest.x+Mathf.Cos(ang+(vert ? Mathf.PI/2 : 0))*dist*i,
			               dest.y+Mathf.Sin(ang+(vert ? Mathf.PI/2 : 0))*dist*i, 
			                                            0,ang);
			card.UnstackCoolOff=UnstackDelay;
			inds[i]=card.index; ind--;
		}
		return n;
	}

	public static void AllignNbyAng(int[,] inds, int ArrDim,int n, Vector3 dest , float ang, bool vert,float dist){
		for (int i=0; i<n; i++) {
			Card card = globV.deck.card [inds[ArrDim,i]];
			card.MoveMeTo (dest.x+Mathf.Cos(ang+(vert ? Mathf.PI/2 : 0))*dist*i,
			               dest.y+Mathf.Sin(ang+(vert ? Mathf.PI/2 : 0))*dist*i, 
			               0,ang); 
		}
	}



	public int AllignNbyAng(ref int ind, ref int[,] inds, int ArrDim, int n, Vector3 dest , float ang, bool vert,float dist){
		n = Mathf.Min (n, ind + 1);
	
		for (int i=0; i<n; i++) {
			Card card = cards [ind].GetComponent<Card> ();
			card.MoveMeTo (dest.x+Mathf.Cos(ang+(vert ? Mathf.PI/2 : 0))*dist*i,
			               dest.y+Mathf.Sin(ang+(vert ? Mathf.PI/2 : 0))*dist*i, 
			               0,ang);
			card.UnstackCoolOff=UnstackDelay;
			inds[ArrDim,i]=card.index; ind--;
		}
		return n;
	}

	public void ArrangeYourCards(){
		for (int i=0; i<cardsCount; i++)
						cards [i].GetComponent<Card> ().Z = -globV.StackCardThick * (i+1);
	}

	Material[] ScaleMaterial;

	public void RecalcHeight(){
		if (ScaleMaterial==null){
			ScaleMaterial=new Material[sides.Length];
			for (int i=0; i<sides.Length; i++)
				ScaleMaterial[i]=sides [i].GetComponent<Renderer>().material;
		}

		for (int i=0; i<sides.Length; i++) {
			float size = cardsCount * globV.StackCardThick;
			sides [i].transform.position = new Vector3 (sides [i].transform.position.x, sides [i].transform.position.y, -size / 2);
			sides [i].transform.localScale = new Vector3 (sides [i].transform.localScale.x, 
			                                              size, 
			                                              sides [i].transform.localScale.z);
			//sides [i].GetComponent<Renderer>().material
				ScaleMaterial[i].mainTextureScale = new Vector2 (1, cardsCount);
			
		}


		if (cardsCount < 2)
		{
			if (cardsCount==1) { Card scr=cards[0];
				scr.Stacked=false;
				scr.AssignZ();
			}
			Destroy (gameObject);
		}
		if ((cardsCount==52) && (globV.wantHD))
			Instantiate(RadialEffect, transform.position,transform.rotation);
		
		updated=false;
	}

	// Update is called once per frame
	void Update () {
		if (updated) {
			RecalcHeight();	
				}
	}


}
