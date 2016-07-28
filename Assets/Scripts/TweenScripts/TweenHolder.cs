﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nx;

public class TweenHolder : MonoBehaviour, IFinishable
{
						private	Action												_onFinishedOnce;
						public	float												Duration				= 1.0f;
						public	float												Delay					= 0.0f;
						private	LinkedList<Tween>									_tweens					= new LinkedList<Tween>();
						private	NxSortedLinkedList<Action<Transform, float, float>>	_updateDelegates		= new NxSortedLinkedList<Action<Transform, float, float>>
																												(sortBy: d => ((Tween)d.Target).GetExecutionOrder());
						private	Action<Transform>									_endOfFrameDelegates;
						private	float												_timeStarted;

	[SerializeField]	private	GameObject[]										_gameObjectsToChangeLayerOfDuringTween;
	[SerializeField]	private	int													_inTweenLayer;
	[SerializeField]	private	int													_outOfTweenLayer;

	private bool _shouldChangeLayer = true;
	public bool ShouldChangeLayer
	{
		get
		{
			return _shouldChangeLayer;
		}
		set
		{
			_shouldChangeLayer = value;
			_gameObjectsToChangeLayerOfDuringTween.ForEach(g => g.layer = _shouldChangeLayer && enabled ? _inTweenLayer : _outOfTweenLayer);
		}
	}

	public float TimeRemaining
	{
		get
		{
			return enabled ? Duration - (Time.time - _timeStarted - Delay) : Duration;
		}
	}

	private void Awake()
	{
		ResetVars();
	}

	private void ResetVars()
	{
		_onFinishedOnce = null;
		Duration = 1.0f;
		Delay = 0.0f;
		_shouldChangeLayer = true;
	}

	public TweenHolder Play()
	{
		_timeStarted = Time.time;
		enabled = true;
		_gameObjectsToChangeLayerOfDuringTween.ForEach(g => g.layer = _shouldChangeLayer ? _inTweenLayer : _outOfTweenLayer);
		return this;
	}

#region Method Chainers
	public TweenHolder SetDuration(float newDuration)
	{
		Duration = newDuration;
		return this;
	}
	public TweenHolder SetDelay(float newDelay)
	{
		Delay = newDelay;
		return this;
	}
	public TweenHolder SetShouldChangeLayer(bool newShouldChangeLayer)
	{
		ShouldChangeLayer = newShouldChangeLayer;
		return this;
	}
	public TweenHolder AddToOnFinishedOnce(Action toAdd)
	{
			toAdd.IfIsNotNullThen(a => _onFinishedOnce += a);
			return this;
	}
	public TweenHolder AddTween(Tween tweenToAdd)
	{
		RemoveDelegates(tweenToAdd); // Make sure it is only registered once
		AddDelegates(tweenToAdd);
		for (LinkedListNode<Tween> node = _tweens.First; node != null; node = node.Next)
		{
			if (node.Value.GetType() == tweenToAdd.GetType())
			{
				if (!object.ReferenceEquals(node.Value, tweenToAdd))
				{
					RemoveDelegates(node.Value);
					node.Value = tweenToAdd;
				}
				return this;
			}
		}
		_tweens.AddLast(tweenToAdd);
		return this;
	}
	public TweenHolder RemoveTweenOfType<T>() where T : Tween, new()
	{
		Type cachedTweenType = typeof(T);
		for (LinkedListNode<Tween> node = _tweens.First; node != null; node = node.Next)
		{
			if (node.Value.GetType() == cachedTweenType)
			{
				RemoveDelegates(node.Value);
				_tweens.Remove(node);
				return this;
			}
		}
		return this;
	}
#endregion

