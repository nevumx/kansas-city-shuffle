using UnityEngine;
using System;
using Nx;

public class LocalRotationTween : Tween
{
	public Vector3 EulerFrom = Vector3.zero;
	public Vector3 EulerTo = Vector3.zero;

	public LocalRotationTween() {}

	public LocalRotationTween(Vector3 from, Vector3 to)
	{
		EulerFrom = from;
		EulerTo = to;
	}
	
	public override Action<GameObject, float, float> GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate(GameObject gameObj, float percentDone, float timeRemaining)
	{
		gameObj.transform.localEulerAngles = Vector3.Lerp(EulerFrom, EulerTo, percentDone);
	}
}

public static class LocalRotationTweenHelperFunctions
{
	public static TweenHolder AddLocalRotationTween(this GameObject gameObj, Vector3 to)
	{
		TweenHolder tweenHolder = gameObj.GetComponent<TweenHolder>();
		tweenHolder.IfIsNullThen(() => tweenHolder = gameObj.AddComponent<TweenHolder>());
		return AddLocalRotationTweenInternal(tweenHolder, tweenHolder.gameObject.transform.localRotation.eulerAngles, to);
	}
	
	public static TweenHolder AddLocalRotationTween(this GameObject gameObj, Vector3 from, Vector3 to)
	{
		TweenHolder tweenHolder = gameObj.GetComponent<TweenHolder>();
		tweenHolder.IfIsNullThen(() => tweenHolder = gameObj.AddComponent<TweenHolder>());
		return AddLocalRotationTweenInternal(tweenHolder, from, to);
	}

	public static TweenHolder AddLocalRotationTween(this TweenHolder tweenHolder, Vector3 to)
	{
		return AddLocalRotationTweenInternal(tweenHolder, tweenHolder.gameObject.transform.localRotation.eulerAngles, to);
	}
	
	public static TweenHolder AddLocalRotationTween(this TweenHolder tweenHolder, Vector3 from, Vector3 to)
	{
		return AddLocalRotationTweenInternal(tweenHolder, from, to);
	}
	
	private static TweenHolder AddLocalRotationTweenInternal(TweenHolder tweenHolder, Vector3 from, Vector3 to)
	{
		return tweenHolder.AddTween(new LocalRotationTween(from, to)).Play();
	}
}
