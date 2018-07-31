using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The camera behaviour
public class CameraBehaviour : MonoBehaviour {

	// The normal guide for the player
	public Transform playerGuide;

	// The guide for the bolt
	public Transform boltGuide;

	// The aiming guide for the player
	public Transform aimGuide;

	// The current player
	public Transform currentPlayer;

	// How long to make the transitions
	public float transitionsTime = 0.5f;

	// The time scale when following bolts
	public float boltFollowTimeScale = 0.5f;

	// Can this camera follow bolts?
	public bool canFollowBolt = true;

	// Is this camera locked?
	public bool locked { get; private set; }

	// Is this camera following a bolt?
	private bool followingBolt = false;

	// How long until the camera goes back to the player
	private float timeUntilBack = float.PositiveInfinity;

	// The current transition
	private Transition transition;

	// Should the camera be following a bolt after the transition completes?
	private bool followBoltAfterTransition;

	void Start () {
		locked = true;

		transition = new Transition (transform, playerGuide, true, true, transitionsTime);

		transform.SetParent (currentPlayer);

		followBoltAfterTransition = false;
		Time.timeScale = 0.5f;

		GameController.mobileControl.SetActive (false);
	}
	
	void Update () {
		bool isTransitioning = (transition == null || transition.isTransitioning);
		if (followingBolt && !isTransitioning) {
			transform.position = boltGuide.position;

			if (float.IsPositiveInfinity (timeUntilBack)) {
				return;
			}

			timeUntilBack -= Time.deltaTime;

			if (timeUntilBack <= 0) {
				transition = new Transition (transform, playerGuide, true, true, transitionsTime);

				Time.timeScale = 1;
				boltGuide = null;
				timeUntilBack = float.PositiveInfinity;

				followBoltAfterTransition = false;
				transform.SetParent (currentPlayer);
			}
		} else if (isTransitioning) {
			locked = true;
			transition.Update ();
			if (!transition.isTransitioning) {
				followingBolt = followBoltAfterTransition;
			}
		} else if (locked) {
			locked = false;
			Time.timeScale = 1;

			#if UNITY_ANDROID
			if (!GameObject.FindWithTag ("GameController").GetComponent<InputManager> ().aiTurn) {
				GameController.mobileControl.SetActive(true);
			}
			#endif
		}
	}

	// The bolt (usually the one it's following) has hit something, start going to the player
	public void BoltHit (float time) {
		if (!canFollowBolt)
			return;
		
		if (time > 1) {
			timeUntilBack = Mathf.Min (time - 1, 1);
		} else {
			timeUntilBack = 0;
		}
	}

	// A bolt has been shot, follow it
	public void BoltShot (GameObject bolt) {
		if (!canFollowBolt)
			return;

		followingBolt = true;
		timeUntilBack = float.PositiveInfinity;

		Time.timeScale = boltFollowTimeScale;

		Transform[] boltTransforms = bolt.GetComponentsInChildren <Transform>();

		boltGuide = boltTransforms [1];

		transition = new Transition (transform, boltGuide, true, false, transitionsTime);

		followBoltAfterTransition = true;
		transform.SetParent (boltTransforms [0]);

		#if UNITY_ANDROID
		GameController.mobileControl.SetActive(false);
		#endif
	}

	// Is this camera following a bolt?
	public bool IsFollowingBolt () {
		return followingBolt;
	}

	// Create and start a transition
	public void TransitionTo(Transform to, bool rotation, bool position, bool follow) {
		transition = new Transition (transform, to, position, rotation, transitionsTime);
		followBoltAfterTransition = follow;
	}
}
