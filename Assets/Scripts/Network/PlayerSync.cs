using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSync : NetworkBehaviour {

	public struct InputUpdate {
		public bool aim;
		public float horizontalAxis;
		public float verticalAxis;

		public GameObject player;

		public InputUpdate (bool aim, float horizontalAxis, float verticalAxis, GameObject player) {
			this.aim = aim;
			this.horizontalAxis = horizontalAxis;
			this.verticalAxis = verticalAxis;
			this.player = player;
		}
	}

	public int ticksBetweenUpdates = 6;

	private InputUpdate input;
	private int updates = 0;

	public void SendInput (InputUpdate iu) {
		if (++updates >= ticksBetweenUpdates) {
			CmdSend (iu);
		}
	}
	
	void Update () {
		if (input.player == null || input.player == gameObject) {
			return;
		}

		input.player.GetComponent<NetworkPlayerBehaviour> ()
			.InputUpdate (input.horizontalAxis, input.verticalAxis, input.aim, false);
	}

	[ClientRpc]
	public void RpcInput (InputUpdate iu) {
		input = new InputUpdate ();

		if (iu.player != gameObject) {
			input = iu;
		}
	}

	[Command]
	public void CmdSend (InputUpdate iu) {
		RpcInput (iu);
	}
}
