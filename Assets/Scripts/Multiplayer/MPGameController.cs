using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_ANDROID
using GoogleMobileAds.Api;
using GooglePlayGames;
#endif

namespace Multiplayer
{
	// Controls the gameplay
	public class MPGameController : GameController {
		public const int PLAYER_POS = 0;
		public const int ISLAND_TYPE = 1;
		public const int PLAYER_UPDATE = 2;
		public const int AIM_PRESS = 10;
		public const int AIM_RELEASE = 11;
		public const int SHOOT_PRESS = 13;
		public const int SHOOT_RELEASE = 14;

		public float updateInterval = 0.1f;

		private RTMultiplayerListener rtml;
		private int local = 0;
		private bool host;

		private InputManager input;
		private Vector2 prevRot;
		private float curRot;
		private Vector2 targetRot;
		private float updateTime = 0;

		private bool aimPressed = false;
		private bool shot = false;

		void Awake () {
			rtml = GameObject.Find ("RTMP").GetComponent<RTMultiplayerListener> ();
			rtml.controller = this;

			input = GetComponent<InputManager> ();

			if (PlayGamesPlatform.Instance.RealTime.GetSelf ().CompareTo (rtml.First) == 0) {
				local = 0;
				host = true;
			} else {
				local = 1;
				host = false;
			}

			SetupPlayers ();
			SetupCrossbows ();
			SetupIslands ();
			SetupCamera ();
			SetupMouseLook ();

			SetupInput ();

			LoadAd ();
		}

		public void PeerDisconnected () {
			PlayGamesPlatform.Instance.RealTime.LeaveRoom ();

			// TODO something
			Logger.LogInfo ("Peer disconnected, leaving");

			Menu ();
		}

		public void SendAim (bool press) {
			if (currentPlayer == local) {
				Logger.LogInfo (string.Format ("Sending AIM_{0}", press ? "PRESS" : "RELEASE"));

				Message msg = new Message (press ? AIM_PRESS : AIM_RELEASE);
				Vector2 rot = new Vector2 (mouseLook.camera.transform.localEulerAngles.x, players [currentPlayer].transform.localEulerAngles.y);
				msg.Add (rot);
				PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, msg.GetBytes ());
			}
		}

