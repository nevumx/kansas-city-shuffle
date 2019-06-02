using UnityEngine;
using System;

public class LocalQuaternionRotationTween : CachedTransformTween
{
	public	Quaternion	From	= Quaternion.identity;
	public	Quaternion	To		= Quaternion.identity;

	public LocalQuaternionRotationTween() {}

	public LocalQuaternionRotationTween(Quaternion from, Quaternion to)
	{
		From = from;
		To = to;
	}

	public override void OnUpdate()
	{
		float percentDone = TweenHolder.EaseInOutAnimationCurve(TweenHolder.PercentDone);
		_CachedTransform.localRotation = Quaternion.Slerp(From, To, percentDone);
	}
}

public static class LocalQuaternionRotationTweenHelperFunctions
{
	public static TweenHolder AddLocalQuaternionRotationTween(this ITweenable tweenable, Quaternion to)
	{
		return AddLocalQuaternionRotationTweenInternal(tweenable.Holder, tweenable.gameObject.transform.localRotation, to);
	}

	public static TweenHolder AddLocalQuaternionRotationTween(this ITweenable tweenable, Quaternion from, Quaternion to)
	{
		return AddLocalQuaternionRotationTweenInternal(tweenable.Holder, from, to);
	}

	private static TweenHolder AddLocalQuaternionRotationTweenInternal(TweenHolder tweenHolder, Quaternion from, Quaternion to)
	{
		return tweenHolder.AddTween(new LocalQuaternionRotationTween(from, to)).Play();
	}
}
