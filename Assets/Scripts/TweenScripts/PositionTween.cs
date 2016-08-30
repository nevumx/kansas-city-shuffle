using UnityEngine;
using System;
using Nx;

public class PositionTween : Tween
{
	public	Vector3	PositionFrom	= Vector3.zero;
	public	Vector3	PositionTo		= Vector3.zero;

	public PositionTween() {}

	public PositionTween(Vector3 from, Vector3 to)
	{
		PositionFrom = from;
		PositionTo = to;
	}

	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		float percentDone = TweenHolder.EaseInOutAnimationCurve(TweenHolder.PercentDone);
		TweenHolder.transform.position = Vector3.Lerp(PositionFrom, PositionTo, percentDone);
	}
}

public static class PositionTweenHelperFunctions
{
	public static TweenHolder AddPositionTween(this TweenHolder tweenHolder, Vector3 to)
	{
		return AddPositionTweenInternal(tweenHolder, tweenHolder.transform.position, to);
	}

	public static TweenHolder AddPositionTween(this TweenHolder tweenHolder, Vector3 from, Vector3 to)
	{
		return AddPositionTweenInternal(tweenHolder, from, to);
	}

	public static TweenHolder AddPositionTween(this ITweenable tweenable, Vector3 to)
	{
		return AddPositionTweenInternal(tweenable.TweenHolder, tweenable.gameObject.transform.position, to);
	}

	public static TweenHolder AddPositionTween(this ITweenable tweenable, Vector3 from, Vector3 to)
	{
		return AddPositionTweenInternal(tweenable.TweenHolder, from, to);
	}

	private static TweenHolder AddPositionTweenInternal(TweenHolder tweenHolder, Vector3 from, Vector3 to)
	{
		return tweenHolder.AddTween(new PositionTween(from, to)).Play();
	}
}