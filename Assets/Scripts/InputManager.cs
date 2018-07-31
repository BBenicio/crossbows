using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The input manager
public class InputManager : MonoBehaviour {

	// The label for the horizontal axis
	private const string HORIZONTAL_AXIS = "Horizontal";

	// The label for the vertical axis
	private const string VERTICAL_AXIS = "Vertical";

	// The label for the shoot button
	private const string SHOOT_BUTTON = "Shoot";

	// The label for the aim button
	private const string AIM_BUTTON = "Aim";

	// The virtual button
	public class Button {

		// Is this button pressed
		public bool pressed { get; private set; }

		// Was the button pressed this frame
		public bool justPressed {
			get {
				#if UNITY_ANDROID
				return downFrame > Time.frameCount - 1;
				#else
				return downFrame >= Time.frameCount - 1;
				#endif
			}
		}

		// Was the button released this frame
		public bool justReleased {
			get {
				#if UNITY_ANDROID
				return upFrame > Time.frameCount - 1;
				#else
				return upFrame >= Time.frameCount - 1;
				#endif
			}
		}

		// Frame when the button was pressed
		private int downFrame = -1;

		// Frame when the button was released
		private int upFrame = -1;

		// Press the button
		public void Press () {
			pressed = true;
			downFrame = Time.frameCount;
			upFrame = -1;
		}

		// Release the button
		public void Release () {
			pressed = false;
			downFrame = -1;
			upFrame = Time.frameCount;
		}

		// Cancel the press
		public void Cancel () {
			pressed = false;
			downFrame = -1;
			upFrame = -1;
		}
	}

	// The horizontal axis
	private float horizontalAxis;

	// The vertical axis
	private float verticalAxis;

	// The shoot button
	private Button shootButton = new Button ();

	// The aim button
	private Button aimButton = new Button ();

	// The camera behaviour (It's a hack, shouldn't exist)
	public CameraBehaviour cameraBehaviour { get; private set; }

	// Is it the ai's turn?
	public bool aiTurn;

	#if UNITY_ANDROID
	private bool isMP = false;
	private Multiplayer.MPGameController controller = null;
	#endif
	private

	void Awake () {
		instance = this;

		cameraBehaviour = GameObject.FindWithTag ("MainCamera").GetComponent<CameraBehaviour> ();

		#if UNITY_ANDROID
		isMP = GameObject.Find ("RTMP");
		#endif
	}

	void Start () {
		if (isMP)
			controller = GetComponent<Multiplayer.MPGameController> ();
	}

	void Update () {
		if (!aiTurn) {
			#if UNITY_ANDROID
			MobileInput ();
			#else
			NormalInput ();
			#endif
		}
	}

	#if UNITY_ANDROID
	// Take care of touch input
	void MobileInput () {
		SetAxes (0, 0);

		foreach (Touch touch in Input.touches) {
			if (touch.phase == TouchPhase.Moved && !(shootButton.pressed || aimButton.pressed)) {
				SetAxes (touch.deltaPosition.normalized);
			}
		}
	}

	#else
	// Handle mouse and keyboard
	void NormalInput () {
		horizontalAxis = Input.GetAxis (HORIZONTAL_AXIS);
		verticalAxis = Input.GetAxis (VERTICAL_AXIS);
		
		if (Input.GetButtonDown (SHOOT_BUTTON)) {
			shootButton.Press ();
		} else if (Input.GetButtonUp (SHOOT_BUTTON)) {
			shootButton.Release ();
		}

		if (Input.GetButtonDown (AIM_BUTTON)) {
			aimButton.Press ();
		} else if (Input.GetButtonUp (AIM_BUTTON)) {
			aimButton.Release ();
		}
	}
	#endif

	// Set the horizontal and vertical axes
	public void SetAxes (float horizontal, float vertical) {
		horizontalAxis = Mathf.Clamp (horizontal, -1, 1);
		verticalAxis = Mathf.Clamp (vertical, -1, 1);
	}

	// Set the horizontal and vertical axes
	public void SetAxes (Vector2 axes) {
		SetAxes (axes.x, axes.y);
	}

	// The shoot button was pressed (on mobile)
	public void ShootButtonDown () {
		if (!shootButton.pressed && !cameraBehaviour.locked) {
			Logger.LogInfo ("Shoot pressed");
			shootButton.Press ();

			#if UNITY_ANDROID
			if (isMP)
				controller.SendShoot (true);
			#endif
		}
	}

	// The shoot button was released (on mobile)
	public void ShootButtonUp () {
		if (shootButton.pressed && !cameraBehaviour.locked) {
			Logger.LogInfo ("Shoot released");
			shootButton.Release ();

			#if UNITY_ANDROID
			if (isMP)
				controller.SendShoot (false);
			#endif
		}
	}

	// The shoot button was cancelled (on mobile)
	public void ShootButtonOut () {
		shootButton.Cancel ();
	}

	// The aim button was pressed (on mobile)
	public void AimButtonDown () {
		if (!aimButton.pressed && !cameraBehaviour.locked) {
			Logger.LogInfo ("Aim pressed");
			aimButton.Press ();

			#if UNITY_ANDROID
			if (isMP)
				controller.SendAim (true);
			#endif
		}
	}

	// The aim button was released (on mobile)
	public void AimButtonUp () {
		if (aimButton.pressed && !cameraBehaviour.locked) {
			Logger.LogInfo ("Aim released");
			aimButton.Release ();

			#if UNITY_ANDROID
			if (isMP)
				controller.SendAim (false);
			#endif
		}
	}

	// The aim button was cancelled (on mobile)
	public void AimButtonOut () {
		aimButton.Cancel ();
	}

	// The input manager instance
	private static InputManager instance;

	// Get the shoot button state
	public static Button GetShootButton () {
		return instance.shootButton;
	}

	// Get the aim button state
	public static Button GetAimButton () {
		return instance.aimButton;
	}

	// Get the horizontal axis value
	public static float GetHorizontalAxis () {
		return instance.horizontalAxis;
	}

	// Get the vertical axis value
	public static float GetVerticalAxis () {
		return instance.verticalAxis;
	}
}
