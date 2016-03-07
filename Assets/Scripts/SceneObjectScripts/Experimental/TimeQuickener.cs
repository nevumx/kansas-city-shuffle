using UnityEngine;
using System;
using System.Collections;
using Nx;

public class TimeQuickener : MonoBehaviour
{
	public void SpeedUp()
	{
		Time.timeScale *= 2.0f;
	}

	public void SlowDown()
	{
		Time.timeScale /= 2.0f;
	}

	private void OnDestroy()
	{
		Time.timeScale = 1.0f;
	}
}