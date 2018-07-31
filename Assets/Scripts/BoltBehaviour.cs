using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The bolt behaviour
public class BoltBehaviour : MonoBehaviour {
	
	// Time to go before killing the bolt after contact
	public float timeToLive = 10;

	// The bolt's mesh
	public GameObject mesh;

	// The camera guide
	public Transform guide;

	// The amount of damage the bolt carries
	public float damage;

	// If the bolt has hit something
	public bool hit { get; private set; }

	// The rigidbody
	private new Rigidbody rigidbody;

	// The main camera's CameraBehaviour
	private CameraBehaviour cameraBehaviour;

	// The game controller
	private GameController gameController;

	void Start () {
		cameraBehaviour = GameObject.FindWithTag ("MainCamera").GetComponent<CameraBehaviour> ();
		gameController = GameObject.FindWithTag ("GameController").GetComponent<GameController> ();

		rigidbody = GetComponent<Rigidbody> ();
	}

	void Update () {
		if (rigidbody == null) { // if the bolt doesn't have a rigidbody it has either hit something or never been shot
			return;
		}

		if (rigidbody.velocity.y != 0) {
			Vector3 normal = rigidbody.velocity.normalized;

			float angle = normal.y * -90;

			Vector3 eulerAngles = transform.localRotation.eulerAngles;
			eulerAngles.z = Mathf.Min (Mathf.Max (-90, angle), 90);

			transform.localRotation = Quaternion.Euler (eulerAngles);

			if (!hit && transform.position.y < 0) {
				AudioSource splashAudio = GetComponents<AudioSource>()[1];
				splashAudio.volume = Data.sound;
				splashAudio.Play ();

				gameController.BoltHit ();

				Destroy (gameObject, timeToLive);
				cameraBehaviour.BoltHit (0);

				hit = true;
			}
		}

		if (!rigidbody.isKinematic) {
			Vector3 meshEA = mesh.transform.localRotation.eulerAngles;
			if (meshEA.x + 360 * Time.deltaTime >= 90) {
				meshEA.x -= 90;
			}
			meshEA.x += 360 * Time.deltaTime;

			mesh.transform.localRotation = Quaternion.Euler (meshEA);
		} else if (guide != null) {
			transform.position = guide.position;
		}
	}

	void OnTriggerEnter(Collider collider) {
		AudioSource hitAudio = GetComponents<AudioSource>()[0];
		hitAudio.volume = Data.sound;
		hitAudio.Play ();

		if (rigidbody == null) {
			return;
		}

		bool hitPlayer = collider.GetComponentInParent<PlayerBehaviour> () != null;
		bool hitCrossbow = collider.GetComponentInParent<CrossbowBehaviour> () != null;

		if (hitCrossbow) {
			Logger.LogInfo ("Bolt hit a crossbow, do not call GameController.BoltHit()");
		} else {
			gameController.BoltHit ();
		}

		transform.position -= rigidbody.velocity * Time.fixedDeltaTime;

		rigidbody.isKinematic = true;

		//if (collider.attachedRigidbody != null) { // Hit a player
		//if (collider.CompareTag("Player")) {
		if (hitPlayer && !hitCrossbow) {
			collider.transform.root.GetComponent<PlayerBehaviour> ().Hit (this, collider.gameObject);

			transform.SetParent (collider.transform);
		}

		Destroy (rigidbody);
		Destroy (GetComponentInChildren<BoxCollider> ());

		cameraBehaviour.BoltHit (timeToLive);

		rigidbody = null;

		hit = true;
	}

	// Shoot this bolt with a force
	public void Shoot(Vector3 force) {
		transform.SetParent (null);

		guide = null;
		GetComponent<BoxCollider> ().enabled = true;
		rigidbody.useGravity = true;
		rigidbody.isKinematic = false;
		rigidbody.AddForce (force);

		cameraBehaviour.BoltShot (gameObject);

		// TODO play "wind"
	}
}
