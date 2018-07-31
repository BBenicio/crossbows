using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi.Nearby;
#endif

namespace Multiplayer
{
	public class MultiplayerMenu : MonoBehaviour
	{
		public GameObject nearbyScrollContent;
		public GameObject playerButton;

		public Button quickGameButton;
		public Button inviteButton;
		public Button invitationsButton;

		public GameObject messageBox;

		public bool enableNearby = true;

		#if UNITY_ANDROID

		private string friendlyName;
		private List<EndpointDetails> discoveredPlayers = new List<EndpointDetails> ();

		private MenuDiscoveryAndMessageListener daml;
		private RTMultiplayerListener rtml;

		public GameObject rtmpPrefab;

		public MultiplayerMenu () {
			daml = new MenuDiscoveryAndMessageListener (this);
		}

		void Start () {
			List<string> appIdentifiers = new List<string> ();
			appIdentifiers.Add (PlayGamesPlatform.Nearby.GetAppBundleId ());

			if (Data.signedIn) {
				friendlyName = PlayGamesPlatform.Instance.GetUserDisplayName ();

				PlayGamesPlatform.Instance.RealTime.GetAllInvitations ((invites) => {
					if (invites.Length > 0) {
						invitationsButton.interactable = true;
						invitationsButton.GetComponentInChildren<Text> ().text = string.Format ("Invitations ({0})", invites.Length);
					}
				});
			} else {
				Logger.LogInfo ("User not signed in to Play Games, will only be able to play nearby");

				friendlyName = SystemInfo.deviceName;

				quickGameButton.interactable = false;
				inviteButton.interactable = false;
			}
			if (enableNearby) {
				if (Initializer.nearbyInitialized) {
					PlayGamesPlatform.Nearby.StartAdvertising (
						friendlyName,  // User-friendly name
						appIdentifiers,  // App bundle Id for this game
						TimeSpan.FromSeconds (0),// 0 = advertise forever
						(AdvertisingResult result) => {
							Logger.LogInfo (string.Format ("OnAdvertisingResult = {0} {1}", result.Succeeded, result.Status));
						},
						(ConnectionRequest request) => {
							Logger.LogInfo (string.Format ("Recieved connection request: {0} {1}", request.RemoteEndpoint.Name, request.RemoteEndpoint.EndpointId));
						}
					);
				} else {
					Logger.LogWarning ("Nearby was not initialized");

					GameObject.Find ("Nearby").SetActive (false);

					if (!Data.signedIn) {
						ShowError ("Nearby multiplayer is not available\nSign in to Play Games to play multiplayer");
					}
				}

				PlayGamesPlatform.Nearby.StartDiscovery (PlayGamesPlatform.Nearby.GetServiceId (), TimeSpan.FromSeconds (0), daml);
			} else {
				Logger.LogInfo ("Nearby disabled by developer");
				GameObject.Find ("Nearby").SetActive (false);
			}

			GetComponentInChildren<Animator> ().SetBool ("display", true);

			GameObject rtmp = GameObject.Instantiate (rtmpPrefab);
			rtmp.name = "RTMP";
			rtml = rtmp.GetComponent<RTMultiplayerListener> ();
			rtml.menu = this;
		}

		void Update () {
		}

		public void AddEndpoint (EndpointDetails discoveredEndpoint) {
			discoveredPlayers.Add (discoveredEndpoint);

			GameObject btn = GameObject.Instantiate (playerButton, nearbyScrollContent.transform);
			btn.name = discoveredEndpoint.EndpointId;
			btn.GetComponent<Button> ().onClick.AddListener (() => {
				NearbyButtonClicked (discoveredEndpoint.EndpointId);
			});
			btn.GetComponentInChildren<Text> ().text = discoveredEndpoint.Name;
			btn.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, 50 * (discoveredPlayers.Count - 1));
		}

		public void RemoveEndpoint (string lostEndpointId) {
			discoveredPlayers.RemoveAll((EndpointDetails endpoint) => {
				return endpoint.EndpointId == lostEndpointId;
			});

			GameObject.Destroy (GameObject.Find (lostEndpointId));
		}

		public void ShowError (string msg) {
			messageBox.SetActive (true);

			if (msg != null && msg.Length > 0)
				messageBox.GetComponentInChildren<Text> ().text = msg;
		}

		public void MessageBoxOK () {
			messageBox.SetActive (false);
		}

		public void PlayRTMP () {
			SceneManager.LoadScene ("multiplayer");
		}

		#endif

		private void ButtonClicked () {
			AudioSource selectAudio = GetComponent<AudioSource> ();
			selectAudio.volume = Data.sound;
			selectAudio.Play ();
		}

		public void QuickGameButtonClicked () {
			ButtonClicked ();

			#if UNITY_ANDROID
			Logger.LogInfo ("Starting quick game");
			PlayGamesPlatform.Instance.RealTime.CreateQuickGame (1, 1, 0, rtml);
			#endif
		}

		public void InviteButtonClicked () {
			ButtonClicked ();

			#if UNITY_ANDROID
			Logger.LogInfo ("Opening invitation screen");
			PlayGamesPlatform.Instance.RealTime.CreateWithInvitationScreen (1, 1, 0, rtml);
			#endif
		}

		public void InvitationsButtonClicked () {
			ButtonClicked ();

			#if UNITY_ANDROID
			Logger.LogInfo ("Opening invitation inbox");
			PlayGamesPlatform.Instance.RealTime.AcceptFromInbox (rtml);
			#endif
		}

		public void BackButtonClicked () {
			ButtonClicked ();

			GameObject.Destroy (rtml.gameObject);

			GetComponentInChildren<Animator> ().SetBool ("display", false);
			SceneManager.LoadScene ("menu");
		}

		public void NearbyButtonClicked (string endpointId) {
			ButtonClicked ();

			#if UNITY_ANDROID
			PlayGamesPlatform.Nearby.SendConnectionRequest (friendlyName, endpointId, null, (ConnectionResponse response) => {
				Logger.LogInfo (string.Format ("Response {0}", response.ResponseStatus));
			}, daml);
			#endif
		}
	}
}

