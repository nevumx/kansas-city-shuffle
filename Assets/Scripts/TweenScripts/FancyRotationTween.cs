using UnityEngine;
using System;

public class FancyRotationTween : CachedTransformTween
{
	public	Vector3	EulerFrom	= Vector3.zero;
	public	Vector3	EulerTo		= Vector3.zero;

	public FancyRotationTween() {}

	public FancyRotationTween(Vector3 from, Vector3 to)
	{
		EulerFrom = from;
		EulerTo = to;
	}

	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		Vector3 newRot = Vector3.Lerp(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(360.0f, 360.0f, 360.0f), TweenHolder.PercentDone);
		Quaternion VecXQ = Quaternion.Euler(new Vector3(newRot.x, 0.0f, 0.0f));
		Quaternion VecYQ = Quaternion.Euler(new Vector3(0.0f, newRot.y, 0.0f));
		Quaternion VecZQ = Quaternion.Euler(new Vector3(0.0f, 0.0f, newRot.z));
		_CachedTransform.localRotation =
				VecXQ
				* (VecXQ * VecYQ)
				* (VecXQ * VecYQ * VecZQ);
	}
}

public static class FancyRotationTweenHelperFunctions
{
	public static TweenHolder AddFancyRotationTween(this ITweenable tweenable)
	{
		return AddFancyRotationTweenInternal(tweenable.Holder);
	}

	private static TweenHolder AddFancyRotationTweenInternal(TweenHolder tweenHolder)
	{
		return tweenHolder.AddTween(new FancyRotationTween()).Play();
	}
}