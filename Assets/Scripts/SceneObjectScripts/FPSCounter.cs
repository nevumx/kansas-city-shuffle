using UnityEngine;
using System;
using System.Collections.Generic;
using Nx;

public class FPSCounter : MonoBehaviour
{
	private	const		int				DELTA_TIMES_BUFFER_SIZE	= 180;

	private	readonly	Queue<float>	_deltaTimes				= new Queue<float>();
	private				float			_cachedMinimumDeltaTime	= 1.0f;
	private				float			_cachedAverageDeltaTime	= 1.0f;
	private				float			_cachedMaximumDeltaTime	= 1.0f;
	private				int				_numDeltaTimesToIgnore	= 3;
#if UNITY_IOS && !UNITY_EDITOR
	private				int				_previousTouchCount;
#endif

	private				bool			_drawFPS;

	private void Awake()
	{
		if (Debug.isDebugBuild)
		{
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		if (!Debug.isDebugBuild)
		{
			return;
		}

#if UNITY_ANDROID && !UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Menu))
#elif UNITY_IOS && !UNITY_EDITOR
		if (Input.touchCount != _previousTouchCount && Input.touchCount == 4)
#else
		if (Input.GetKeyDown(KeyCode.Escape))
#endif
		{
			_drawFPS = !_drawFPS;
		}
#if UNITY_IOS && !UNITY_EDITOR
		_previousTouchCount = Input.touchCount;
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
		_cachedMinimumDeltaTime = float.MaxValue;
		float sumDeltaTimes = 0.0f;
		_cachedMaximumDeltaTime = float.MinValue;
		_deltaTimes.ForEach(d =>
		{
			_cachedMinimumDeltaTime = Mathf.Min(_cachedMinimumDeltaTime, d);
			sumDeltaTimes += d;
			_cachedMaximumDeltaTime = Mathf.Max(_cachedMaximumDeltaTime, d);
		});
		_cachedAverageDeltaTime = sumDeltaTimes / _deltaTimes.Count;
	}

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
		GUI.Label(new Rect(0, 0, Screen.width, Screen.height),
			"FPS: " + Mathf.RoundToInt(1.0f / _cachedAverageDeltaTime)
				+ " = " + Mathf.RoundToInt(_cachedAverageDeltaTime * 1000.0f) + "ms" + Environment.NewLine
			+ "Max FPS: " + Mathf.RoundToInt(1.0f / _cachedMinimumDeltaTime)
				+ " = " + Mathf.RoundToInt(_cachedMinimumDeltaTime * 1000.0f) + "ms" + Environment.NewLine
			+ "Min FPS: " + Mathf.RoundToInt(1.0f / _cachedMaximumDeltaTime)
				+ " = " + Mathf.RoundToInt(_cachedMaximumDeltaTime * 1000.0f) + "ms");
	}

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
		_cachedMinimumDeltaTime = 1.0f;
		_cachedAverageDeltaTime = 1.0f;
		_cachedMaximumDeltaTime = 1.0f;
	}
}