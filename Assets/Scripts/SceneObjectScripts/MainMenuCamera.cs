﻿using UnityEngine;
using Nx;
using System;

[Serializable]
public struct TweenTransformPair
{
	[SerializeField]	private	Transform	_from;
	[SerializeField]	private	Transform	_to;

						public	Transform	From	{ get { return _from; } }
						public	Transform	To		{ get { return _to; } }
}

public class MainMenuCamera : MonoBehaviour, ITweenable
{
	[SerializeField]	private	TweenTransformPair[]	_tweenTransformPairs	= null;
	[SerializeField]	private	float					_durationForEachTween	= 6.0f;
	[SerializeField]	private	TweenHolder				_tweenHolder;
						public	TweenHolder				TweenHolder				{ get { return _tweenHolder; } }
						private	System.Random			_randomNumber			= new System.Random();
						private	int						_lastTransformPairIndex	= -1;

	private void Start()
	{
		TweenThroughNewTweenTransformPair();
	}

	private void TweenThroughNewTweenTransformPair()
	{
		int nextIndex;
		while ((nextIndex = _randomNumber.Next(_tweenTransformPairs.Length)) == _lastTransformPairIndex);
		_lastTransformPairIndex = nextIndex;
		TweenTransformPair nextPair = _tweenTransformPairs[nextIndex];
		this.AddPositionTween(nextPair.From.position, nextPair.To.position)
			.AddQuaternionRotationTween(nextPair.From.rotation, nextPair.To.rotation)
			.SetAnimationCurveFunction(TweenHolder.EaseInOutAnimationCurve)
			.SetDuration(_durationForEachTween)
			.AddToOnFinishedOnce(TweenThroughNewTweenTransformPair);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		_tweenTransformPairs.ForEach(t => 
		{
			if (t.From != null && t.To != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(t.From.position, t.To.position);
				for (int i = 0, iMax = 15; i <= iMax; ++i)
				{
					float percentLerped = TweenHolder.EaseInOutAnimationCurve((float) i / iMax);
					Vector3 positionLerp = Vector3.Lerp(t.From.position, t.To.position, percentLerped);
					Gizmos.color = Color.red;
					Gizmos.DrawLine(positionLerp, positionLerp + Vector3.Slerp(t.From.forward, t.To.forward, percentLerped));
				}
			}
		});
	}
}
