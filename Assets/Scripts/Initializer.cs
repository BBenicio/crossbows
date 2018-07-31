using System;
using UnityEngine;

#if UNITY_ANDROID
using GoogleMobileAds.Api;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class Initializer
{
	private static bool loaded = false;

	#if UNITY_ANDROID
	public static bool adsInitialized { get; private set; }
	public static bool playGamesInitialized { get; private set; }
	public static bool nearbyInitialized { get; private set; }
	#endif

	public static void Load () {
		if (!loaded) {
			//Data.sensitivity = PlayerPrefs.GetFloat ("sensitivity", Data.sensitivity);
			//Data.aimingSensitivity = PlayerPrefs.GetFloat ("aimingSensitivity", Data.aimingSensitivity);
			Data.tutorial = PlayerPrefs.GetInt ("tutorial", 1) == 1 && Data.tutorial;
			Data.sound = PlayerPrefs.GetFloat ("sound", Data.sound);
			Data.quality = PlayerPrefs.GetInt ("quality", Data.quality);

			#if UNITY_ANDROID
			Data.signedIn = PlayerPrefs.GetInt ("signedIn", 1) == 1;
			#endif

			QualitySettings.SetQualityLevel (Data.quality);

			loaded = true;
		}
	}

	#if UNITY_ANDROID
	public static void InitAndroid () {
		if (!adsInitialized) {
			Logger.LogInfo ("Initializing ads");
			MobileAds.Initialize("ca-app-pub-2833633163238735~6400467607");
			adsInitialized = true;
		}

		if (!playGamesInitialized) {
			Logger.LogInfo ("Initializing Play Games");
			PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
			PlayGamesPlatform.InitializeInstance(config);
			PlayGamesPlatform.Activate();

			playGamesInitialized = true;
		}

		if (!nearbyInitialized) {
			Logger.LogInfo ("Initializing Nearby");
			PlayGamesPlatform.InitializeNearby ((client) => {
				Logger.LogInfo ("Initialized Nearby");
				nearbyInitialized = true;
			});
		}
	}
	#endif
}