		public void SendShoot (bool press) {
			if (currentPlayer == local) {
				Logger.LogInfo (string.Format ("Sending SHOOT_{0}", press ? "PRESS" : "RELEASE"));

				Message msg = new Message (press ? SHOOT_PRESS : SHOOT_RELEASE);
				Vector2 rot = new Vector2 (mouseLook.camera.transform.localEulerAngles.x, players [currentPlayer].transform.localEulerAngles.y);
				msg.Add (rot);
				PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, msg.GetBytes ());
			}
		}

		public void MessageRecieved (byte[] data) {
			Message msg = new Message (data);

			if (msg.code == PLAYER_POS) { // 'host' sent local position and rotation
				Logger.LogInfo ("Got position");

				players [1].transform.position = msg.GetVector3 (0);
				players [1].transform.rotation = msg.GetQuaternion (1);

				curRot = 0;
				prevRot = new Vector2 (mouseLook.camera.transform.localEulerAngles.x, players [currentPlayer].transform.localEulerAngles.y);
				targetRot = new Vector2 (prevRot.x, prevRot.y);

				players [1].SetActive (true);
				Logger.LogInfo (string.Format ("Distance is {0}", Vector3.Distance (players [0].transform.position, players [1].transform.position)));
			} else if (msg.code == ISLAND_TYPE) { // 'host' sent islands
				Logger.LogInfo ("Got island");
				int val = msg.GetInt (0);

				MakeIsland ((val & 1) == 1 ? island2Prefab : island1Prefab, 0);
				MakeIsland ((val & 2) == 1 ? island2Prefab : island1Prefab, 1);
			} else if (msg.code == PLAYER_UPDATE && !shot) {
				prevRot = targetRot;
				targetRot = msg.GetVector2 (0);
				updateTime = Time.time;
			} else if (msg.code == AIM_PRESS) {
				Logger.LogInfo ("Got AIM_PRESS");

				targetRot = msg.GetVector2 (0);
				updateTime = Time.time;

				prevRot.x = mouseLook.camera.transform.localEulerAngles.x;
				prevRot.y = players [currentPlayer].transform.localEulerAngles.y;
				UpdatePlayerRot (targetRot.x, targetRot.y);

				Aim ();
			}  else if (msg.code == AIM_RELEASE) {
				/*Logger.LogInfo ("Got AIM_RELEASE");

				targetRot = msg.GetVector2 (0);
				updateTime = Time.time;

				UpdatePlayerRot (targetRot.x, targetRot.y);
				prevRot = targetRot;*/
			} else if (msg.code == SHOOT_PRESS) {
				Logger.LogInfo ("Got SHOOT_PRESS");

				targetRot = msg.GetVector2 (0);
				updateTime = Time.time;

				UpdatePlayerRot (targetRot.x, targetRot.y);
				prevRot = targetRot;

				Shoot ();

				shot = true;
			} else if (msg.code == SHOOT_RELEASE) {
				/*Logger.LogInfo ("Got SHOOT_RELEASE");

				targetRot = msg.GetVector2 (0);
				updateTime = Time.time;

				UpdatePlayerRot (targetRot.x, targetRot.y);
				prevRot = targetRot;*/
			}
		}

		private void Aim () {
			aimButtonHitTime = Time.time;
			aiming = !aiming;

			crossbow.Aim ();
			players [currentPlayer].GetComponent<PlayerBehaviour> ().Aim ();
		}

		private void Shoot () {
			crossbow.Shoot ();
			players [currentPlayer].GetComponent<PlayerBehaviour> ().Shoot ();

			if (aiming) {
				aiming = false;
				aimButtonHitTime = Time.time;
			}

			if (nextPlayer == 0) {
				SetOutline (Color.clear, redColor);
			} else {
				SetOutline (blueColor, Color.clear);
			}
		}

		// Spawn the players
		protected override void SetupPlayers () {
			players = new GameObject[2];

			// Player 1 (blue)
			players [0] = GameObject.Instantiate (playerPrefab, new Vector3 (0, 1, 0), new Quaternion ());
			players [0].GetComponentInChildren<SkinnedMeshRenderer> ().material = blueMaterial;
			players [0].GetComponent<PlayerBehaviour> ().gameController = this;
			players [0].name = rtml.First.DisplayName;

			Vector3 pos = Random.onUnitSphere;
			pos.y = 0;
			pos.Normalize ();

			pos *= Random.Range (minDistance, maxDistance);
			pos.y = 1;

			// PLayer 2 (red)
			players [1] = GameObject.Instantiate (playerPrefab, pos, new Quaternion ());
			players [1].transform.Rotate (Vector3.up, Random.Range (0, 360));
			players [1].GetComponentInChildren<SkinnedMeshRenderer> ().material = redMaterial;
			players [1].GetComponent<PlayerBehaviour> ().gameController = this;
			players [1].name = rtml.Second.DisplayName;

			if (host) {
				Logger.LogInfo (string.Format ("Distance is {0}", Vector3.Distance (players [0].transform.position, players [1].transform.position)));

				Logger.LogInfo ("Is host, sending red position");

				Message msg = new Message (PLAYER_POS);
				msg.Add (players [1].transform.position);
				msg.Add (players [1].transform.rotation);

				PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, msg.GetBytes ());
			} else {
				input.aiTurn = true;

				Logger.LogInfo ("Is client, awating position");
				players [1].SetActive (false);
			}

			SetOutline (Color.clear, redColor);
		}

		// Set the islands
		protected override void SetupIslands () {
			if (host) {
				Logger.LogInfo ("Sending island");

				var val = Random.Range (0, 4);

				MakeIsland ((val & 1) == 1 ? island2Prefab : island1Prefab, 0);
				MakeIsland ((val & 2) == 1 ? island2Prefab : island1Prefab, 1);

				Message msg = new Message (ISLAND_TYPE);
				msg.Add (val);
				PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, msg.GetBytes ());
			}
		}

		protected override void SetupInput () {
			base.SetupInput ();

			mobileControl.SetActive (false);
		}

		private void UpdatePlayerRot (float x, float y) {
			Vector3 ea = players [currentPlayer].transform.localEulerAngles;
			ea.y = y;
			players [currentPlayer].transform.localEulerAngles = ea;

			x = x - mouseLook.camera.transform.localEulerAngles.x;

			mouseLook.camera.transform.RotateAround(mouseLook.playerBehaviour.head.position, mouseLook.playerBehaviour.transform.right, x);
			mouseLook.normalGuide.transform.RotateAround(mouseLook.playerBehaviour.head.position, mouseLook.playerBehaviour.transform.right, x);
			mouseLook.aimingGuide.transform.RotateAround(mouseLook.playerBehaviour.head.position, mouseLook.playerBehaviour.transform.right, x);
			mouseLook.playerBehaviour.Rotate (-x);
		}

		void Update () {
			rtml.DoLostMessages ();

			if (gameOver) {
				return;
			}

			if (Input.GetButtonUp ("Cancel")) {
				Pause ();
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

			if (paused) {
				return;
			}
				
			if (currentPlayer != local) {
				UpdatePlayerRot (Mathf.LerpAngle (prevRot.x, targetRot.x, (Time.time - updateTime) / updateInterval), Mathf.LerpAngle (prevRot.y, targetRot.y, (Time.time - updateTime) / updateInterval));
			} else if (Time.time - updateTime >= updateInterval) {
				Message msg = new Message (PLAYER_UPDATE);

				Vector2 rot = new Vector2 (mouseLook.camera.transform.localEulerAngles.x, players [currentPlayer].transform.localEulerAngles.y);
				msg.Add (rot);
				PlayGamesPlatform.Instance.RealTime.SendMessageToAll (false, msg.GetBytes ());

				updateTime = Time.time;
			}


			if (InputManager.GetAimButton ().justPressed) {
				aimButtonHitTime = Time.time;
				aiming = !aiming;
			} else if (InputManager.GetShootButton ().justPressed && crossbow.active) {
				if (aiming) {
					aiming = false;
					aimButtonHitTime = Time.time;
				}

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

			if (aimPressed) {
				input.AimButtonUp ();
				aimPressed = false;
			}
		}

		// Go back to the menu
		public override void Menu () {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			SetOutline (Color.clear, Color.clear);

			PlayGamesPlatform.Instance.RealTime.LeaveRoom ();
			GameObject.Destroy (rtml.gameObject);

			SceneManager.LoadScene ("multiplayer_menu");
		}

		// A bolt hit something. A turn has ended.
		public override void BoltHit () {
			shouldSwitchPlayers = true;
			shotByMainPlayer = currentPlayer == local;
		}

		// Switch to the next player or, if it's dead, end the game
		public override void SwitchPlayers () {
			shot = false;

			Logger.LogInfo (string.Format ("Switching to player {0}", nextPlayer + 1));

			currentPlayer = nextPlayer;
			input.aiTurn = currentPlayer != local;

			int distance = Mathf.RoundToInt(Vector3.Distance(players[0].transform.position, players[1].transform.position) * 10);

			#if UNITY_ANDROID
			if (shotByMainPlayer && noScope) {
				if (hit) {
					Social.ReportScore(distance, GPGSIds.leaderboard_no_scope_distance, (bool success) => {
						if (success)
							Logger.LogInfo (string.Format ("Leaderboard: No Scope {0} - Success", distance));
						else
							Logger.LogWarning (string.Format ("Leaderboard: No Scope {0} - Fail", distance));
					});

					Social.ReportProgress(GPGSIds.achievement_no_scope, 100, (bool success) => {
						if (success)
							Logger.LogInfo ("Achievement: No Scope! - Success");
						else
							Logger.LogWarning ("Achievement: No Scope! - Fail");
					});
				}
				if (headshot) {
					Social.ReportProgress(GPGSIds.achievement_no_scope_headshot, 100, (bool success) => {
						if (success)
							Logger.LogInfo ("Achievement: No Scope Headshot! - Success");
						else
							Logger.LogWarning ("Achievement: No Scope Headshot! - Fail");
					});
				}
			}
			#endif

			bool dead = players [currentPlayer].GetComponent<PlayerBehaviour> ().health <= 0;
			if (dead) { // If the next player is dead enable the crossbow's colliders and rigidbody
				Logger.LogInfo (string.Format ("Player {0} is dead, freeing crossbow and switching back", currentPlayer + 1));

				#if UNITY_ANDROID
				if (currentPlayer == local && players [nextPlayer].GetComponent<PlayerBehaviour> ().health == 30) {
					Social.ReportProgress(GPGSIds.achievement_dominated, 100, (bool success) => {
						if (success)
							Logger.LogInfo ("Achievement: Dominated - Success");
						else
							Logger.LogWarning ("Achievement: Dominated - Fail");
					});
				}

				if (shotByMainPlayer) {
					if (players [local].GetComponent<PlayerBehaviour> ().health == 30) {
						Social.ReportProgress(GPGSIds.achievement_dominator, 100, (bool success) => {
							if (success)
								Logger.LogInfo ("Achievement: Dominator - Success");
							else
								Logger.LogWarning ("Achievement: Dominator - Fail");
						});
					} else if (players [local].GetComponent<PlayerBehaviour> ().health <= 10) {
						Social.ReportProgress(GPGSIds.achievement_close_contest, 100, (bool success) => {
							if (success)
								Logger.LogInfo ("Achievement: Close Contest - Success");
							else
								Logger.LogWarning ("Achievement: Close Contest - Fail");
						});
					}
					if (headshot) {
						PlayGamesPlatform.Instance.IncrementAchievement (GPGSIds.achievement_head_hunter, 1, (bool success) => {
							if (success)
								Logger.LogInfo ("Achievement: Head Hunter - Success");
							else
								Logger.LogWarning ("Achievement: Head Hunter - Fail");
						});
						Social.ReportProgress(GPGSIds.achievement_headshot, 100, (bool success) => {
							if (success)
								Logger.LogInfo ("Achievement: Headshot - Success");
							else
								Logger.LogWarning ("Achievement: Headshot - Fail");
						});
						Social.ReportScore(distance, GPGSIds.leaderboard_headshot_distance, (bool success) => {
							if (success)
								Logger.LogInfo (string.Format ("Leaderboard: Headshot {0} - Success", distance));
							else
								Logger.LogWarning (string.Format ("Leaderboard: Headshot {0} - Fail", distance));
						});
					}

				}
				#endif

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

				hud.SetWinText (string.Format ("{0} Wins", blueWins ? players[0].name : players[1].name), blueWins ? blueColor : redColor);
				hud.Enable (true);
				mouseLook.lockCursor = false;

				#if UNITY_ANDROID
				Destroy (mobileControl);
				#endif

				gameOver = true;

				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;

				SetOutline (Color.clear, Color.clear);

				if (aiController != null) {
					aiController.input.aiTurn = false;
				}

				AudioSource victoryAudio = GetComponent<AudioSource>();
				victoryAudio.volume = Data.sound;
				victoryAudio.Play ();

				// GameOver ();
			}

			crossbow.active = false;

			SetupCamera ();
			SetupMouseLook ();

			hit = false;
			shotByMainPlayer = false;
			headshot = false;
			noScope = false;
		}
	}
}