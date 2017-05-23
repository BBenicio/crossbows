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

	public Dropdown graphicsDropdown;
	public Slider soundSlider;
	public Toggle fullscreenToggle;
	public Toggle tutorialToggle;

	// Going to options?
	private bool options;

	private void Load () {
		//Data.sensitivity = PlayerPrefs.GetFloat ("sensitivity", Data.sensitivity);
		//Data.aimingSensitivity = PlayerPrefs.GetFloat ("aimingSensitivity", Data.aimingSensitivity);
		Data.tutorial = PlayerPrefs.GetInt ("tutorial", 1) == 1 && Data.tutorial;

		OnQualityChanged (PlayerPrefs.GetInt ("quality", Data.quality));
		QualitySettings.SetQualityLevel (Data.quality);

		OnSoundChanged (PlayerPrefs.GetFloat ("sound", Data.sound));

		fullscreenToggle.isOn = Screen.fullScreen;
		tutorialToggle.isOn = Data.tutorial;
	}

	void Start () {
		Load ();

		List<Dropdown.OptionData> options = new List<Dropdown.OptionData> ();
		foreach (string name in QualitySettings.names) {
			options.Add (new Dropdown.OptionData (name));
		}

		//Dropdown qualityDropdown = GameObject.Find ("Graphics").GetComponent<Dropdown> ();
		graphicsDropdown.AddOptions (options);
		graphicsDropdown.value = QualitySettings.GetQualityLevel ();
		graphicsDropdown.RefreshShownValue ();

		soundSlider.value = Data.sound;

		#if UNITY_ANDROID
		fullscreenToggle.gameObject.SetActive (false);
		#endif

		#if UNITY_ADS
		if (Data.hasPlayed && Advertisement.IsReady () && Random.value <= 0.4f) {
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

	public void QuitButtonClicked () {
		ButtonClicked ();

		Application.Quit ();
	}

	private void SetFullscreen(bool fs) {
		if (Screen.resolutions == null || Screen.resolutions.Length == 0) {
			return;
		}

		Resolution r = Screen.resolutions [Screen.resolutions.Length - 1];
		if (!fs) {
			Debug.Log ("OnFullscreenChanged(): windowed 960x540");
			Screen.SetResolution (960, 540, false);
		} else {
			Debug.LogFormat ("OnFullscreenChanged(): fullscreen {0}x{1}", r.width, r.height);
			Screen.SetResolution (r.width, r.height, true);
		}
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

		SetFullscreen (fullscreenToggle.isOn);

		//PlayerPrefs.SetFloat ("sensitivity", Data.sensitivity);
		//PlayerPrefs.SetFloat ("aimingSensitivity", Data.aimingSensitivity);
		PlayerPrefs.SetInt ("quality", Data.quality);
		PlayerPrefs.SetFloat ("sound", Data.sound);
		PlayerPrefs.SetInt ("tutorial", Data.tutorial ? 1 : 0);
		PlayerPrefs.Save ();
	}

	// Reset button pressed, restore the sensitivity sliders
	public void ResetButtonClicked () {
		ButtonClicked ();

		Data.sensitivity = Data.DefaultSensitivity;
		Data.aimingSensitivity = Data.DefaultAimingSensitivity;
		Data.quality = Data.DefaultQuality;
		Data.sound = Data.DefaultSound;
		Data.tutorial = true;


		//GameObject.Find ("GraphicsQuality").GetComponentInChildren<Dropdown> ().value = Data.DefaultQuality;
		graphicsDropdown.value = Data.DefaultQuality;
		//GameObject.Find ("SoundSlider").GetComponentInChildren<Slider> ().value = Data.DefaultSound;
		soundSlider.value = Data.DefaultSound;

		tutorialToggle.isOn = true;
		fullscreenToggle.isOn = false;
	}

	// Quality setting changed
	public void OnQualityChanged (int quality) {
		Data.quality = quality;
	}

	public void OnFullscreenChanged (bool fs) {
		/*if (Screen.resolutions == null || Screen.resolutions.Length == 0) {
			return;
		}

		Resolution r = Screen.resolutions [Screen.resolutions.Length - 1];
		if (!fs) {
			Debug.Log ("OnFullscreenChanged(): windowed 960x540");
			Screen.SetResolution (960, 540, false);
		} else {
			Debug.LogFormat ("OnFullscreenChanged(): fullscreen {0}x{1}", r.width, r.height);
			Screen.SetResolution (r.width, r.height, true);
		}*/
		Debug.LogFormat ("OnFullscreenChanged({0})", fs);
	}

	public void OnSoundChanged (float sound) {
		Data.sound = sound;
	}

	public void OnTutorialChanged (bool tutorial) {
		Data.tutorial = tutorial;
	}
}
