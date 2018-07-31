using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger {
	private static string TAG = "[Crossbows]";

	private static string MakeLogString (string level, string msg) {
		return string.Format ("{0} {1}: {2}", TAG, level, msg);
	}

	public static void LogInfo (string msg) {
		Debug.Log (MakeLogString ("INFO", msg));
	}

	public static void LogError (string msg) {
		Debug.LogError (MakeLogString ("ERROR", msg));
	}

	public static void LogWarning (string msg) {
		Debug.LogWarning (MakeLogString ("WARN", msg));
	}
}
