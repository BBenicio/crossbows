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

	public void Enable () {
		canvas.enabled = true;
		eventSystem.SetActive (true);
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
}
