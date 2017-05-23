using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HudBehaviour : MonoBehaviour {
	public Text winText;

	private Canvas canvas;
	private GameObject eventSystem;

	void Awake () {
		canvas = GetComponent<Canvas> ();
		canvas.enabled = false;

		eventSystem = transform.FindChild ("EventSystem").gameObject;
		eventSystem.SetActive (false);
	}

	public void Enable (bool gameOver) {
		canvas.enabled = true;
		eventSystem.SetActive (true);

		transform.FindChild ("ResumeButton").gameObject.SetActive (!gameOver);
		transform.FindChild ("PlayAgainButton").gameObject.SetActive (gameOver);
	}

	public void Disable () {
		canvas.enabled = false;
		eventSystem.SetActive (false);
	}

	public void SetWinText (string text, Color color) {
		winText.text = text;
		winText.color = color;
		winText.GetComponent<Animator> ().SetTrigger ("active");
	}

	public void OnMenuButtonClicked () {
		SceneManager.LoadScene ("menu");
	}

	public void OnPlayAgainButtonClicked () {
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
	}

	public void OnResumeButtonClicked () {
		GameObject.FindWithTag ("GameController").GetComponent<GameController> ().Pause ();
		InputManager.GetShootButton ().Cancel ();
		InputManager.GetAimButton ().Cancel ();
	}
}
