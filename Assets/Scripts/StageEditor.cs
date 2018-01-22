using UnityEngine;
using System.Collections;

using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class StageEditor : MonoBehaviour {

	public void TrySetActive(){
		if (globV.DevBuild)
						gameObject.SetActive (true);
	}

	public MySmartButtonScript stageNoText;
	public GameObject ShuffleTo;
	// Use this for initialization
	void updateStage(){
		stageNoText.ChangeMyText(globV.CurrentStageEd);
		PlayerPrefs.SetInt ("EditingStage", globV.CurrentStageEd);
	}

	void Start () {
		globV.CurrentStageEd=PlayerPrefs.GetInt ("EditingStage");
		updateStage ();
		globV.PendingEditorArrangement = false;
	}

	public void ChangeStage(int by){
		globV.CurrentStageEd = Mathf.Max (0, globV.CurrentStageEd+by);
		updateStage ();
	}
	
	void Update () {
	if ((globV.PendingEditorArrangement) && (!globV.deck.Arranging())) {
						if ((PendArrangement != null) && (PendArrangement.Length > 0) && (globV.deck.card [0].Stacked)) {
								globV.deck.ArrangeStoredState (globV.deck.card [0].stacker, ref PendArrangement);
						}
					globV.PendingEditorArrangement=false;
				}

	}

	string StagesFolder = "/Resources";



	public void Save(string gameName  ){
		int count = globV.deck.CountUnstacked ();
		if (count < 1)
			return;
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create(Application.dataPath+StagesFolder+"/"+gameName+"/"+globV.CurrentStageEd+".bytes");
		StoredCard[] arrangement=new StoredCard[count];
		globV.deck.FillUnstacked (ref arrangement);
		bf.Serialize (file,arrangement);
		file.Close();
	}

	StoredCard[] PendArrangement;


	public void Load(string gameName){
		string path = Application.dataPath  + StagesFolder+"/"+gameName+ "/" + globV.CurrentStageEd + ".bytes";
		if (File.Exists (path)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (path, FileMode.Open);
			PendArrangement = (StoredCard[])bf.Deserialize (file);
			globV.PendingEditorArrangement=true;
			globV.deck.ShuffleCards(ShuffleTo, 0);
			file.Close ();
		} 
	}
}
