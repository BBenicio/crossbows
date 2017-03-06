using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Controls the gameplay
public class GameController : MonoBehaviour {

	// The name of the normal camera guide
	private const string NORMAL_CAMERA_GUIDE_NAME = "Camera Guide (Normal)";

	// The name of the aim camera guide
	private const string AIMING_CAMERA_GUIDE_NAME = "Camera Guide (Aiming)";

	// A handle to the mobile controller
	public static GameObject mobileControl;

	// The first player's color used on the outline and win text
	public Color blueColor;

	// The sencond player's color used on the outline and win text
	public Color redColor;

	// The win text object
	public Text winText;

	// The players
	public GameObject[] players;

	// The player Prefab
	public GameObject playerPrefab;

	// The crossbow Prefab
	public GameObject crossbowPrefab;

	// The Prefab for the island 1
	public GameObject island1Prefab;

	// The Prefab for the island 2
	public GameObject island2Prefab;

	// The player's blue
	public Material blueMaterial;

	// The player's red
	public Material redMaterial;

	// The bolt material
	public Material boltMaterial;

	public AiController aiController;

	// The minimum distance allowed between players
	public float minDistance;

	// The maximum distance allowed between players
	public float maxDistance;

	// The FOV used when not aiming
	public float normalFOV = 60;

	// The FOV used when aiming
	public float aimingFOV = 40;

	private float aimButtonHitTime;

	private bool aiming;

	// The current player's index
	private int currentPlayer = 0;

	// The main camera
	private GameObject mainCamera;

	// The current crossbow's CrossbowBehaviour script
	private CrossbowBehaviour crossbow;

	// The MouseLook script
	private MouseLook mouseLook;

	// Should the next update switch the players?
	// Has the current players turn ended?
	private bool shouldSwitchPlayers;

	// Has the game ended?
	private bool gameOver = false;

	// The next player's index
	private int nextPlayer {
		get {
			return (currentPlayer + 1) % 2;
		}
	}

	// Setup the game
	void Awake () {
		SetupPlayers ();
		SetupCrossbows ();
		SetupIslands ();
		SetupCamera ();
		SetupMouseLook ();

		SetupInput ();
	}

	// Spawn the players
	void SetupPlayers () {
		players = new GameObject[2];

		// Player 1 (blue)
		players [0] = GameObject.Instantiate (playerPrefab, new Vector3 (0, 1, 0), new Quaternion ());
		players [0].GetComponentInChildren<SkinnedMeshRenderer> ().material = blueMaterial;

		Vector3 pos = Random.onUnitSphere;
		pos.y = 0;
		pos.Normalize ();

		pos *= Random.Range (minDistance, maxDistance);
		pos.y = 1;

		// PLayer 2 (red)
		players [1] = GameObject.Instantiate (playerPrefab, pos, new Quaternion ());
		players [1].transform.Rotate (Vector3.up, Random.Range (0, 360));
		players [1].GetComponentInChildren<SkinnedMeshRenderer> ().material = redMaterial;

		Debug.LogFormat ("Distance is {0}", Vector3.Distance(players[0].transform.position, players[1].transform.position));

		SetOutline (Color.clear, redColor);

		if (aiController != null) {
			aiController.player = players [0];
			aiController.ai = players [1];
		}
	}

	// Make the crossbows
	void SetupCrossbows () {
		CrossbowBehaviour prefabCrossbowBehaviour = crossbowPrefab.GetComponent<CrossbowBehaviour> ();

		for (int i = 0; i < 2; ++i) {
			GameObject crossbowInstance = GameObject.Instantiate (crossbowPrefab,
				players [i].transform.FindChild (NORMAL_CAMERA_GUIDE_NAME));
			crossbowInstance.transform.localPosition = prefabCrossbowBehaviour.idlePosition;
			crossbowInstance.transform.localRotation = Quaternion.Euler (prefabCrossbowBehaviour.idleRotation);
		}

		if (aiController != null) {
			aiController.aimingGuide = players [1].transform.FindChild (AIMING_CAMERA_GUIDE_NAME);
		}
	}

	// Set the islands
	void SetupIslands () {
		var val = Random.Range (0, 4);

		MakeIsland ((val & 1) == 1 ? island2Prefab : island1Prefab, 0);
		MakeIsland ((val & 2) == 1 ? island2Prefab : island1Prefab, 1);

		/*switch (val) {
		case 0:
			MakeIsland (island1Prefab, 0);
			MakeIsland (island1Prefab, 1);
			break;
		case 1:
			MakeIsland (island1Prefab, 0);
			MakeIsland (island2Prefab, 1);
			break;
		case 2:
			MakeIsland (island2Prefab, 0);
			MakeIsland (island1Prefab, 1);
			break;
		case 3:
			MakeIsland (island2Prefab, 0);
			MakeIsland (island2Prefab, 1);
			break;
		}*/
	}

	// Create an individual island
	void MakeIsland (GameObject islandPrefab, int player) {
		Vector3 pos = players [player].transform.position;

		var island = GameObject.Instantiate (islandPrefab, pos, Quaternion.Euler (-90, 0, 0));

		pos.y = islandPrefab.transform.position.y;
		players [player].transform.position = pos;

		pos.y = 0;
		island.transform.position = pos;
	}

