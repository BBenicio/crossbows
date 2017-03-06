using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

// The main menu and options handler
public class MenuHandler : MonoBehaviour {

	// The transition
	private Transition transition;

	// Is the game starting as versus?
	private bool startingVersus;

	// Is the game starting as single player?
	private bool startingSingle;

	// The animator for the transitions
	public Animator panelAnimator;

	// Going to options?
	private bool options;

	private void Load () {
		Data.sensitivity = PlayerPrefs.GetFloat ("sensitivity", Data.sensitivity);
		Data.aimingSensitivity = PlayerPrefs.GetFloat ("aimingSensitivity", Data.aimingSensitivity);

		OnQualityChanged (PlayerPrefs.GetInt ("quality", Data.quality));
		QualitySettings.SetQualityLevel (Data.quality);

		OnSoundChanged (PlayerPrefs.GetFloat ("sound", Data.sound));
	}

	void Start () {
		Load ();

		List<Dropdown.OptionData> options = new List<Dropdown.OptionData> ();
		foreach (string name in QualitySettings.names) {
			options.Add (new Dropdown.OptionData (name));
		}

		Dropdown qualityDropdown = GameObject.Find ("GraphicsQuality").GetComponent<Dropdown> ();
		qualityDropdown.AddOptions (options);
		qualityDropdown.value = QualitySettings.GetQualityLevel ();
		qualityDropdown.RefreshShownValue ();

		#if UNITY_ADS
		if (Data.hasPlayed && Advertisement.IsReady ()) {
			Advertisement.Show ();
		}
		#endif
	}

	void Update () {
		bool back = Input.GetButtonUp ("Cancel");

		if (back && !options) {
			Application.Quit ();
		} else if (back && options) {
			BackButtonClicked ();
		}

		if (transition != null && transition.isTransitioning) {
			transition.Update ();
			if (!transition.isTransitioning && startingVersus) {
				Debug.Log ("Loading local_versus");
				SceneManager.LoadScene ("local_versus");
			} else if (!transition.isTransitioning && startingSingle) {
				Debug.Log ("Loading single_player");
				SceneManager.LoadScene ("single_player");
			}
		}
	}

	// Common code for button presses
	private void ButtonClicked () {
		AudioSource selectAudio = GetComponent<AudioSource> ();
		selectAudio.volume = Data.sound;
		selectAudio.Play ();
	}

	// Versus button pressed, make transition and start game
	public void VersusButtonClicked () {
		ButtonClicked ();

		transition = new Transition (GameObject.FindWithTag ("MainCamera").transform, GameObject.Find ("Play Guide").transform,
			true, true, 0.5f);
		startingVersus = true;
		startingSingle = false;

		panelAnimator.SetTrigger ("play");

		Data.hasPlayed = true;
	}

	// Single player button pressed, make transition and start game
	public void SinglePlayerButtonClicked () {
		ButtonClicked ();

		transition = new Transition (GameObject.FindWithTag ("MainCamera").transform, GameObject.Find ("Play Guide").transform,
			true, true, 0.5f);
		startingVersus = false;
		startingSingle = true;

		panelAnimator.SetTrigger ("play");

		Data.hasPlayed = true;
	}

	// Options button pressed, make transitions
	public void OptionsButtonClicked () {
		ButtonClicked ();

		transition = new Transition (GameObject.FindWithTag ("MainCamera").transform, GameObject.Find ("Options Guide").transform,
			true, true, 1);
		startingVersus = false;
		startingSingle = false;

		panelAnimator.SetBool ("options", true);
		options = true;
	}

	// Back button pressed, make transitions
	public void BackButtonClicked () {
		ButtonClicked ();

		transition = new Transition (GameObject.FindWithTag ("MainCamera").transform, GameObject.Find ("Main Guide").transform,
			true, true, 1);
		startingVersus = false;
		startingSingle = false;

		panelAnimator.SetBool ("options", false);
		options = false;

		QualitySettings.SetQualityLevel (Data.quality);

		PlayerPrefs.SetFloat ("sensitivity", Data.sensitivity);
		PlayerPrefs.SetFloat ("aimingSensitivity", Data.aimingSensitivity);
		PlayerPrefs.SetInt ("quality", Data.quality);
		PlayerPrefs.SetFloat ("sound", Data.sound);
		PlayerPrefs.Save ();
	}

	// Reset button pressed, restore the sensitivity sliders
	public void ResetButtonClicked () {
		ButtonClicked ();

		Data.sensitivity = Data.DefaultSensitivity;
		Data.aimingSensitivity = Data.DefaultAimingSensitivity;
		Data.quality = Data.DefaultQuality;
		Data.sound = Data.DefaultSound;

		GameObject.Find ("GraphicsQuality").GetComponentInChildren<Dropdown> ().value = Data.DefaultQuality;
		GameObject.Find ("SoundSlider").GetComponentInChildren<Slider> ().value = Data.DefaultSound;
	}

	// Quality setting changed
	public void OnQualityChanged (int quality) {
		Data.quality = quality;
	}

	public void OnSoundChanged (float sound) {
		Data.sound = sound;
	}
}
