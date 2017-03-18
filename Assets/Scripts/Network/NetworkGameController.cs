using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkGameController : NetworkBehaviour {
	public NetworkManager networkManager;

	// The minimum distance allowed between players
	public float minDistance;

	// The maximum distance allowed between players
	public float maxDistance;

	[SyncVar]
	private int turn = 0;

	[SyncVar]
	private int playerCount;

	private GameObject[] players;
	private new Transform camera;

	private bool initialized = false;

	// Use this for initialization
	void Start () {
		GameObject origin = new GameObject ("Origin");
		origin.transform.position = new Vector3 (0, 1, 0);
		networkManager.startPositions.Add (origin.transform);

		Vector3 pos = Random.onUnitSphere;
		pos.y = 0;
		pos.Normalize ();

		pos *= Random.Range (minDistance, maxDistance);
		pos.y = 1;

		GameObject second = new GameObject ("Other");
		second.transform.position = pos;
		second.transform.Rotate (Vector3.up, Random.Range (0, 360));

		networkManager.startPositions.Add (second.transform);

		Debug.Log (string.Format ("Distance is {0}", Vector3.Distance (origin.transform.position, second.transform.position)));
	}

	[ClientRpc]
	public void RpcSetCamera (GameObject player) {
		if (player == null) {
			Debug.LogWarning ("RpcSetCamera(): player is null");
			return;
		}
		Transform playerGuide = player.transform.FindChild ("Camera Guide (Normal)");
		camera = GameObject.FindWithTag ("MainCamera").transform;

		camera.position = playerGuide.position;
		camera.rotation = playerGuide.rotation;
		camera.SetParent (playerGuide);
	}

	// Update is called once per frame
	void Update () {
		if (networkManager.numPlayers < 2) {
			return;
		} else if (!initialized) {
			players = GameObject.FindGameObjectsWithTag ("Player");

			RpcSetCamera (GetCurrentPlayer ());

			initialized = true;
			return;
		}
	}

	void LateUpdate () {
		if (playerCount != networkManager.numPlayers) {
			playerCount = networkManager.numPlayers;
		}
	}

	public int GetTurn () {
		return turn;
	}

	public void NextTurn () {
		turn = (turn + 1) % 2;

		players = GameObject.FindGameObjectsWithTag ("Player");
		RpcSetCamera (GetCurrentPlayer ());
		/*Transform playerGuide = players [turn].transform.FindChild ("Camera Guide (Normal)");
		camera = GameObject.FindWithTag ("MainCamera").transform;

		camera.position = playerGuide.position;
		camera.rotation = playerGuide.rotation;
		camera.SetParent (playerGuide);*/
	}

	public GameObject GetCurrentPlayer () {
		if (players != null && players.Length > 0) {
			return players [turn];
		} else {
			return null;
		}
	}

	public GameObject GetNextPlayer () {
		if (players != null && players.Length > 0) {
			return players [(turn + 1) % players.Length];
		} else {
			return null;
		}
	}

	public int GetPlayerCount () {
		return playerCount;
	}
}
