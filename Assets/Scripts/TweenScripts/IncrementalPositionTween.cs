using UnityEngine;
using System;
using Nx;

// Similar to PositionTween except its PositionTo can be changed without "jumping;"
// The tween will simply change course towards the new position. Note that Duration won't change, though
public class IncrementalPositionTween : Tween
{
	public Vector3 PositionTo = Vector3.zero;

	public IncrementalPositionTween() {}

	public IncrementalPositionTween(Vector3 to)
	{
		PositionTo = to;
	}

	public override Action<GameObject, float, float> GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate(GameObject gameObj, float percentDone, float timeRemaining)
	{
		if (Mathf.Approximately(timeRemaining, 0.0f))
		{
			gameObj.transform.position = PositionTo;
			return;
		}
		Vector3 destOffset = PositionTo - gameObj.transform.position;
		float speed = destOffset.magnitude / timeRemaining;
		Vector3 nextDest = gameObj.transform.position + destOffset.normalized * speed * Time.deltaTime;
		if (Vector3.Distance(gameObj.transform.position, nextDest)
			> Vector3.Distance(gameObj.transform.position, PositionTo))
		{
			gameObj.transform.position = PositionTo;
		}
		else
		{
			gameObj.transform.position = nextDest;
		}
	}
}

public static class IncrementalPositionTweenHelperFunctions
{
	public static TweenHolder AddIncrementalPositionTween(this GameObject gameObj, Vector3 to)
	{
		TweenHolder tweenHolder = gameObj.GetComponent<TweenHolder>();
		tweenHolder.IfIsNullThen(() => tweenHolder = gameObj.AddComponent<TweenHolder>());
		return AddIncrementalPositionTweenInternal(tweenHolder, to);
	}

	public static TweenHolder AddIncrementalPositionTween(this TweenHolder tweenHolder, Vector3 to)
	{
		return AddIncrementalPositionTweenInternal(tweenHolder, to);
	}

	private static TweenHolder AddIncrementalPositionTweenInternal(TweenHolder tweenHolder, Vector3 to)
	{
		return tweenHolder.AddTween(new IncrementalPositionTween(to)).Play();
	}
}