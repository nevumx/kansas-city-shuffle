using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nx;

public class TweenHolder : MonoBehaviour, Finishable
{
	private	Action													_onFinishedOnce;
	public	float													Duration;
	public	float													Delay;
	public	Func<float, float>										AnimationCurveFunction;
	private	LinkedList<Tween>										_tweens					= new LinkedList<Tween>();
	private	NxSortedLinkedList<Action<GameObject, float, float>>	_updateDelegates		= new NxSortedLinkedList<Action<GameObject, float, float>>
																								(sortBy: d => ((Tween)d.Target).GetExecutionOrder());
	private	Action<GameObject>										_endOfFrameDelegates;
	private	float													_timeStarted;

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
		AnimationCurveFunction = EaseInOutAnimationCurve; // LinearAnimationCurve
	}
	
	public TweenHolder Play()
	{
		_timeStarted = Time.time;
		enabled = true;
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
	public TweenHolder SetAnimationCurveFunction(Func<float, float> newAnimationCurveFunction)
	{
		newAnimationCurveFunction.IfIsNotNullThen(f => AnimationCurveFunction = f);
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
			enabled = false;
		}
		else if (timeElapsed >= 0.0f)
		{
			percentDone = Mathf.Clamp(timeElapsed / Duration, 0.0f, 1.0f);
		}
		else // delay not over
		{
			return;
		}
		_updateDelegates.IfIsNotNullThen(u => u.ForEach(d => d(gameObject, AnimationCurveFunction(percentDone), done ? 0.0f : Mathf.Max(0.0f, TimeRemaining))));
		StartCoroutine(RaiseEndOfFrameCallbacks());
		if (done)
		{
			Action prevOnFinishedOnce = _onFinishedOnce;
			ResetVars();
			enabled = false;
			_tweens.Clear();
			_updateDelegates.Clear();
			_endOfFrameDelegates = null;
			prevOnFinishedOnce.Raise();
		}
	}

	private IEnumerator RaiseEndOfFrameCallbacks()
	{
		yield return new WaitForEndOfFrame();
		_endOfFrameDelegates.IfIsNotNullThen(d => d(gameObject));
	}

	private void AddDelegates(Tween tweenToAdd)
	{
		Action<GameObject, float, float> updateDelegate = tweenToAdd.GetUpdateDelegate();
		Action<GameObject> endOfFrameDelegate = tweenToAdd.GetEndOfFrameDelegate();

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
			delegateList.FirstOrDefault(l => object.ReferenceEquals(l.Target, tween)).IfIsNotNullThen(d => _endOfFrameDelegates -= (Action<GameObject>)d);
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
	
	public static float LinearAnimationCurve(float percentDone) { return percentDone; }

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
	
	public virtual Action<GameObject, float, float> GetUpdateDelegate() { return null; }
	
	public virtual Action<GameObject> GetEndOfFrameDelegate() { return null; }
}