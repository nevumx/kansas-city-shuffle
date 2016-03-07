using UnityEngine;
using System;
using Nx;

public class LocalQuaternionRotationTween : Tween
{
	public Quaternion From = Quaternion.identity;
	public Quaternion To = Quaternion.identity;

	public LocalQuaternionRotationTween() {}

	public LocalQuaternionRotationTween(Quaternion from, Quaternion to)
	{
		From = from;
		To = to;
	}
	
	public override Action<GameObject, float, float> GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate(GameObject gameObj, float percentDone, float timeRemaining)
	{
		gameObj.transform.localRotation = Quaternion.Slerp(From, To, percentDone);
	}
}

public static class LocalQuaternionRotationTweenHelperFunctions
{
	public static TweenHolder AddLocalQuaternionRotationTween(this GameObject gameObj, Quaternion to)
	{
		TweenHolder tweenHolder = gameObj.GetComponent<TweenHolder>();
		tweenHolder.IfIsNullThen(() => tweenHolder = gameObj.AddComponent<TweenHolder>());
		return AddLocalQuaternionRotationTweenInternal(tweenHolder, tweenHolder.gameObject.transform.localRotation, to);
	}
	
	public static TweenHolder AddLocalQuaternionRotationTween(this GameObject gameObj, Quaternion from, Quaternion to)
	{
		TweenHolder tweenHolder = gameObj.GetComponent<TweenHolder>();
		tweenHolder.IfIsNullThen(() => tweenHolder = gameObj.AddComponent<TweenHolder>());
		return AddLocalQuaternionRotationTweenInternal(tweenHolder, from, to);
	}

	public static TweenHolder AddLocalQuaternionRotationTween(this TweenHolder tweenHolder, Quaternion to)
	{
		return AddLocalQuaternionRotationTweenInternal(tweenHolder, tweenHolder.gameObject.transform.localRotation, to);
	}
	
	public static TweenHolder AddLocalQuaternionRotationTween(this TweenHolder tweenHolder, Quaternion from, Quaternion to)
	{
		return AddLocalQuaternionRotationTweenInternal(tweenHolder, from, to);
	}
	
	private static TweenHolder AddLocalQuaternionRotationTweenInternal(TweenHolder tweenHolder, Quaternion from, Quaternion to)
	{
		return tweenHolder.AddTween(new LocalQuaternionRotationTween(from, to)).Play();
	}
}
