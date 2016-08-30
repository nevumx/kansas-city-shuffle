using UnityEngine;
using System;
using Nx;

public class LocalRotationTween : Tween
{
	public	Vector3	EulerFrom	= Vector3.zero;
	public	Vector3	EulerTo		= Vector3.zero;
	public	bool	PingPong	= false;

	public LocalRotationTween() {}

	public LocalRotationTween(Vector3 from, Vector3 to, bool pingPong)
	{
		EulerFrom = from;
		EulerTo = to;
		PingPong = pingPong;
	}

	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		float percentDone = PingPong ? TweenHolder.EaseInOutPingPongAnimationCurveFastOutro(TweenHolder.PercentDone)
									 : TweenHolder.EaseInOutAnimationCurve(TweenHolder.PercentDone);
		TweenHolder.transform.localEulerAngles = Vector3.Lerp(EulerFrom, EulerTo, percentDone);
	}
}

public static class LocalRotationTweenHelperFunctions
{
	public static TweenHolder AddLocalRotationTween(this TweenHolder tweenHolder, Vector3 to, bool pingPong = false)
	{
		return AddLocalRotationTweenInternal(tweenHolder, tweenHolder.transform.localRotation.eulerAngles, to, pingPong);
	}

	public static TweenHolder AddLocalRotationTween(this TweenHolder tweenHolder, Vector3 from, Vector3 to, bool pingPong = false)
	{
		return AddLocalRotationTweenInternal(tweenHolder, from, to, pingPong);
	}

	public static TweenHolder AddLocalRotationTween(this ITweenable tweenable, Vector3 to, bool pingPong = false)
	{
		return AddLocalRotationTweenInternal(tweenable.TweenHolder, tweenable.gameObject.transform.localRotation.eulerAngles, to, pingPong);
	}

	public static TweenHolder AddLocalRotationTween(this ITweenable tweenable, Vector3 from, Vector3 to, bool pingPong = false)
	{
		return AddLocalRotationTweenInternal(tweenable.TweenHolder, from, to, pingPong);
	}

	private static TweenHolder AddLocalRotationTweenInternal(TweenHolder tweenHolder, Vector3 from, Vector3 to, bool pingPong)
	{
		return tweenHolder.AddTween(new LocalRotationTween(from, to, pingPong)).Play();
	}
}
