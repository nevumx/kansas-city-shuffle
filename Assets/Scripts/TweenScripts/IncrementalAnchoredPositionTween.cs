using UnityEngine;
using System;
using Nx;

public class IncrementalAnchoredPositionTween : Tween
{
	private	RectTransform	_targetRectTransform;
	public	Vector2			PositionTo	= Vector2.zero;

	public IncrementalAnchoredPositionTween() {}

	public IncrementalAnchoredPositionTween(RectTransform targetRectTransform, Vector2 to)
	{
		_targetRectTransform = targetRectTransform;
		PositionTo = to;
	}

	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		if (Mathf.Approximately(TweenHolder.TimeRemaining, 0.0f))
		{
			_targetRectTransform.anchoredPosition = PositionTo;
			return;
		}

		Vector2 destOffset = PositionTo - _targetRectTransform.anchoredPosition;
		float speed = destOffset.magnitude / TweenHolder.TimeRemaining;
		Vector2 nextDest = _targetRectTransform.anchoredPosition + destOffset.normalized * speed * TweenHolder.DeltaTime;

		if (Vector2.Distance(_targetRectTransform.anchoredPosition, nextDest)
			> Vector2.Distance(_targetRectTransform.anchoredPosition, PositionTo))
		{
			_targetRectTransform.anchoredPosition = PositionTo;
		}
		else
		{
			_targetRectTransform.anchoredPosition = nextDest;
		}
	}
}

public static class IncrementalAnchorPositionTweenHelperFunctions
{
	public static TweenableGraphics AddIncrementalAnchoredPositionTween(this TweenableGraphics tweenableGraphics, Vector2 to)
	{
		tweenableGraphics.TweenHolder.AddTween(new IncrementalAnchoredPositionTween(tweenableGraphics.RootRectTransform, to)).Play();
		return tweenableGraphics;
	}
}