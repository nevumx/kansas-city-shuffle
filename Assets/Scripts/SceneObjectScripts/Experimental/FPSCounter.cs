using UnityEngine;
using System.Collections.Generic;
using Nx;

public class FPSCounter : MonoBehaviour
{
	private	const	int				DELTA_TIMES_BUFFER_SIZE		= 60;

	private			Queue<float>	_deltaTimes					= new Queue<float>();
	private			float			_cachedAverageDeltaTime		= 0.01f;
	public			float			CachedAverageDeltaTime		{ get { return _cachedAverageDeltaTime; } }

	private void Awake()
	{
#if NX_DEBUG
		DontDestroyOnLoad(gameObject);
#endif
		FPSCounter[] allCounters = FindObjectsOfType<FPSCounter>();
		allCounters.ForEach(c =>
		{
			if (c !=  this)
			{
				Destroy(c.gameObject);
			}
		});
	}

	private void Update()
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

#if NX_DEBUG
	private void OnGUI()
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
#endif
}
