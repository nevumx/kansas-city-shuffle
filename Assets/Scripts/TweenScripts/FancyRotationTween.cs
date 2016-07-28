using UnityEngine;
using System;
using Nx;

public class FancyRotationTween : Tween
{
	public	Vector3	EulerFrom	= Vector3.zero;
	public	Vector3	EulerTo		= Vector3.zero;

	public FancyRotationTween() {}

	public FancyRotationTween(Vector3 from, Vector3 to)
	{
		EulerFrom = from;
		EulerTo = to;
	}

	public override Action<Transform, float, float> GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate(Transform gameObjTransform, float percentDone, float timeRemaining)
	{
		Vector3 newRot = Vector3.Lerp(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(360.0f, 360.0f, 360.0f), percentDone);
		Quaternion VecXQ = Quaternion.Euler(new Vector3(newRot.x, 0.0f, 0.0f));
		Quaternion VecYQ = Quaternion.Euler(new Vector3(0.0f, newRot.y, 0.0f));
		Quaternion VecZQ = Quaternion.Euler(new Vector3(0.0f, 0.0f, newRot.z));
		gameObjTransform.localRotation =
				VecXQ
				* (VecXQ * VecYQ)
				* (VecXQ * VecYQ * VecZQ);
	}
}

public static class FancyRotationTweenHelperFunctions
{
	public static TweenHolder AddFancyRotationTween(this TweenHolder tweenHolder)
	{
		return AddFancyRotationTweenInternal(tweenHolder);
	}

	public static TweenHolder AddFancyRotationTween(this ITweenable tweenable)
	{
		return AddFancyRotationTweenInternal(tweenable.TweenHolder);
	}

	private static TweenHolder AddFancyRotationTweenInternal(TweenHolder tweenHolder)
	{
		return tweenHolder.AddTween(new FancyRotationTween()).Play();
	}
}
