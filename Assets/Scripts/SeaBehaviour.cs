using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaBehaviour : MonoBehaviour {
	AudioSource oceanAudio;

	void Awake () {
		oceanAudio = GetComponent<AudioSource> ();
	}

	void Update () {
		oceanAudio.volume = Data.sound;
	}
}
