using UnityEngine;
using System.Collections;

public class audioController : MonoBehaviour {

	public AudioClip[] MenuMusic;
	public AudioClip[] NoteClips;
	public AudioClip[] CardFlip;
	public AudioClip[] TapCloth;
	public AudioClip[] ClothScratch;

	public int MaxDistance=700;

	const int EffectSrcNo = 6;

	AudioSource Music;
	AudioSource[] oneSound = new AudioSource[EffectSrcNo];
	float[] PlayStarted = new float[EffectSrcNo];


	public void PlayMySoundSet(AudioClip[] sound ){
		if (!globV.wantSounds)
			return;
		int no = 0;
		float time = Time.time;
		for (int i=0; i<EffectSrcNo; i++) {
			if (PlayStarted [i] < time) {
				time = PlayStarted [i];
				no = i;
			}
		}

		PlayStarted[no]=Time.time;
		
		oneSound[no].clip = sound[Random.Range(0,sound.Length)];
		oneSound[no].Play ();
	}

	public void PlayMySound(AudioClip sound ){
		if (!globV.wantSounds)
						return;
		int no = 0;
		float time = Time.time;
		for (int i=0; i<EffectSrcNo; i++) {
						if (PlayStarted [i] < time) {
								time = PlayStarted [i];
								no = i;
						}
				}
			PlayStarted[no]=Time.time;

		oneSound[no].clip = sound;
		oneSound[no].Play ();
	}

	void PlayNextSong(){
		Music.clip = MenuMusic[Random.Range(0,MenuMusic.Length)];
		Music.Play();
	//	Invoke("PlayNextSong", audio.clip.length);
	}

	public GameObject soundTogler;
	public GameObject musicTogler;

	public void ToggleSounds(){
		soundTogler.SetActive (globV.wantSounds);
		globV.wantSounds = !globV.wantSounds;
		PlayerPrefs.SetInt ("WantSounds", globV.wantSounds ? 1 : -1);
	}

	public void ToggleMusic(){
		musicTogler.SetActive (globV.wantMusic);
		if (globV.wantMusic) {
						Music.Stop ();
				} else 
			PlayNextSong ();

		globV.wantMusic = !globV.wantMusic;
		PlayerPrefs.SetInt ("WantMusic", globV.wantMusic ? 1 : -1);
	}

	void Start(){
		AudioSource[] tmp= GetComponents<AudioSource>();
		globV.MyAudio = gameObject.GetComponent<audioController>();
		Music = tmp[0]; 

		for (int i=0; i<EffectSrcNo; i++) {
						oneSound [i] = tmp [i + 1];
			oneSound[i].maxDistance=MaxDistance;
				}
		globV.wantSounds=(PlayerPrefs.GetInt ("WantSounds")!=-1) ? true : false;
		globV.wantMusic=(PlayerPrefs.GetInt ("WantMusic")!=-1) ? true : false;
		soundTogler.SetActive (!globV.wantSounds);
		musicTogler.SetActive (!globV.wantMusic);
		if (globV.wantMusic)
		PlayNextSong ();
	}

}
