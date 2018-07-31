using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_ANDROID
using GoogleMobileAds.Api;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
#endif

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
	public Button playGamesButton;

	public Button signInButton;
	public Button leaderboardsButton;
	public Button achievementsButton;

	// Going to options?
	private bool options;
	private bool playGames;

	private void SignInToPlayGames () {
		#if UNITY_ANDROID
		Social.localUser.Authenticate((bool success) => {
			if (success) {
				Logger.LogInfo (string.Format ("User authenticated! Name: '{0}'", PlayGamesPlatform.Instance.GetUserDisplayName ()));
				leaderboardsButton.interactable = true;
				achievementsButton.interactable = true;
				signInButton.GetComponentInChildren<Text> ().text = "Sign Out";

				Data.signedIn = true;
			} else {
				Logger.LogWarning ("User not authenticated!");

				Data.signedIn = false;
			}
		});
		#endif
	}

	void Start () {
		Initializer.Load ();

		fullscreenToggle.isOn = Screen.fullScreen;
		tutorialToggle.isOn = Data.tutorial;

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

		bool trySignIn = !Initializer.playGamesInitialized && Data.signedIn;

		Initializer.InitAndroid ();

		if (trySignIn) {
			SignInToPlayGames ();
		} else if (Data.signedIn) {
			Logger.LogInfo (string.Format ("User is already authenticated! Name: '{0}'", PlayGamesPlatform.Instance.GetUserDisplayName ()));
			leaderboardsButton.interactable = true;
			achievementsButton.interactable = true;
			signInButton.GetComponentInChildren<Text> ().text = "Sign Out";
		}
		#else
		playGamesButton.gameObject.SetActive (false);
		#endif
	}

	void Update () {
		bool back = Input.GetButtonUp ("Cancel");

		if (back && !options) {
			Application.Quit ();
		} else if (back && options) {
			BackButtonClicked ();
		} else if (back && playGames) {
			PGBackButtonClicked ();
		}

		if (transition != null && transition.isTransitioning) {
			transition.Update ();
			if (!transition.isTransitioning && startingVersus) {
				Logger.LogInfo ("Loading local_versus");
				SceneManager.LoadScene ("local_versus");
			} else if (!transition.isTransitioning && startingSingle) {
				Logger.LogInfo ("Loading single_player");
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

	public void MultiplayerButtonClicked () {
		ButtonClicked ();

		SceneManager.LoadScene ("multiplayer_menu");
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

	public void PlayGamesButtonClicked () {
		ButtonClicked ();

		panelAnimator.SetBool ("playGames", true);
		playGames = true;
	}

	public void PGSignInButtonClicked () {
		ButtonClicked ();

		#if UNITY_ANDROID
		if (Data.signedIn) {
			Logger.LogInfo ("Sign out button clicked");
			PlayGamesPlatform.Instance.SignOut ();

			leaderboardsButton.interactable = false;
			achievementsButton.interactable = false;
			signInButton.GetComponentInChildren<Text> ().text = "Sign In";

			Data.signedIn = false;
		} else {
			Logger.LogInfo ("Sign in button clicked");
			SignInToPlayGames ();
		}
		#endif
	}

	public void PGLeaderboardsButtonClicked () {
		ButtonClicked ();
		Logger.LogInfo ("Leaderboards button clicked");

		#if UNITY_ANDROID
		PlayGamesPlatform.Instance.ShowLeaderboardUI (null, (UIStatus status) => {
			Logger.LogInfo (string.Format ("ShowLeaderboardUI: {0}", status));
		});
		#endif
	}

	public void PGAchievementsButtonClicked () {
		ButtonClicked ();
		Logger.LogInfo ("Achievements button clicked");

		#if UNITY_ANDROID
		PlayGamesPlatform.Instance.ShowAchievementsUI ((UIStatus status) => {
			Logger.LogInfo (string.Format ("ShowAchievementUI: {0}", status));
		});
		#endif
	}

	public void PGBackButtonClicked () {
		ButtonClicked ();

		panelAnimator.SetBool ("playGames", false);
		playGames = false;
	}

	private void SetFullscreen(bool fs) {
		if (Screen.resolutions == null || Screen.resolutions.Length == 0) {
			return;
		}

		Resolution r = Screen.resolutions [Screen.resolutions.Length - 1];
		if (!fs) {
			Logger.LogInfo ("OnFullscreenChanged(): windowed 960x540");
			Screen.SetResolution (960, 540, false);
		} else {
			Logger.LogInfo (string.Format ("OnFullscreenChanged(): fullscreen {0}x{1}", r.width, r.height));
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

		#if UNITY_ANDROID
		PlayerPrefs.SetInt ("signedIn", Data.signedIn ? 1 : 0);
		#endif

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
			Logger.LogInfo ("OnFullscreenChanged(): windowed 960x540");
			Screen.SetResolution (960, 540, false);
		} else {
			Logger.LogInfo (string.Format ("OnFullscreenChanged(): fullscreen {0}x{1}", r.width, r.height));
			Screen.SetResolution (r.width, r.height, true);
		}*/
		Logger.LogInfo (string.Format ("OnFullscreenChanged({0})", fs));
	}

	public void OnSoundChanged (float sound) {
		Data.sound = sound;
	}

	public void OnTutorialChanged (bool tutorial) {
		Data.tutorial = tutorial;
	}
}
