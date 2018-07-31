using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;
#endif

namespace Multiplayer
{
	#if UNITY_ANDROID
	public class RTMultiplayerListener : MonoBehaviour, RealTimeMultiplayerListener
	{
		public MultiplayerMenu menu;
		public MPGameController controller;

		private bool showingWaitScreen = false;
		private bool inMenu = true;

		public Participant First { get; private set; }
		public Participant Second { get; private set; }

		private List<byte[]> lostMsgs = new List<byte[]> ();

		public void OnRoomSetupProgress (float percent) {
			if (!showingWaitScreen) {
				PlayGamesPlatform.Instance.RealTime.ShowWaitingRoomUI ();
				showingWaitScreen = true;
			}
		}

		public void OnRoomConnected (bool success) {
			if (success) {
				Logger.LogInfo ("Room connected successfully");

				GameObject.DontDestroyOnLoad (gameObject);

				List<Participant> participants = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants ();
				First = participants [0];
				Second = participants [1];

				menu.PlayRTMP ();

				inMenu = false;
				menu = null; // let the GC do its thing
			} else {
				Logger.LogWarning ("Room failed to connect");
				PlayGamesPlatform.Instance.RealTime.LeaveRoom ();

				menu.ShowError ("Failed to connect!");
			}
		}

		public void OnLeftRoom () {
			Logger.LogInfo ("Left the room");

			if (inMenu) {
				menu.ShowError ("");
			}
		}

		public void OnParticipantLeft (Participant participant) {
			Logger.LogInfo (string.Format ("Participant '{0}' left", participant.DisplayName));

			if (inMenu) {
				menu.ShowError (string.Format ("{0} disconnected", participant.DisplayName));
			}
		}

		public void OnPeersConnected (string[] participantIds) {
		}

		public void OnPeersDisconnected (string[] participantIds) {
			if (controller != null) {
				controller.PeerDisconnected ();
			}
		}

		public void OnRealTimeMessageReceived (bool isReliable, string senderId, byte[] data) {
			if (controller != null) {
				controller.MessageRecieved (data);
			} else {
				lostMsgs.Add (data);
			}
		}

		private void LostMsgProcess (byte[] data) {
			if (controller != null) {
				controller.MessageRecieved (data);
			}
		}

		public void DoLostMessages () {
			if (controller != null && lostMsgs.Count > 0) {
				lostMsgs.ForEach(LostMsgProcess);
				lostMsgs.Clear ();
			}
		}
	}
	#else
	public class MenuRTMultiplayerListener : MonoBehaviour
	{
	}
	#endif
}

