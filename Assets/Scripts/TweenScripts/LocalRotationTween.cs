using UnityEngine;
using System;
using Nx;

public class LocalRotationTween : Tween
{
	public	Vector3	EulerFrom	= Vector3.zero;
	public	Vector3	EulerTo		= Vector3.zero;

	public LocalRotationTween() {}

	public LocalRotationTween(Vector3 from, Vector3 to)
	{
		EulerFrom = from;
		EulerTo = to;
	}

	public override Action<GameObject, float, float> GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate(GameObject gameObj, float percentDone, float timeRemaining)
	{
		gameObj.transform.localEulerAngles = Vector3.Lerp(EulerFrom, EulerTo, percentDone);
	}
}

public static class LocalRotationTweenHelperFunctions
{
	public static TweenHolder AddLocalRotationTween(this TweenHolder tweenHolder, Vector3 to)
	{
		return AddLocalRotationTweenInternal(tweenHolder, tweenHolder.transform.localRotation.eulerAngles, to);
	}

	public static TweenHolder AddLocalRotationTween(this TweenHolder tweenHolder, Vector3 from, Vector3 to)
	{
		return AddLocalRotationTweenInternal(tweenHolder, from, to);
	}

	public static TweenHolder AddLocalRotationTween(this ITweenable tweenable, Vector3 to)
	{
		return AddLocalRotationTweenInternal(tweenable.TweenHolder, tweenable.gameObject.transform.localRotation.eulerAngles, to);
	}

	public static TweenHolder AddLocalRotationTween(this ITweenable tweenable, Vector3 from, Vector3 to)
	{
		return AddLocalRotationTweenInternal(tweenable.TweenHolder, from, to);
	}

	private static TweenHolder AddLocalRotationTweenInternal(TweenHolder tweenHolder, Vector3 from, Vector3 to)
	{
		return tweenHolder.AddTween(new LocalRotationTween(from, to)).Play();
	}
}
