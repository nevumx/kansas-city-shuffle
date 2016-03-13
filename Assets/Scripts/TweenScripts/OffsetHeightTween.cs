using UnityEngine;
using System;
using Nx;

public class OffsetHeightTween : Tween
{
	public	float	Height					= 0.0f;
	private	Vector3	_previousHeightOffset	= Vector3.zero;

	public OffsetHeightTween() {}

	public OffsetHeightTween(float height)
	{
		Height = height;
	}

	public override Action<GameObject, float, float> GetUpdateDelegate() { return OnUpdate; }

	public override Action<GameObject> GetEndOfFrameDelegate() { return OnEndOfFrame; }

	public override int GetExecutionOrder() { return 1; }

	private void OnUpdate(GameObject gameObj, float percentDone, float timeRemaining)
	{
		_previousHeightOffset = HeightFunction(percentDone, Height) * Vector3.up;
		gameObj.transform.position += _previousHeightOffset;
	}

	private void OnEndOfFrame(GameObject gameObj)
	{
		gameObj.transform.position -= _previousHeightOffset;
	}

	private static float HeightFunction(float percentDone, float height)
	{
		return -4.0f * height * percentDone * (percentDone - 1.0f);
	}
}

public static class OffsetHeightTweenHelperFunctions
{
	public static TweenHolder AddOffsetHeightTween(this TweenHolder tweenHolder, float height)
	{
		return AddOffsetHeightTweenInternal(tweenHolder, height);
	}

	public static TweenHolder AddOffsetHeightTween(this ITweenable tweenable, float height)
	{
		return AddOffsetHeightTweenInternal(tweenable.TweenHolder, height);
	}

	private static TweenHolder AddOffsetHeightTweenInternal(TweenHolder tweenHolder, float height)
	{
		return tweenHolder.AddTween(new OffsetHeightTween(height)).Play();
	}
}