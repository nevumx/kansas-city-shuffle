using UnityEngine;
using System;
using Nx;

public class IncrementalPositionTween : Tween
{
	public	Vector3	PositionTo	= Vector3.zero;
	public	bool	BoostSpeed	= false;

	public IncrementalPositionTween() {}

	public IncrementalPositionTween(Vector3 to, bool boostSpeed)
	{
		PositionTo = to;
		BoostSpeed = boostSpeed;
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
		float speed = destOffset.magnitude / timeRemaining * -6.0f * percentDone * (percentDone - 1.0f) + (BoostSpeed ? 2.5f * percentDone : 0.65f);
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