using UnityEngine;

public class Data {

	// The default sensitivity
	public static float DefaultSensitivity {
		get {
			return 5;
		}
	}

	// The default relative aiming sensitivity
	public static float DefaultAimingSensitivity {
		get {
			#if UNITY_ANDROID
			return 0.01f;
			#else
			return 0.8f;
			#endif
		}
	}

	public static int DefaultQuality {
		get {
			#if UNITY_ANDROID
			return 1;
			#else
			return 3;
			#endif
		}
	}

	public static float DefaultSound {
		get {
			return 1;
		}
	}

	// The sensitivity
	public static float sensitivity = DefaultSensitivity;

	// The relative aiming sensitivity
	public static float aimingSensitivity = DefaultAimingSensitivity;

	// The sensitivity to use
	public static float GetSensitivity(bool aiming) {
		return sensitivity * (aiming ? aimingSensitivity : 1);
	}

	public static int quality = DefaultQuality;

	public static float sound = DefaultSound;

	public static bool hasPlayed = false;
}
