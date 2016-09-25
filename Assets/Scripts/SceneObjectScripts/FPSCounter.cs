using UnityEngine;
using System.Collections.Generic;
using Nx;

public class FPSCounter : MonoBehaviour
{
	private	const	int				DELTA_TIMES_BUFFER_SIZE				= 60;


	private			Queue<float>	_deltaTimes							= new Queue<float>();
	private			float			_cachedAverageDeltaTime				= 1.0f;
	private			int				_numDeltaTimesToIgnore				= 3;

#if NX_DEBUG
	private			bool			_drawFPS							= false;
#endif

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	private void Update()
	{
#if NX_DEBUG
#if UNITY_ANDROID && !UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Menu))
#elif UNITY_IOS && !UNITY_EDITOR
		if (Input.touchCount >= 4)
#else
		if (Input.GetKeyDown(KeyCode.Escape))
#endif
		{
			_drawFPS = !_drawFPS;
		}
#endif
		if (_numDeltaTimesToIgnore > 0)
		{
			--_numDeltaTimesToIgnore;
			return;
		}

		_deltaTimes.Enqueue(Time.unscaledDeltaTime);

		if (_deltaTimes.Count > DELTA_TIMES_BUFFER_SIZE)
		{
			_deltaTimes.Dequeue();
		}

		float sumDeltaTimes = 0.0f;
		_deltaTimes.ForEach(d => sumDeltaTimes += d);
		_cachedAverageDeltaTime =  sumDeltaTimes / _deltaTimes.Count;
	}

#if NX_DEBUG
	private void OnGUI()
	{
		if (!_drawFPS)
		{
			return;
		}

		GUI.skin.label.fontSize = 25;
		if (1.0f / _cachedAverageDeltaTime < 15.0f)
		{
			GUI.color = Color.red;
		}
		else if (1.0f / _cachedAverageDeltaTime < 30.0f)
		{
			GUI.color = Color.yellow;
		}
		else
		{
			GUI.color = Color.green;
		}
		GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "FPS: " + Mathf.RoundToInt(1.0f / _cachedAverageDeltaTime)
						   + " = " + Mathf.RoundToInt(_cachedAverageDeltaTime * 1000.0f) + "ms");
	}
#endif

	public bool IsUnderFramerate(float framerate)
	{
		if (framerate <= 0.0f)
		{
			return true;
		}
		return _cachedAverageDeltaTime >= 1.0f / framerate;
	}

	public void ClearHistory()
	{
		_deltaTimes.Clear();
		_numDeltaTimesToIgnore = 2;
		_cachedAverageDeltaTime = 1.0f;
	}
}
