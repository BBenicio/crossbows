using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The crossbow behaviour
public class CrossbowBehaviour : MonoBehaviour {

	// The force to shoot bolts
	public float force;

	// The guide to use when spawning bolts
	public Transform boltGuide;

	// The bolt prefab
	public GameObject bolt;

	// The player's normal camera guide
	public Transform normalGuide;

	// The player's aiming camera guide
	public Transform aimingGuide;

	// Is this crossbow active?
	public bool active;

	// The position to assume when idle
	public Vector3 idlePosition = new Vector3 (0.5f, -1.5f, 0.15f);

	// The rotation to assume when idle
	public Vector3 idleRotation = new Vector3 (-10, 0, 100);

	// The position to assume when ready
	public Vector3 readyPosition = new Vector3 (0.3f, -0.8f, 1.15f);

	// The rotation to assume when ready
	public Vector3 readyRotation = new Vector3 (0, 90, 0);

	// Is this crossbow in aiming position?
	public bool aiming { get; private set; }

	// A handle to the current bolt
	private GameObject currentBolt;

	private GameController gameController;

	// Time elapsed since the last shot
	private float sinceLastShot;

	// The camera's behaviour
	private CameraBehaviour cameraBehaviour;

	// The animator
	private Animator animator;

	void Start () {
		currentBolt = GameObject.Instantiate (bolt, boltGuide.position, new Quaternion (), transform);
		currentBolt.GetComponent<BoxCollider> ().enabled = false;
		currentBolt.GetComponent<BoltBehaviour> ().guide = boltGuide;

		cameraBehaviour = GameObject.FindWithTag ("MainCamera").GetComponent<CameraBehaviour> ();

		gameController = GameObject.FindWithTag ("GameController").GetComponent<GameController> ();

		animator = GetComponent<Animator> ();
	}

	void Update () {
		if (gameController.paused) {
			return;
		}

		sinceLastShot += Time.deltaTime;

		if (sinceLastShot >= 1 && currentBolt == null && active) {
			currentBolt = GameObject.Instantiate (bolt, boltGuide.position, new Quaternion (), transform);
			currentBolt.GetComponent<BoxCollider> ().enabled = false;
			currentBolt.GetComponent<BoltBehaviour> ().guide = boltGuide;
		} else if (sinceLastShot >= 0.5f && aiming && currentBolt == null) {
			aiming = !aiming;
			animator.SetBool ("aim", aiming);
		}

		if (active && InputManager.GetShootButton ().justPressed && currentBolt != null && !gameController.gameOver) {
			currentBolt.GetComponent<BoltBehaviour> ().Shoot (-transform.right * force);

			currentBolt = null;

			sinceLastShot = 0;

			animator.SetTrigger ("shoot");

			active = false;

			GameObject.FindWithTag ("GameController").GetComponent<MouseLook> ().ResetRotation ();

			AudioSource shootAudio = GetComponent<AudioSource> ();
			shootAudio.volume = Data.sound;
			shootAudio.Play ();

			#if UNITY_ANDROID
			GameObject.FindWithTag ("GameController").GetComponent<InputManager> ().ShootButtonOut ();
			#endif
		} else if (active && InputManager.GetAimButton ().justPressed && !animator.IsInTransition (0) && !gameController.gameOver) {
			aiming = !aiming;
			animator.SetBool ("aim", aiming);

			cameraBehaviour.TransitionTo (aiming ? cameraBehaviour.aimGuide : cameraBehaviour.playerGuide, false, true, false);
			if (aiming) {
				transform.SetParent (aimingGuide, true);
			} 
		}

		/*/ DEBUG //
		if (Input.GetKeyDown (KeyCode.PageUp)) {
			force += 100;
		} else if (Input.GetKeyDown (KeyCode.PageDown)) {
			force -= 100;
		}
		// DEBUG /*/
	}

	void LateUpdate () {
		// Set positions and rotations to override the animations

		if (!aiming && transform.parent != normalGuide && normalGuide != null && animator.GetCurrentAnimatorStateInfo (0).IsName ("Idle")) {
			transform.SetParent (normalGuide, true);
		}
		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Idle") && GetComponent<Rigidbody> ().isKinematic) {
			if (!active) {
				transform.localPosition = idlePosition;
				transform.localRotation = Quaternion.Euler (idleRotation);
			} else {
				transform.localPosition = readyPosition;
				transform.localRotation = Quaternion.Euler (readyRotation);
			}
		} else if (!GetComponent<Rigidbody> ().isKinematic) {
			animator.Stop ();
		}
	}
}