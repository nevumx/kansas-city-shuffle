using UnityEngine;
using System;
using Nx;

public class QuaternionRotationTween : CachedTransformTween
{
	public	Quaternion	From	= Quaternion.identity;
	public	Quaternion	To		= Quaternion.identity;

	public QuaternionRotationTween() {}

	public QuaternionRotationTween(Quaternion from, Quaternion to)
	{
		From = from;
		To = to;
	}

	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		float percentDone = TweenHolder.EaseInOutAnimationCurve(TweenHolder.PercentDone);
		_CachedTransform.rotation = Quaternion.Slerp(From, To, percentDone);
	}
}

public static class QuaternionRotationTweenHelperFunctions
{
	public static TweenHolder AddQuaternionRotationTween(this TweenHolder tweenHolder, Quaternion to)
	{
		return AddQuaternionRotationTweenInternal(tweenHolder, tweenHolder.transform.rotation, to);
	}

	public static TweenHolder AddQuaternionRotationTween(this TweenHolder tweenHolder, Quaternion from, Quaternion to)
	{
		return AddQuaternionRotationTweenInternal(tweenHolder, from, to);
	}

	public static TweenHolder AddQuaternionRotationTween(this ITweenable tweenable, Quaternion to)
	{
		return AddQuaternionRotationTweenInternal(tweenable.TweenHolder, tweenable.gameObject.transform.rotation, to);
	}

	public static TweenHolder AddQuaternionRotationTween(this ITweenable tweenable, Quaternion from, Quaternion to)
	{
		return AddQuaternionRotationTweenInternal(tweenable.TweenHolder, from, to);
	}

	private static TweenHolder AddQuaternionRotationTweenInternal(TweenHolder tweenHolder, Quaternion from, Quaternion to)
	{
		return tweenHolder.AddTween(new QuaternionRotationTween(from, to)).Play();
	}
}
