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
			return 0.8f;
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
}
