using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class Initializer : MonoBehaviour
{
	private void Awake()
	{
#if UNITY_STANDALONE
		Resolution bestResolution = Screen.resolutions[Screen.resolutions.Length - 1];
		Screen.SetResolution(bestResolution.width, bestResolution.height, fullscreen: true);
#elif UNITY_ANDROID
		DisableNavUI();
		DontDestroyOnLoad(this);
#elif UNITY_IOS
		Application.targetFrameRate = 60;
#endif
		SceneManager.LoadScene("MainMenu");
	}
	
#if UNITY_ANDROID
	private	static	AndroidJavaObject	activityInstance;
	private	static	AndroidJavaObject	windowInstance;
	private	static	AndroidJavaObject	viewInstance;

	private	const	int					SYSTEM_UI_FLAG_HIDE_NAVIGATION			= 2;
	private	const	int					SYSTEM_UI_FLAG_LAYOUT_STABLE			= 256;
	private	const	int					SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION	= 512;
	private	const	int					SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN		= 1024;
	private	const	int					SYSTEM_UI_FLAG_IMMERSIVE				= 2048;
	private	const	int					SYSTEM_UI_FLAG_IMMERSIVE_STICKY			= 4096;
	private	const	int					SYSTEM_UI_FLAG_FULLSCREEN				= 4;

	private static void Run()
	{
		if (viewInstance != null) {
			viewInstance.Call("setSystemUiVisibility",
								SYSTEM_UI_FLAG_LAYOUT_STABLE
							  | SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
							  | SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
							  | SYSTEM_UI_FLAG_HIDE_NAVIGATION
							  | SYSTEM_UI_FLAG_FULLSCREEN
							  | SYSTEM_UI_FLAG_IMMERSIVE_STICKY);
		}
	}

	private static void DisableNavUI()
	{
#if NX_DEBUG
		try
		{
#endif
			using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
				windowInstance = activityInstance.Call<AndroidJavaObject>("getWindow");
				viewInstance = windowInstance.Call<AndroidJavaObject>("getDecorView");
				
				AndroidJavaRunnable runThis = new AndroidJavaRunnable(Run);
				activityInstance.Call("runOnUiThread", runThis);
			}
#if NX_DEBUG
		}
		catch {}
#endif
	}


	private void OnApplicationPause(bool paused)
	{
		DisableNavUI();
	}
#endif
}