	private void Update()
	{
		float percentDone = 0.0f;
		float timeElapsed = Time.time - _timeStarted - Delay;
		bool done = timeElapsed >= Duration;
		if (done)
		{
			percentDone = 1.0f;
		}
		else if (timeElapsed >= 0.0f)
		{
			percentDone = Mathf.Clamp(timeElapsed / Duration, 0.0f, 1.0f);
		}
		else // delay not over
		{
			return;
		}

		_updateDelegates.IfIsNotNullThen(u => u.ForEach(d => d(transform, percentDone, done ? 0.0f : Mathf.Max(0.0f, TimeRemaining))));
		StartCoroutine(RaiseEndOfFrameCallbacks());

		if (done)
		{
			Action prevOnFinishedOnce = _onFinishedOnce;
			ResetVars();
			enabled = false;
			_gameObjectsToChangeLayerOfDuringTween.ForEach(g => g.layer = _outOfTweenLayer);
			_tweens.Clear();
			_updateDelegates.Clear();
			_endOfFrameDelegates = null;
			prevOnFinishedOnce.Raise();
		}
	}

	private IEnumerator RaiseEndOfFrameCallbacks()
	{
		yield return new WaitForEndOfFrame();
		_endOfFrameDelegates.IfIsNotNullThen(d => d(transform));
	}

	private void AddDelegates(Tween tweenToAdd)
	{
		Action<Transform, float, float> updateDelegate = tweenToAdd.GetUpdateDelegate();
		Action<Transform> endOfFrameDelegate = tweenToAdd.GetEndOfFrameDelegate();

		if (updateDelegate != null && updateDelegate.GetInvocationList().Length == 1 && object.ReferenceEquals(updateDelegate.Target, tweenToAdd)
			&& !_updateDelegates.Exists(d => object.ReferenceEquals(d.Target, tweenToAdd)))
		{
			_updateDelegates.AddSorted(updateDelegate);
		}

		if (endOfFrameDelegate != null && endOfFrameDelegate.GetInvocationList().Length == 1 && object.ReferenceEquals(endOfFrameDelegate.Target, tweenToAdd)
			&& (_endOfFrameDelegates == null || !_endOfFrameDelegates.GetInvocationList().Exists(d => object.ReferenceEquals(d.Target, tweenToAdd))))
		{
			_endOfFrameDelegates += endOfFrameDelegate;
		}
	}

	private void RemoveDelegates(Tween tween)
	{
		_updateDelegates.FirstOrDefault(l => object.ReferenceEquals(l.Target, tween)).IfIsNotNullThen(d => _updateDelegates.Remove(d));
		if (_endOfFrameDelegates != null)
		{
			Delegate[] delegateList = _endOfFrameDelegates.GetInvocationList();
			delegateList.FirstOrDefault(l => object.ReferenceEquals(l.Target, tween)).IfIsNotNullThen(d => _endOfFrameDelegates -= (Action<Transform>)d);
		}
	}

	public T GetTweenOfType<T>() where T : Tween, new()
	{
		Type cachedTweenType = typeof(T);
		foreach (Tween tween in _tweens)
		{
			if (tween.GetType() == cachedTweenType)
			{
				return (T)tween;
			}
		}
		return default(T);
	}

	public static float EaseInOutAnimationCurve(float percentDone)
	{
		return -2.0f * percentDone * percentDone * (percentDone - 1.5f);
	}

	public static float EaseInOutPingPongAnimationCurve(float percentDone)
	{
		float cachedValue = Mathf.Abs(2.0f * percentDone - 1.0f);
		return (percentDone < 0.5f ? 1.0f : -1.0f) * cachedValue * cachedValue * (cachedValue - 1.5f) + 0.5f;
	}

	public static float EaseInOutPingPongAnimationCurveFastOutro(float percentDone)
	{
		if (percentDone > 0.5f)
		{
			return 2.0f * (percentDone - 0.5f) * (percentDone - 0.5f) + 0.5f;
		}
		return -8.0f * percentDone * percentDone * (percentDone - 0.75f);
	}

	public void AddToOnFinished(Action toAdd)
	{
		AddToOnFinishedOnce(toAdd);
	}
}

public class Tween
{
	public virtual int GetExecutionOrder() // For tweens which operate on the same property.
	{
		return 0;
	}

	public virtual Action<Transform, float, float> GetUpdateDelegate() { return null; }

	public virtual Action<Transform> GetEndOfFrameDelegate() { return null; }
}