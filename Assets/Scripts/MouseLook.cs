using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The mouse look
public class MouseLook : MonoBehaviour {

	// Maximum vertical rotation
	public float maximumX = 80;

	// Minimum vertical rotation
	public float minimumX = -60;

	// Should we lock the cursor?
	public bool lockCursor = true;

	// If we do lock the cursor, keep it visible?
	public bool keepCursorVisible = false;

	// The camera
	public new GameObject camera;

	// The current player
	public GameObject player;

	// The current player's crossbow
	public GameObject crossbow;

	// The current player's normal camera guide
	public GameObject normalGuide;

	// The current player's aiming camera guide
	public GameObject aimingGuide;

	// The current player's behaviour
	public PlayerBehaviour playerBehaviour;

	private GameController gameController;

	// The x axis rotation (vertical)
	private float xRot;

	// The y axis rotation (horizontal)
	private float yRot;

	// Is the cursor locked?
	private bool cursorLocked;

	void Start () {
		#if UNITY_ANDROID
		lockCursor = false;
		keepCursorVisible = true;
		#endif

		gameController = GameObject.FindWithTag ("GameController").GetComponent<GameController> ();

		if (!lockCursor) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			cursorLocked = false;
		} else {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = keepCursorVisible;
			cursorLocked = true;
		}
	}

	// Set the rotations
	public void SetRotation (float x, float y) {
		xRot = x;
		yRot = y;
	}

	void Update () {
		if (cursorLocked && Input.GetKeyUp (KeyCode.Escape)) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			cursorLocked = false;
		} else if (lockCursor && !cursorLocked && (InputManager.GetShootButton ().justPressed || InputManager.GetAimButton().justPressed)) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = keepCursorVisible;
			cursorLocked = true;
		}

		if (camera.GetComponent<CameraBehaviour> ().locked || gameController.gameOver) {
			return;
		}
			
		float sensitivity = Data.GetSensitivity (crossbow.GetComponent<CrossbowBehaviour> ().aiming);

		yRot = InputManager.GetHorizontalAxis () * sensitivity;
		xRot = InputManager.GetVerticalAxis () * sensitivity;

		Quaternion cameraQ = camera.transform.localRotation * Quaternion.Euler(-xRot, 0, 0);
		if (cameraQ.eulerAngles.x >= 360 + minimumX || cameraQ.eulerAngles.x <= maximumX) {
			camera.transform.RotateAround(playerBehaviour.head.position, camera.transform.right, -xRot);
			normalGuide.transform.RotateAround(playerBehaviour.head.position, normalGuide.transform.right, -xRot);
			aimingGuide.transform.RotateAround(playerBehaviour.head.position, aimingGuide.transform.right, -xRot);
			playerBehaviour.Rotate (xRot);
		}

		player.transform.localRotation *= Quaternion.Euler (0, yRot, 0);
	}

	// Reset the vertical rotations
	public void ResetRotation () {
		float angle = -normalGuide.transform.localRotation.eulerAngles.x;

		normalGuide.transform.RotateAround(playerBehaviour.head.position, normalGuide.transform.right, angle);
		aimingGuide.transform.RotateAround(playerBehaviour.head.position, aimingGuide.transform.right, angle);

		playerBehaviour.ResetRotation ();
	}
}
