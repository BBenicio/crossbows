using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayerBehaviour : NetworkBehaviour {
	public Material blue;
	public Material red;

	public Transform head;
	public Transform normalCameraGuide;

	// Maximum vertical rotation
	public float maximumX = 80;

	// Minimum vertical rotation
	public float minimumX = -60;

	public float defaultFov = 60;
	public float aimingFov = 40;

	public NetworkManager networkManager;

	public PlayerSync sync;

	private NetworkGameController gameController;
	private new Transform camera;
	private Camera cameraComponent;

	private int id;

	public override void OnStartClient ()
	{
		base.OnStartClient ();

		SetMaterials ();
		gameController = GameObject.FindWithTag ("GameController").GetComponent<NetworkGameController> ();

		camera = GameObject.FindWithTag ("MainCamera").transform;
		cameraComponent = camera.GetComponent<Camera> ();
	}

	private void SetMaterials () {
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");

		for (int i = 0; i < players.Length; ++i) {
			players [i].GetComponentInChildren<SkinnedMeshRenderer> ().material = i == 0 ? blue : red;

			if (players [i] == gameObject) {
				id = i;
			}
		}
	}

	[ClientRpc]
	public void RpcInput (GameObject player, float h, float v, float fov) {
		if (player != null) {
			player.transform.Rotate (player.transform.up, h);

			if (v != 0) {
				NetworkPlayerBehaviour npb = player.GetComponent<NetworkPlayerBehaviour> ();

				npb.normalCameraGuide.RotateAround (npb.head.position, npb.normalCameraGuide.right, -v);
			}
		}

		cameraComponent.fieldOfView = fov;
	}

	/*[Command]
	public void CmdInput (float h, float v, bool shoot, bool aim) {
		transform.Rotate (Vector3.up, h);

		Quaternion cameraQ = normalCameraGuide.localRotation * Quaternion.Euler (-v, 0, 0);
		if (cameraQ.eulerAngles.x >= 360 + minimumX || cameraQ.eulerAngles.x <= maximumX) {
			normalCameraGuide.RotateAround (head.position, normalCameraGuide.right, -v);
		}

		float fov = cameraComponent.fieldOfView;

		if (aim) {
			fov = fov != defaultFov ? defaultFov : aimingFov;
		}

		if (shoot) {
			gameController.NextTurn ();
			if (cameraComponent.fieldOfView != defaultFov) {
				fov = defaultFov;
			}
		}

		//RpcInput (h == 0 && v == 0 ? null : gameObject, h, v, fov);

		//if (fov != cameraComponent.fieldOfView) {
		//	RpcUpdateFov (fov);
		//}
	}*/

	public void InputUpdate (float h, float v, bool aim, bool shoot) {
		transform.Rotate (transform.up, h);

		Quaternion cameraQ = normalCameraGuide.localRotation * Quaternion.Euler (-v, 0, 0);
		if (cameraQ.eulerAngles.x >= 360 + minimumX || cameraQ.eulerAngles.x <= maximumX) {
			normalCameraGuide.RotateAround (head.position, normalCameraGuide.right, -v);
		}

		float fov = cameraComponent.fieldOfView;

		if (aim) {
			fov = fov != defaultFov ? defaultFov : aimingFov;
		}

		cameraComponent.fieldOfView = fov;

		if (shoot) {
			gameController.NextTurn ();
			if (cameraComponent.fieldOfView != defaultFov) {
				fov = defaultFov;
			}
		}
	}

	void Update () {
		if (!isLocalPlayer || gameController.GetTurn () != id || gameController.GetPlayerCount() < 2) {
			return;
		}

		float horizontal = 5 * Input.GetAxis ("Horizontal");
		float vertical = 5 * Input.GetAxis ("Vertical");
		bool shoot = Input.GetButtonDown ("Shoot");
		bool aim = Input.GetButtonDown ("Aim");

		//CmdInput (horizontal, vertical, shoot, aim);
		InputUpdate (horizontal, vertical, aim, shoot);
		sync.SendInput (new PlayerSync.InputUpdate (aim, horizontal, vertical, gameObject));
	}
}
