﻿using UnityEngine;
using System;
using Nx;

public class IncrementalPositionTween : CachedTransformTween
{
	public	Vector3	PositionTo	= Vector3.zero;
	public	bool	BoostSpeed	= false;

	public IncrementalPositionTween() {}

	public IncrementalPositionTween(Vector3 to, bool boostSpeed)
	{
		PositionTo = to;
		BoostSpeed = boostSpeed;
	}

	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		if (Mathf.Approximately(TweenHolder.TimeRemaining, 0.0f))
		{
			_CachedTransform.position = PositionTo;
			return;
		}

		Vector3 destOffset = PositionTo - _CachedTransform.position;
		float speed;
		if (BoostSpeed)
		{
			speed = destOffset.magnitude / TweenHolder.TimeRemaining 
				* -6.0f * TweenHolder.PercentDone * (TweenHolder.PercentDone - 1.0f) + 2.5f * TweenHolder.PercentDone;
		}
		else
		{
			speed = destOffset.magnitude / TweenHolder.TimeRemaining
				* Mathf.Max(-6.0f * TweenHolder.PercentDone * (TweenHolder.PercentDone - 1.0f), TweenHolder.PercentDone > 0.5f ? 1.0f : 0.0f);
		}
		Vector3 nextDest = _CachedTransform.position + destOffset.normalized * speed * TweenHolder.DeltaTime;

		if ((_CachedTransform.position - nextDest).sqrMagnitude > (_CachedTransform.position -  PositionTo).sqrMagnitude)
		{
			_CachedTransform.position = PositionTo;
		}
		else
		{
			_CachedTransform.position = nextDest;
		}
	}
}

public static class IncrementalPositionTweenHelperFunctions
{
	public static TweenHolder AddIncrementalPositionTween(this TweenHolder tweenHolder, Vector3 to, bool boostSpeed = false)
	{
		return AddIncrementalPositionTweenInternal(tweenHolder, to, boostSpeed);
	}

	public static TweenHolder AddIncrementalPositionTween(this ITweenable tweenable, Vector3 to, bool boostSpeed = false)
	{
		return AddIncrementalPositionTweenInternal(tweenable.TweenHolder, to, boostSpeed);
	}

	private static TweenHolder AddIncrementalPositionTweenInternal(TweenHolder tweenHolder, Vector3 to, bool boostSpeed)
	{
		return tweenHolder.AddTween(new IncrementalPositionTween(to, boostSpeed)).Play();
	}
}