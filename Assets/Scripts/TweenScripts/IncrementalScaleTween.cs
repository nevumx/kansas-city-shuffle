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
	
	public override Action<GameObject, float, float> GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate(GameObject gameObj, float percentDone, float timeRemaining)
	{
		if (Mathf.Approximately(timeRemaining, 0.0f))
		{
			gameObj.transform.localScale = ScaleTo;
			return;
		}

		Vector3 destOffset = ScaleTo - gameObj.transform.localScale;
		float speed = destOffset.magnitude / timeRemaining;
		Vector3 nextDest = gameObj.transform.localScale + destOffset.normalized * speed * Time.deltaTime;

		if (Vector3.Distance(gameObj.transform.localScale, nextDest)
			> Vector3.Distance(gameObj.transform.localScale, ScaleTo))
		{
			gameObj.transform.localScale = gameObj.transform.localScale = ScaleTo;
		}
		else
		{
			gameObj.transform.localScale = gameObj.transform.localScale = nextDest;
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
