using UnityEngine;
using System;

public class LocalPositionTween : CachedTransformTween
{
	public	Vector3	LocalPositionFrom	= Vector3.zero;
	public	Vector3	LocalPositionTo		= Vector3.zero;

	public LocalPositionTween() {}

	public LocalPositionTween(Vector3 from, Vector3 to)
	{
		LocalPositionFrom = from;
		LocalPositionTo = to;
	}

	public override void OnUpdate()
	{
		float percentDone = TweenHolder.EaseInOutAnimationCurve(TweenHolder.PercentDone);
		_CachedTransform.localPosition = Vector3.Lerp(LocalPositionFrom, LocalPositionTo, percentDone);
	}
}

public static class LocalPositionTweenHelperFunctions
{
	public static TweenHolder AddLocalPositionTween(this ITweenable tweenable, Vector3 to)
	{
		return AddLocalPositionTweenInternal(tweenable.Holder, tweenable.gameObject.transform.localPosition, to);
	}

	public static TweenHolder AddLocalPositionTween(this ITweenable tweenable, Vector3 from, Vector3 to)
	{
		return AddLocalPositionTweenInternal(tweenable.Holder, from, to);
	}

	private static TweenHolder AddLocalPositionTweenInternal(TweenHolder tweenHolder, Vector3 from, Vector3 to)
	{
		return tweenHolder.AddTween(new LocalPositionTween(from, to)).Play();
	}
}