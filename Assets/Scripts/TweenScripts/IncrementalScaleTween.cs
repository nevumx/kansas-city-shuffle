using UnityEngine;
using System;
using Nx;

public class IncrementalScaleTween : Tween
{
	public	Vector3	ScaleTo	= Vector3.zero;

	public IncrementalScaleTween() {}

	public IncrementalScaleTween(Vector3 to)
	{
		ScaleTo = to;
	}
	
	public override Action<Transform, float, float> GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate(Transform gameObjTransform, float percentDone, float timeRemaining)
	{
		if (Mathf.Approximately(timeRemaining, 0.0f))
		{
			gameObjTransform.localScale = ScaleTo;
			return;
		}

		Vector3 destOffset = ScaleTo - gameObjTransform.localScale;
		float speed = destOffset.magnitude / timeRemaining * -6.0f * percentDone * (percentDone - 1.0f) + 0.65f;
		Vector3 nextDest = gameObjTransform.localScale + destOffset.normalized * speed * Time.deltaTime;

		if (Vector3.Distance(gameObjTransform.localScale, nextDest)
			> Vector3.Distance(gameObjTransform.localScale, ScaleTo))
		{
			gameObjTransform.localScale = gameObjTransform.localScale = ScaleTo;
		}
		else
		{
			gameObjTransform.localScale = gameObjTransform.localScale = nextDest;
		}
	}
}

public static class IncrementalScaleTweenHelperFunctions
{
	public static TweenHolder AddIncrementalScaleTween(this TweenHolder tweenHolder, Vector3 to)
	{
		return AddIncrementalScaleTweenInternal(tweenHolder, to);
	}

	public static TweenHolder AddIncrementalScaleTween(this ITweenable tweenable, Vector3 to)
	{
		return AddIncrementalScaleTweenInternal(tweenable.TweenHolder, to);
	}

	private static TweenHolder AddIncrementalScaleTweenInternal(TweenHolder tweenHolder, Vector3 to)
	{
		return tweenHolder.AddTween(new IncrementalScaleTween(to)).Play();
	}
}
