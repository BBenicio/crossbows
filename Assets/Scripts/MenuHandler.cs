using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

	void Start () {
		Data.sensitivity = PlayerPrefs.GetFloat ("sensitivity", Data.sensitivity);
		Data.aimingSensitivity = PlayerPrefs.GetFloat ("aimingSensitivity", Data.aimingSensitivity);

		GameObject.Find ("Sensitivity").GetComponentInChildren<Slider> ().value = Data.sensitivity;
		GameObject.Find ("AimingSensitivity").GetComponentInChildren<Slider> ().value = Data.aimingSensitivity;
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
		// TODO play "click"
	}

	// Versus button pressed, make transition and start game
	public void VersusButtonClicked () {
		transition = new Transition (GameObject.FindWithTag ("MainCamera").transform, GameObject.Find ("Play Guide").transform,
			true, true, 0.5f);
		startingVersus = true;
		startingSingle = false;

		panelAnimator.SetTrigger ("play");
	}

	// Single player button pressed, make transition and start game
	public void SinglePlayerButtonClicked () {
		transition = new Transition (GameObject.FindWithTag ("MainCamera").transform, GameObject.Find ("Play Guide").transform,
			true, true, 0.5f);
		startingVersus = false;
		startingSingle = true;

		panelAnimator.SetTrigger ("play");
	}

	// Options button pressed, make transitions
	public void OptionsButtonClicked () {
		transition = new Transition (GameObject.FindWithTag ("MainCamera").transform, GameObject.Find ("Options Guide").transform,
			true, true, 1);
		startingVersus = false;
		startingSingle = false;

		panelAnimator.SetBool ("options", true);
		options = true;
	}

	// Back button pressed, make transitions
	public void BackButtonClicked () {
		transition = new Transition (GameObject.FindWithTag ("MainCamera").transform, GameObject.Find ("Main Guide").transform,
			true, true, 1);
		startingVersus = false;
		startingSingle = false;

		panelAnimator.SetBool ("options", false);
		options = false;

		PlayerPrefs.SetFloat ("sensitivity", Data.sensitivity);
		PlayerPrefs.SetFloat ("aimingSensitivity", Data.aimingSensitivity);
		PlayerPrefs.Save ();
	}

	// Reset button pressed, restore the sensitivity sliders
	public void ResetButtonClicked () {
		Data.sensitivity = Data.DefaultSensitivity;
		Data.aimingSensitivity = Data.DefaultAimingSensitivity;

		GameObject.Find ("Sensitivity").GetComponentInChildren<Slider> ().value = Data.DefaultSensitivity;
		GameObject.Find ("AimingSensitivity").GetComponentInChildren<Slider> ().value = Data.DefaultAimingSensitivity;
	}

	// Main sensitivity slider changed
	public void OnSensitivityChange (float sensitivity) {
		Data.sensitivity = sensitivity;
	}

	// Relative aiming sensitivity changed
	public void OnAimSensitivityChange (float sensitivity) {
		Data.aimingSensitivity = sensitivity;
	}
}
