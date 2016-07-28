using UnityEngine;
using System.Collections.Generic;
using Nx;

public class FPSCounter : MonoBehaviour
{
	private	const	int				DELTA_TIMES_BUFFER_SIZE		= 60;

	private			bool			_drawFPS					= false;
	private			Queue<float>	_deltaTimes					= new Queue<float>();
	private			float			_cachedAverageDeltaTime		= 0.01f;
	public			float			CachedAverageDeltaTime		{ get { return _cachedAverageDeltaTime; } }

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

		if (_drawFPS)
		{
			_deltaTimes.Enqueue(Time.deltaTime);
			if (_deltaTimes.Count > DELTA_TIMES_BUFFER_SIZE)
			{
				_deltaTimes.Dequeue();
			}

			float sumDeltaTimes = 0.0f;
			_deltaTimes.ForEach(d => sumDeltaTimes += d);
			_cachedAverageDeltaTime =  sumDeltaTimes / _deltaTimes.Count;
		}
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
			GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "FPS: " + Mathf.RoundToInt(1.0f / _cachedAverageDeltaTime) + " = " + Mathf.RoundToInt(_cachedAverageDeltaTime * 1000.0f) + "ms");
		}
	}
}
