using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls the AI
public class AiController : MonoBehaviour {

	// The input manager instance
	public InputManager input;

	// The ai's "player"
	public GameObject ai;

	// The player
	public GameObject player;

	// The camera aiming guide, used to calculate the rotation
	public Transform aimingGuide;

	// The maximum error accepted in the distance estimation
	public float distanceError = 10;

	// The maximum error accepted in the horizontal alignment
	public float angleError = 1;

	// The angle error for the current attempt
	private float attemptError;

	// The tolerance for the horizontal alignment when not aiming
	public float yTolerance = 0;

	// The angle between the ai and the player when aiming
	private float aimingYAngle;

	// The angle between the ai and the player
	private float yAngle;

	// The angle at which to fire the bolt
	private float xAngle;

	// Estimate the distance between the ai and the player
	private float distanceEstimation {
		get {
			return distance + (distanceError * Random.Range (-1f, 1f)) / (attempts + 1);
		}
	}

	// The precise distance between the ai and the player
	private float distance;

	// The distance used for calculations for the current attempt
	private float attemptDistance;

	// The last horizontal ajustment, used for smoothing when going too fast
	private float lastY = 0;

	// The amount of shots fired (attempts made)
	private int attempts;

	// Was the aim button pressed?
	private bool aimPressed;

	// Is the ai aiming?
	private bool aiming;

	// Has the ai shot the bolt?
	private bool shot;

	void Start () {
		Vector3 aiPos = ai.transform.position;
		aiPos.y = 0;

		Vector3 playerPos = player.transform.position;
		playerPos.y = 0;

		Vector3 playerHead = player.GetComponent<PlayerBehaviour> ().head.position;

		// Normal angle
		Quaternion rot = ai.transform.rotation;

		Quaternion target;
		ai.transform.LookAt (playerHead);
		target = ai.transform.rotation;

		ai.transform.rotation = rot;

		yAngle = target.eulerAngles.y;

		// Aiming angle
		rot = aimingGuide.transform.rotation;
		aimingGuide.LookAt (playerHead);
		target = aimingGuide.transform.rotation;

		aimingGuide.transform.rotation = rot;

		aimingYAngle = target.eulerAngles.y;

		distance = Vector3.Distance (aiPos, playerPos);

		attemptError = angleError * Random.Range (-1f, 1f);
		attemptDistance = distanceEstimation;
	}

	void Update () {
		if (!input.aiTurn || shot) {
			if (!input.aiTurn && shot) {
				shot = false;
			}
			return;
		}

		if (aimPressed) {
			input.AimButtonOut ();
			aimPressed = false;
		}

		float horizontalAxis = UpdateYAngle ();
		float verticalAxis = 0;

		if (Mathf.Abs(horizontalAxis) < 0.1f && !aiming && !input.cameraBehaviour.locked) {
			input.AimButtonDown (); // aim when close enough
			aimPressed = true;
			aiming = true;
			SetXAngle ();
		} else if (Mathf.Abs(horizontalAxis) < 0.1f && aiming) {
			verticalAxis = UpdateXAngle ();

			if (Mathf.Abs(horizontalAxis) <= 1e-3 && Mathf.Abs(verticalAxis) <= 1e-3) {
				Debug.LogFormat ("Distance = {0};\nAttempt Error = {1};", attemptDistance, attemptError);

				input.ShootButtonDown ();
				shot = true;
				++attempts;

				attemptError = angleError * Random.Range (-1f, 1f);
				attemptDistance = distanceEstimation;

				aiming = false;
			}
		}

		input.SetAxes (horizontalAxis / 2, verticalAxis / 2);
	}

	// Find the horizontal rotation change needed
	private float UpdateYAngle () {
		if (aiming) {
			return UpdateYAngleAiming ();
		}

		float target = yAngle;
		if (target > 180) {
			target -= 360;
		} else if (target < -180) {
			target += 360;
		}

		float rot = ai.transform.rotation.eulerAngles.y;
		if (rot > 180) {
			rot -= 360;
		}

		float deltaY = target - rot;

		if (Mathf.Abs (deltaY) > yTolerance) {
			deltaY = Mathf.Clamp (deltaY / Data.DefaultSensitivity, -1, 1);
		} else {
			deltaY = 0;
		}

		if (lastY < 0 && deltaY > 0 || lastY > 0 && deltaY < 0) { // Avoid going back and forth eternally
			deltaY = -lastY / 2;
		}

		lastY = deltaY;

		return deltaY;
	}

	// Find the horizontal rotation change needed when aiming
	private float UpdateYAngleAiming () {
		float target = aimingYAngle;
		if (target > 180) {
			target -= 360;
		} else if (target < -180) {
			target += 360;
		}

		float rot = aimingGuide.rotation.eulerAngles.y + attemptError;
		if (rot > 180) {
			rot -= 360;
		}

		float deltaY = target - rot;

		if (deltaY > 180) {
			deltaY -= 180;
		} else if (deltaY < -180) {
			deltaY += 180;
		}

		deltaY = Mathf.Clamp (deltaY / (Data.DefaultSensitivity * Data.DefaultAimingSensitivity), -1, 1);

		if (lastY < 0 && deltaY > 0 || lastY > 0 && deltaY < 0) { // Avoid going back and forth eternally
			deltaY = -lastY / 2;
		}

		lastY = deltaY;

		return deltaY;
	}

	// Find the vertical adjustment to be made
	private float UpdateXAngle () {
		float rot = aimingGuide.localRotation.eulerAngles.x;

		if (rot > 180) {
			rot -= 360;
		}

		float deltaX = xAngle - rot;

		if (deltaX > 180) {
			deltaX -= 180;
		} else if (deltaX < -180) {
			deltaX += 180;
		}

		return -Mathf.Clamp (deltaX / (Data.DefaultSensitivity * Data.DefaultAimingSensitivity), -1, 1);
	}

	// Find the angle at which to fire the bolt in order to hit the player
	private void SetXAngle () {
		float velocity = ai.GetComponentInChildren<CrossbowBehaviour> ().force * Time.fixedDeltaTime;
		xAngle = (Mathf.Asin (distanceEstimation * Physics.gravity.y / Mathf.Pow (velocity, 2)) / 2) * Mathf.Rad2Deg;
	}

	// Check if it's the ai's turn
	public void SwitchingPlayers (GameObject to) {
		input.aiTurn = to == ai;
	}
}
