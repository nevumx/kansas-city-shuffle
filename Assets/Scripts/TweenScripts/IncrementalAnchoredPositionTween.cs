using UnityEngine;

public class IncrementalAnchoredPositionTween : Tween
{
	private	readonly	RectTransform	_targetRectTransform;
	public				Vector2			PositionTo	= Vector2.zero;

	public IncrementalAnchoredPositionTween() {}

	public IncrementalAnchoredPositionTween(RectTransform targetRectTransform, Vector2 to)
	{
		_targetRectTransform = targetRectTransform;
		PositionTo = to;
	}

	public override void OnUpdate()
	{
		if (Mathf.Approximately(TweenHolder.TimeRemaining, 0.0f))
		{
			_targetRectTransform.anchoredPosition = PositionTo;
			return;
		}

		Vector2 destOffset = PositionTo - _targetRectTransform.anchoredPosition;
		float speed = destOffset.magnitude / TweenHolder.TimeRemaining;
		Vector2 nextDest = _targetRectTransform.anchoredPosition + destOffset.normalized * speed * TweenHolder.DeltaTime;

		if ((_targetRectTransform.anchoredPosition - nextDest).sqrMagnitude > (_targetRectTransform.anchoredPosition - PositionTo).sqrMagnitude)
		{
			_targetRectTransform.anchoredPosition = PositionTo;
		}
		else
		{
			_targetRectTransform.anchoredPosition = nextDest;
		}
	}
}

public static class IncrementalAnchoredPositionTweenHelperFunctions
{
	public static TweenableGraphics AddIncrementalAnchoredPositionTween(this TweenableGraphics tweenableGraphics, Vector2 to)
	{
		tweenableGraphics.Holder.AddTween(new IncrementalAnchoredPositionTween(tweenableGraphics.RootRectTransform, to)).Play();
		return tweenableGraphics;
	}
	public static TweenableAlphaMultipliedGraphics AddIncrementalAnchoredPositionTween(this TweenableAlphaMultipliedGraphics tweenableGraphics, Vector2 to)
	{
		tweenableGraphics.Holder.AddTween(new IncrementalAnchoredPositionTween(tweenableGraphics.RootRectTransform, to)).Play();
		return tweenableGraphics;
	}
}