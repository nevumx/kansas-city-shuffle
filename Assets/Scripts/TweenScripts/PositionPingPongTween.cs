using UnityEngine;
using System;
using Nx;

public class PositionPingPongTween : Tween
{
	public Vector3 PositionFrom = Vector3.zero;
	public Vector3 PositionTo = Vector3.zero;
	public Vector3 PositionBackTo = Vector3.zero;

	public PositionPingPongTween() {}

	public PositionPingPongTween(Vector3 from, Vector3 to, Vector3 backTo)
	{
		PositionFrom = from;
		PositionTo = to;
		PositionBackTo = backTo;
	}

	public override Action<GameObject, float, float> GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate(GameObject gameObj, float percentDone, float timeRemaining)
	{
		if (percentDone < 0.5f)
		{
			gameObj.transform.position = Vector3.Lerp(PositionFrom, PositionTo, PingPongFunction(percentDone));
		}
		else
		{
			gameObj.transform.position = Vector3.Lerp(PositionBackTo, PositionTo, PingPongFunction(percentDone));
		}
	}

	public static float PingPongFunction(float percentDone)
	{
		return -Mathf.Abs(2.0f * percentDone - 1.0f) + 1.0f;
	}
}

public static class PositionPingPongTweenHelperFunctions
{
	public static TweenHolder AddPositionPingPongTween(this GameObject gameObj, Vector3 to)
	{
		TweenHolder tweenHolder = gameObj.GetComponent<TweenHolder>();
		tweenHolder.IfIsNullThen(() => tweenHolder = gameObj.AddComponent<TweenHolder>());
		return AddPositionPingPongTweenInternal(tweenHolder, tweenHolder.transform.position, to, tweenHolder.transform.position);
	}

	public static TweenHolder AddPositionPingPongTween(this GameObject gameObj, Vector3 from, Vector3 to)
	{
		TweenHolder tweenHolder = gameObj.GetComponent<TweenHolder>();
		tweenHolder.IfIsNullThen(() => tweenHolder = gameObj.AddComponent<TweenHolder>());
		return AddPositionPingPongTweenInternal(tweenHolder, from, to, from);
	}

	public static TweenHolder AddPositionPingPongTween(this GameObject gameObj, Vector3 from, Vector3 to, Vector3 backTo)
	{
		TweenHolder tweenHolder = gameObj.GetComponent<TweenHolder>();
		tweenHolder.IfIsNullThen(() => tweenHolder = gameObj.AddComponent<TweenHolder>());
		return AddPositionPingPongTweenInternal(tweenHolder, from, to, backTo);
	}

	public static TweenHolder AddPositionPingPongTween(this TweenHolder tweenHolder, Vector3 to)
	{
		return AddPositionPingPongTweenInternal(tweenHolder, tweenHolder.transform.position, to, tweenHolder.transform.position);
	}

	public static TweenHolder AddPositionPingPongTween(this TweenHolder tweenHolder, Vector3 from, Vector3 to)
	{
		return AddPositionPingPongTweenInternal(tweenHolder, from, to, from);
	}

	public static TweenHolder AddPositionPingPongTween(this TweenHolder tweenHolder, Vector3 from, Vector3 to, Vector3 backTo)
	{
		return AddPositionPingPongTweenInternal(tweenHolder, from, to, backTo);
	}

	private static TweenHolder AddPositionPingPongTweenInternal(TweenHolder tweenHolder, Vector3 from, Vector3 to, Vector3 backTo)
	{
		return tweenHolder.AddTween(new PositionPingPongTween(from, to, backTo)).Play();
	}
}