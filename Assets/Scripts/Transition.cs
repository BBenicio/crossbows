using UnityEngine;

// A linear transition
public class Transition {

	// Is this transition active?
	public bool isTransitioning {
		get {
			return transitioning < time;
		}
	}

	// For how long have we been transitioing
	private float transitioning = 0;

	// The transition total time
	private float time;

	// Transition's source and target
	private Transform transform;

	// Should transition to position?
	private bool doPosition;

	// Should transition to rotation?
	private bool doRotation;

	// Transition's destination
	private	Transform transformTo;

	// Create a linear transition
	public Transition (Transform transform, Transform to, bool position, bool rotation, float time) {
		this.transform = transform;
		transformTo = to;
		doPosition = position;
		doRotation = rotation;

		this.time = time;
		transitioning = 0;
	}

	// Update the transition
	public void Update () {
		transitioning += Time.deltaTime;

		if (doPosition) {
			transform.position = Vector3.Lerp (transform.position, transformTo.position, transitioning / time);
		}
		if (doRotation) {
			transform.rotation = Quaternion.Lerp (transform.rotation, transformTo.rotation, transitioning / time);
		}
	}
}