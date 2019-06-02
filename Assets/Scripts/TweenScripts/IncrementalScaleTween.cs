using UnityEngine;
using System;

public class IncrementalScaleTween : CachedTransformTween
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
			_CachedTransform.localScale = ScaleTo;
			return;
		}

		Vector3 destOffset = ScaleTo - _CachedTransform.localScale;
		float speed = destOffset.magnitude / TweenHolder.TimeRemaining
			* Mathf.Max(-6.0f * TweenHolder.PercentDone * (TweenHolder.PercentDone - 1.0f), TweenHolder.PercentDone > 0.5f ? 1.0f : 0.0f);
		Vector3 nextDest = _CachedTransform.localScale + destOffset.normalized * speed * TweenHolder.DeltaTime;

		if ((_CachedTransform.localScale - nextDest).sqrMagnitude > (_CachedTransform.localScale - ScaleTo).sqrMagnitude)
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
	public static TweenHolder AddIncrementalScaleTween(this ITweenable tweenable, Vector3 to)
	{
		return tweenable.Holder.AddTween(new IncrementalScaleTween(to)).Play();
	}
}
