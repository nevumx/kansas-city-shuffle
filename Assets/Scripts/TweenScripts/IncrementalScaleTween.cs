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
	
	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		if (Mathf.Approximately(TweenHolder.TimeRemaining, 0.0f))
		{
			TweenHolder.transform.localScale = ScaleTo;
			return;
		}

		Vector3 destOffset = ScaleTo - TweenHolder.transform.localScale;
		float speed = destOffset.magnitude / TweenHolder.TimeRemaining
			* Mathf.Max(-6.0f * TweenHolder.PercentDone * (TweenHolder.PercentDone - 1.0f), TweenHolder.PercentDone > 0.5f ? 1.0f : 0.0f);
		Vector3 nextDest = TweenHolder.transform.localScale + destOffset.normalized * speed * TweenHolder.DeltaTime;

		if (Vector3.Distance(TweenHolder.transform.localScale, nextDest)
			> Vector3.Distance(TweenHolder.transform.localScale, ScaleTo))
		{
			TweenHolder.transform.localScale = ScaleTo;
		}
		else
		{
			TweenHolder.transform.localScale = nextDest;
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
