using UnityEngine;
using System;
using Nx;

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

	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		float percentDone = TweenHolder.EaseInOutAnimationCurve(TweenHolder.PercentDone);
		_CachedTransform.localRotation = Quaternion.Slerp(From, To, percentDone);
	}
}

public static class LocalQuaternionRotationTweenHelperFunctions
{
	public static TweenHolder AddLocalQuaternionRotationTween(this TweenHolder tweenHolder, Quaternion to)
	{
		return AddLocalQuaternionRotationTweenInternal(tweenHolder, tweenHolder.transform.localRotation, to);
	}

	public static TweenHolder AddLocalQuaternionRotationTween(this TweenHolder tweenHolder, Quaternion from, Quaternion to)
	{
		return AddLocalQuaternionRotationTweenInternal(tweenHolder, from, to);
	}

	public static TweenHolder AddLocalQuaternionRotationTween(this ITweenable tweenable, Quaternion to)
	{
		return AddLocalQuaternionRotationTweenInternal(tweenable.TweenHolder, tweenable.gameObject.transform.localRotation, to);
	}

	public static TweenHolder AddLocalQuaternionRotationTween(this ITweenable tweenable, Quaternion from, Quaternion to)
	{
		return AddLocalQuaternionRotationTweenInternal(tweenable.TweenHolder, from, to);
	}

	private static TweenHolder AddLocalQuaternionRotationTweenInternal(TweenHolder tweenHolder, Quaternion from, Quaternion to)
	{
		return tweenHolder.AddTween(new LocalQuaternionRotationTween(from, to)).Play();
	}
}