	// Setup the camera for the current player
	void SetupCamera () {
		if (mainCamera == null) {
			mainCamera = GameObject.FindWithTag ("MainCamera");
		}

		CameraBehaviour cameraBehaviour = mainCamera.GetComponent<CameraBehaviour> ();
		crossbow = players [currentPlayer].GetComponentInChildren<CrossbowBehaviour> ();

		cameraBehaviour.currentPlayer = players [currentPlayer].transform;
		cameraBehaviour.aimGuide = crossbow.aimingGuide = players[currentPlayer].transform.FindChild(AIMING_CAMERA_GUIDE_NAME);
		cameraBehaviour.playerGuide = crossbow.normalGuide = players[currentPlayer].transform.FindChild(NORMAL_CAMERA_GUIDE_NAME);

		crossbow.active = true;
	}

	// Setup the MouseLook for the current player
	void SetupMouseLook () {
		mouseLook = GetComponent<MouseLook> ();

		mouseLook.player = players [currentPlayer];
		mouseLook.playerBehaviour = players [currentPlayer].GetComponent<PlayerBehaviour> ();
		mouseLook.aimingGuide = crossbow.aimingGuide.gameObject;
		mouseLook.normalGuide = crossbow.normalGuide.gameObject;
		mouseLook.crossbow = crossbow.gameObject;
	}

	// Enable the mobile input only if on mobile
	void SetupInput () {
		mobileControl = GameObject.Find ("Mobile Control");
		#if UNITY_ANDROID
		mobileControl.SetActive(true);
		#else
		mobileControl.SetActive(false);
		#endif
	}

	void Update () {
		if (Input.GetButtonUp ("Cancel") || (gameOver && (InputManager.GetAimButton ().justPressed ||
			InputManager.GetShootButton ().justPressed))) {

			Menu ();
		}

		#if UNITY_ANDROID
		if (gameOver && Input.touchCount > 0) {
			foreach (Touch t in Input.touches) {
				if (t.phase == TouchPhase.Began) {
					Menu ();
					break;
				}
			}
		}
		#endif

		if (InputManager.GetAimButton ().justPressed) {
			aimButtonHitTime = Time.time;
			aiming = !aiming;
		} else if (InputManager.GetShootButton ().justPressed && aiming && crossbow.active) {
			aiming = false;
			aimButtonHitTime = Time.time;

			if (nextPlayer == 0) {
				SetOutline (Color.clear, redColor);
			} else {
				SetOutline (blueColor, Color.clear);
			}
		}

		if (aimButtonHitTime != 0) {
			mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp (aiming ? normalFOV : aimingFOV, aiming ? aimingFOV : normalFOV,
				(Time.time - aimButtonHitTime) / 0.25f);
			
			if (Time.time - aimButtonHitTime >= 0.25f) {
				aimButtonHitTime = 0;
			}
		}

		// DEBUG //
		if (Input.GetKeyDown (KeyCode.R)) {
			SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
		}
		// DEBUG //
	}

	// Go back to the menu
	void Menu () {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		SetOutline (Color.clear, Color.clear);

		SceneManager.LoadScene ("menu");
	}
		
	void LateUpdate () {
		if (shouldSwitchPlayers) {
			SwitchPlayers ();
			shouldSwitchPlayers = false;
		}
	}

	// A bolt hit something. A turn has ended.
	public void BoltHit () {
		shouldSwitchPlayers = true;
	}

	// Set the outline for the players. Use Color.clear to hide
	private void SetOutline (Color blue, Color red) {
		blueMaterial.SetColor ("_OutlineColor", blue);
		redMaterial.SetColor ("_OutlineColor", red);
	}

	// Switch to the next player or, if it's dead, end the game
	public void SwitchPlayers () {
		Debug.LogFormat ("Switching to player {0}", nextPlayer + 1);

		currentPlayer = nextPlayer;

		bool dead = players [currentPlayer].GetComponent<PlayerBehaviour> ().health <= 0;
		if (dead) { // If the next player is dead enable the crossbow's colliders and rigidbody
			Debug.LogFormat ("Player {0} is dead, freeing crossbow and switching back", currentPlayer + 1);

			crossbow = players [currentPlayer].GetComponentInChildren<CrossbowBehaviour> ();

			var colliders = crossbow.GetComponentsInChildren<BoxCollider> ();

			foreach (BoxCollider c in colliders) {
				c.enabled = true;
			}

			crossbow.transform.SetParent (null);

			crossbow.GetComponent<Rigidbody> ().useGravity = true;
			crossbow.GetComponent<Rigidbody> ().isKinematic = false; // Let the crossbow fall free

			currentPlayer = nextPlayer;

			bool blueWins = currentPlayer == 0;
			winText.text = string.Format ("{0} Wins", blueWins ? "Blue" : "Red");
			winText.color = blueWins ? blueColor : redColor;
			winText.GetComponent<Animator> ().SetTrigger ("active");

			gameOver = true;

			if (currentPlayer == 0) {
				SetOutline (Color.clear, redColor);
			} else {
				SetOutline (blueColor, Color.clear);
			}

			if (aiController != null) {
				aiController.input.aiTurn = false;
			}

			// TODO play "victory trumpets"
			AudioSource victoryAudio = GetComponent<AudioSource>();
			victoryAudio.volume = Data.sound;
			victoryAudio.Play ();
		} else if (aiController != null) {
			aiController.SwitchingPlayers (players [currentPlayer]);
		}

		crossbow.active = false;

		SetupCamera ();
		SetupMouseLook ();
	}
}
