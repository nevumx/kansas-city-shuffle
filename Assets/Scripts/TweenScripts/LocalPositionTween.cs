using UnityEngine;
using System;
using Nx;

public class LocalPositionTween : Tween
{
	public	Vector3	LocalPositionFrom	= Vector3.zero;
	public	Vector3	LocalPositionTo		= Vector3.zero;

	public LocalPositionTween() {}

	public LocalPositionTween(Vector3 from, Vector3 to)
	{
		LocalPositionFrom = from;
		LocalPositionTo = to;
	}

	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		float percentDone = TweenHolder.EaseInOutAnimationCurve(TweenHolder.PercentDone);
		TweenHolder.transform.localPosition = Vector3.Lerp(LocalPositionFrom, LocalPositionTo, percentDone);
	}
}

public static class LocalPositionTweenHelperFunctions
{
	public static TweenHolder AddLocalPositionTween(this TweenHolder tweenHolder, Vector3 to)
	{
		return AddLocalPositionTweenInternal(tweenHolder, tweenHolder.transform.localPosition, to);
	}

	public static TweenHolder AddLocalPositionTween(this TweenHolder tweenHolder, Vector3 from, Vector3 to)
	{
		return AddLocalPositionTweenInternal(tweenHolder, from, to);
	}

	public static TweenHolder AddLocalPositionTween(this ITweenable tweenable, Vector3 to)
	{
		return AddLocalPositionTweenInternal(tweenable.TweenHolder, tweenable.gameObject.transform.localPosition, to);
	}

	public static TweenHolder AddLocalPositionTween(this ITweenable tweenable, Vector3 from, Vector3 to)
	{
		return AddLocalPositionTweenInternal(tweenable.TweenHolder, from, to);
	}

	private static TweenHolder AddLocalPositionTweenInternal(TweenHolder tweenHolder, Vector3 from, Vector3 to)
	{
		return tweenHolder.AddTween(new LocalPositionTween(from, to)).Play();
	}
}