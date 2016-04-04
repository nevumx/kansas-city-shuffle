using UnityEngine;
using System.Collections.Generic;
using Nx;

public class FPSCounter : MonoBehaviour
{
	private	Queue<float>	_deltaTimes	= new Queue<float>();

#if NX_DEBUG
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
#endif

	private void Update()
	{
		_deltaTimes.Enqueue(Time.deltaTime);
		if (_deltaTimes.Count > 60)
		{
			_deltaTimes.Dequeue();
		}
	}

	private void OnGUI()
	{
		float sumDeltaTimes = 0.0f;
		_deltaTimes.ForEach(d => sumDeltaTimes += d);
		float averageDeltaTime = sumDeltaTimes / _deltaTimes.Count;

		GUI.skin.label.fontSize = 25;
		if (1.0f / averageDeltaTime < 15.0f)
		{
			GUI.color = Color.red;
		}
		else if (1.0f / averageDeltaTime < 30.0f)
		{
			GUI.color = Color.yellow;
		}
		else
		{
			GUI.color = Color.green;
		}
		GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "FPS: " + Mathf.RoundToInt(1.0f / averageDeltaTime) + " = " + Mathf.RoundToInt(averageDeltaTime * 1000.0f) + "ms");
	}
}
