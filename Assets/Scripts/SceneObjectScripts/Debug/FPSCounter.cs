using UnityEngine;
using System.Collections.Generic;
using Nx;

public class FPSCounter : MonoBehaviour
{
	private	const	int				DELTA_TIMES_BUFFER_SIZE				= 60;

	private			bool			_drawFPS							= false;
	private			Queue<float>	_deltaTimes							= new Queue<float>();
	private			float			_totalHistoricalDeltaTimesSum		= 0.0f;
	private			int				_totalNumHistoricalDeltaTimes		= 0;
	private			float			_cachedAverageDeltaTime				= 0.01f;
	private			float			_cachedAverageHistoricalDeltaTime	= 0.01f;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	private void Update()
	{
#if NX_DEBUG
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			_drawFPS = !_drawFPS;
		}
#endif

		_deltaTimes.Enqueue(Time.deltaTime);
		if (_deltaTimes.Count > DELTA_TIMES_BUFFER_SIZE)
		{
			_totalHistoricalDeltaTimesSum += _deltaTimes.Dequeue();
			++_totalNumHistoricalDeltaTimes;
		}

		float sumDeltaTimes = 0.0f;
		_deltaTimes.ForEach(d => sumDeltaTimes += d);
		_cachedAverageDeltaTime =  sumDeltaTimes / _deltaTimes.Count;
		_cachedAverageHistoricalDeltaTime = (sumDeltaTimes + _totalHistoricalDeltaTimesSum) / (_deltaTimes.Count + _totalNumHistoricalDeltaTimes);
	}

	private void OnGUI()
	{
		if (_drawFPS)
		{
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
							   + " = " + Mathf.RoundToInt(_cachedAverageDeltaTime * 1000.0f) + "ms\n"
							   + "Cumulative Average FPS: " + Mathf.RoundToInt(1.0f / _cachedAverageHistoricalDeltaTime)
							   + " = " + Mathf.RoundToInt(_cachedAverageHistoricalDeltaTime * 1000.0f) + "ms");
		}
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
		_totalHistoricalDeltaTimesSum = 0.0f;
		_totalNumHistoricalDeltaTimes = 0;
		_cachedAverageDeltaTime = 0.01f;
		_cachedAverageHistoricalDeltaTime = 0.01f;
	}
}
