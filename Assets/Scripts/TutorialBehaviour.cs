﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBehaviour : MonoBehaviour {

	public GameObject look;
	public GameObject aim;
	public GameObject shoot;

	public Animator animator;

	public float lookRequirement = 20;

	private enum Stage {
		Look,
		Aim,
		Shoot,
		Done
	}

	private Stage stage;
	private GameObject player;
	private Camera mainCamera;

	private float moved;

	public void Start () {
		if (Data.tutorial) {
			StartTutorial ();
			Data.tutorial = false;
		} else {
			Destroy (gameObject);
		}
	}

	public void StartTutorial () {
		player = GameObject.FindGameObjectsWithTag ("Player") [1];
		mainCamera = GameObject.FindWithTag ("MainCamera").GetComponent<Camera> ();

		stage = Stage.Look;

		look.SetActive (true);
		aim.SetActive (true);
		shoot.SetActive (true);
	}

	void LookStage () {
		moved += Mathf.Abs (InputManager.GetHorizontalAxis ()) + Mathf.Abs (InputManager.GetVerticalAxis ());
		Vector3 playerViewport = mainCamera.WorldToViewportPoint (player.transform.position);

		if (moved >= lookRequirement && playerViewport.x > 0.25f && playerViewport.x < 0.75f && playerViewport.z > 0) {
			stage = Stage.Aim;

			animator.SetTrigger ("aim");
		}
	}

	void AimStage () {
		if (InputManager.GetAimButton ().pressed) {
			stage = Stage.Shoot;

			animator.SetTrigger ("shoot");
		}
	}

	void ShootStage () {
		if (InputManager.GetShootButton ().pressed) {
			stage = Stage.Done;

			animator.SetTrigger ("done");
		}
	}

	// Update is called once per frame
	void Update () {
		switch (stage) {
		case Stage.Look:
			LookStage ();
			break;
		case Stage.Aim:
			AimStage ();
			break;
		case Stage.Shoot:
			ShootStage ();
			break;
		case Stage.Done:
			if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Finished")) {
				Destroy (gameObject);
			}
			break;
		}
	}
}