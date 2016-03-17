using UnityEngine;
using System;
using Nx;

public class OffsetHeightTween : Tween
{
	public	float						Height					= 0.0f;
	public	Func<float, float, float>	HeightFunction			= QuadraticHeightFunction;
	private	Vector3						_previousHeightOffset	= Vector3.zero;

	public OffsetHeightTween() {}

	public OffsetHeightTween(float height, Func<float, float, float> heightFunction)
	{
		Height = height;
		HeightFunction = heightFunction;
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

	public static float QuadraticHeightFunction(float percentDone, float height)
	{
		return -4.0f * height * percentDone * (percentDone - 1.0f);
	}

	public static float CircularHeightFunction(float percentDone, float height)
	{
		return 2.0f * height * Mathf.Sqrt(0.25f - Mathf.Pow(percentDone - 0.5f, 2));
	}
}

public static class OffsetHeightTweenHelperFunctions
{
	public static TweenHolder AddOffsetHeightTween(this TweenHolder tweenHolder, float height)
	{
		return AddOffsetHeightTweenInternal(tweenHolder, height, OffsetHeightTween.QuadraticHeightFunction);
	}

	public static TweenHolder AddOffsetHeightTween(this TweenHolder tweenHolder, float height, Func<float, float, float> heightFunction)
	{
		return AddOffsetHeightTweenInternal(tweenHolder, height, heightFunction);
	}

	public static TweenHolder AddOffsetHeightTween(this ITweenable tweenable, float height)
	{
				return AddOffsetHeightTweenInternal(tweenable.TweenHolder, height, OffsetHeightTween.QuadraticHeightFunction);
	}

		public static TweenHolder AddOffsetHeightTween(this ITweenable tweenable, float height, Func<float, float, float> heightFunction)
	{
				return AddOffsetHeightTweenInternal(tweenable.TweenHolder, height, heightFunction);
	}

	private static TweenHolder AddOffsetHeightTweenInternal(TweenHolder tweenHolder, float height, Func<float, float, float> heightFunction)
	{
		return tweenHolder.AddTween(new OffsetHeightTween(height, heightFunction)).Play();
	}
}