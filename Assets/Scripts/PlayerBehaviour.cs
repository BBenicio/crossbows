using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The player behaviour
public class PlayerBehaviour : MonoBehaviour {

	// The player's head
	public Transform head;

	// The player's right arm
	public Transform rightArm;

	// The player's left arm
	public Transform leftArm;

	// The player's left arm's angles when ready
	public Vector3 leftArmReadyAngles;

	// The player's left arm's angles when aiming
	public Vector3 leftArmAimingAngles;

	// The player's right arm's angles when ready
	public Vector3 rightArmReadyAngles;

	// The player's right arm's angles when aiming
	public Vector3 rightArmAimingAngles;

	// The player's head's angles when ready
	public Vector3 headReadyAngles;

	// The player's head's angles when aiming
	public Vector3 headAimingAngles;

	// The animator
	private Animator animator;

	// The camera
	private new Transform camera;

	// The health
	public float health;

	// The amount of rotation accumulated
	private float accumulatedRotation;

	void Start () {
		animator = GetComponentsInChildren<Animator> ()[0];
		camera = GameObject.FindWithTag ("MainCamera").transform;
	}

	// Rotate vertically (head and arms)
	public void Rotate (float rot) {
		accumulatedRotation += rot;
	}

	// Reset the vertical rotation
	public void ResetRotation () {
		accumulatedRotation = 0;
	}

	// Aim the crossbow
	public void Aim () {
		animator.SetBool ("aim", !animator.GetBool("aim"));
	}

	// Shoot the crossbow
	public void Shoot () {
		animator.SetTrigger ("shot");
		animator.SetBool ("aim", !animator.GetBool("aim"));
	}

	void Update () {
		bool ready = camera.parent == transform;
		bool aiming = animator.GetBool ("aim");

		animator.SetBool ("ready", ready);

		if (InputManager.GetAimButton ().justPressed && ready) {
			Aim ();
		} else if (InputManager.GetShootButton ().justPressed && (ready || aiming)) {
			Shoot ();
		}
	}

	void LateUpdate () {
		// Correct and adjust the arms' positions because of animations

		if (accumulatedRotation == 0 || !animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"))
			return;

		float correction = accumulatedRotation * 0.25f;
		if (accumulatedRotation > 0) {
			correction = -accumulatedRotation * 0.15f;
		}

		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("ReadyIdle")) {
			leftArm.localRotation = Quaternion.Euler (leftArmReadyAngles.x, leftArmReadyAngles.y + accumulatedRotation + correction,
				leftArmReadyAngles.z);
			rightArm.localRotation = Quaternion.Euler (rightArmReadyAngles.x, rightArmReadyAngles.y + accumulatedRotation + correction,
				rightArmReadyAngles.z);
			head.localRotation = Quaternion.Euler (headReadyAngles.x, headReadyAngles.y + accumulatedRotation, headReadyAngles.z);
		} else if (animator.GetCurrentAnimatorStateInfo (0).IsName ("AimingIdle")) {
			leftArm.localRotation = Quaternion.Euler (leftArmAimingAngles.x + correction, leftArmAimingAngles.y + accumulatedRotation + correction,
				leftArmAimingAngles.z);
			if (accumulatedRotation > 0) {
				correction *= 2;
			}
			rightArm.localRotation = Quaternion.Euler (rightArmAimingAngles.x + correction, rightArmAimingAngles.y + accumulatedRotation + correction,
				rightArmAimingAngles.z);
			head.localRotation = Quaternion.Euler (headAimingAngles.x, headAimingAngles.y + accumulatedRotation, headAimingAngles.z);
		}
	}

	// This player was hit by a bolt
	public void Hit (BoltBehaviour bolt, GameObject at) {
		if (bolt.hit) {
			Debug.Log ("This bolt has hit something before; it's invalid. Discarding...");
			return;
		}

		float damage = bolt.damage;

		switch (at.name) {
		case "Head":
			damage *= 3;
			break;
		case "LowerTorso":
			damage *= 1.1f;
			break;
		case "UpperTorso":
			damage *= 1.5f;
			break;
		case "L_UpperArm":
		case "R_UpperArm":
			damage *= 0.5f;
			break;
		case "L_LowerArm":
		case "R_LowerArm":
			damage *= 0.25f;
			break;
		case "L_UpperLeg":
		case "R_UpperLeg":
			damage *= 0.75f;
			break;
		case "L_LowerLeg":
		case "R_LowerLeg":
			damage *= 0.5f;
			break;
		}

		Debug.LogFormat ("{0} was hit at {1} for {2} damage", name, at.name, damage);

		health -= damage;
		if (health <= 0) {
			Debug.LogFormat ("{0} is dead", name);
			animator.SetTrigger ("dead");
		} else {
			Debug.LogFormat ("{0} is hurt", name);
			animator.SetTrigger ("hurt");
		}
	}
}
