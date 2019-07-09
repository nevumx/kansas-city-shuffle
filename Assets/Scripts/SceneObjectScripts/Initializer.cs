using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body

public class Initializer : MonoBehaviour
{
	private void Awake()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		DisableNavUI();
		DontDestroyOnLoad(gameObject);
#elif UNITY_IOS
		Application.targetFrameRate = 60;
#endif
		SceneManager.LoadScene("TitleScreen");
	}

#if UNITY_ANDROID && !UNITY_EDITOR
	private	const	int					SYSTEM_UI_FLAG_HIDE_NAVIGATION			= 2;
	private	const	int					SYSTEM_UI_FLAG_FULLSCREEN				= 4;
	private	const	int					SYSTEM_UI_FLAG_LAYOUT_STABLE			= 256;
	private	const	int					SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION	= 512;
	private	const	int					SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN		= 1024;
	private	const	int					SYSTEM_UI_FLAG_IMMERSIVE				= 2048;
	private	const	int					SYSTEM_UI_FLAG_IMMERSIVE_STICKY			= 4096;

	private	static	AndroidJavaObject	viewInstance;

	private static void Run()
	{
		if (viewInstance != null)
		{
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
		try
		{
			using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				AndroidJavaObject activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
				AndroidJavaObject windowInstance = activityInstance.Call<AndroidJavaObject>("getWindow");
				viewInstance = windowInstance.Call<AndroidJavaObject>("getDecorView");

				AndroidJavaRunnable runner = new AndroidJavaRunnable(Run);
				activityInstance.Call("runOnUiThread", runner);
			}
		}
		catch {}
	}

	private void OnApplicationPause(bool paused)
	{
		DisableNavUI();
	}
#endif
}

#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body