using UnityEngine;
using System;

public class PositionPingPongTween : CachedTransformTween
{
	public	Vector3	PositionFrom	= Vector3.zero;
	public	Vector3	PositionTo		= Vector3.zero;
	public	Vector3	PositionBackTo	= Vector3.zero;

	public PositionPingPongTween() {}

	public PositionPingPongTween(Vector3 from, Vector3 to, Vector3 backTo)
	{
		PositionFrom = from;
		PositionTo = to;
		PositionBackTo = backTo;
	}

	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		float percentDone = TweenHolder.EaseInOutPingPongAnimationCurveFastOutro(TweenHolder.PercentDone);
		if (percentDone < 0.5f)
		{
			_CachedTransform.position = Vector3.Lerp(PositionFrom, PositionTo, PingPongFunction(percentDone));
		}
		else
		{
			_CachedTransform.position = Vector3.Lerp(PositionBackTo, PositionTo, PingPongFunction(percentDone));
		}
	}

	public static float PingPongFunction(float percentDone)
	{
		return -Mathf.Abs(2.0f * percentDone - 1.0f) + 1.0f;
	}
}

public static class PositionPingPongTweenHelperFunctions
{
	public static TweenHolder AddPositionPingPongTween(this ITweenable tweenable, Vector3 to)
	{
		return AddPositionPingPongTweenInternal(tweenable.Holder, tweenable.gameObject.transform.position, to, tweenable.gameObject.transform.position);
	}

	public static TweenHolder AddPositionPingPongTween(this ITweenable tweenable, Vector3 to, Vector3 backTo)
	{
		return AddPositionPingPongTweenInternal(tweenable.Holder, tweenable.gameObject.transform.position, to, backTo);
	}

	public static TweenHolder AddPositionPingPongTween(this ITweenable tweenable, Vector3 from, Vector3 to, Vector3 backTo)
	{
		return AddPositionPingPongTweenInternal(tweenable.Holder, from, to, backTo);
	}

	private static TweenHolder AddPositionPingPongTweenInternal(TweenHolder tweenHolder, Vector3 from, Vector3 to, Vector3 backTo)
	{
		return tweenHolder.AddTween(new PositionPingPongTween(from, to, backTo)).Play();
	}
}