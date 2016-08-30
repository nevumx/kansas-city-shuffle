using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nx;

public class TweenHolder : MonoBehaviour, IFinishable
{
						private	Action						_onFinishedOnce;
	[NonSerialized]		public	float						Duration			= 1.0f;
	[NonSerialized]		public	float						Delay				= 0.0f;
						public	bool						IgnoreTimeScale		= false;
						private	LinkedList<Tween>			_tweens				= new LinkedList<Tween>();
						private	NxSortedLinkedList<Action>	_updateDelegates	= new NxSortedLinkedList<Action>(sortBy: d => ((Tween)d.Target).GetExecutionOrder());
						private	Action						_endOfFrameDelegates;
						private	float						_timeStarted;

	[SerializeField]	private	GameObject[]				_gameObjectsToChangeLayerOfDuringTween;
	[SerializeField]	private	int							_inTweenLayer;
	[SerializeField]	private	int							_outOfTweenLayer;

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

	private float _time
	{
		get
		{
			return IgnoreTimeScale ? Time.unscaledTime : Time.time;
		}
	}

	public float DeltaTime
	{
		get
		{
			return IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
		}
	}

	private float _timeElapsed
	{
		get
		{
			return _time - _timeStarted - Delay;
		}
	}

	public float TimeRemaining
	{
		get
		{
			return enabled ? Mathf.Max(Duration - _timeElapsed, 0.0f) : Duration;
		}
	}

	public float PercentDone
	{
		get
		{
			return enabled ? Mathf.Clamp(_timeElapsed / Duration, 0.0f, 1.0f) : 0.0f;
		}
	}

	private void Reset()
	{
		enabled = false;
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
		_timeStarted = _time;
		enabled = true;
		_gameObjectsToChangeLayerOfDuringTween.ForEach(g => g.layer = _shouldChangeLayer ? _inTweenLayer : _outOfTweenLayer);
		return this;
	}

	public void Finish()
	{
		if (enabled)
		{
			_timeStarted = -Duration - Delay;
			Update();
		}
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
	public TweenHolder SetIgnoreTimeScale(bool newIgnoreTimeScale)
	{
		IgnoreTimeScale = newIgnoreTimeScale;
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
	public TweenHolder ClearOnFinishedOnce()
	{
		_onFinishedOnce = null;
		return this;
	}
	public TweenHolder AddTween(Tween tweenToAdd)
	{
		RemoveDelegates(tweenToAdd); // Make sure it is only registered once
		AddDelegates(tweenToAdd);
		tweenToAdd.TweenHolder = this;
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
				node.Value.TweenHolder = null;
				_tweens.Remove(node);
				return this;
			}
		}
		return this;
	}
#endregion

	private void Update()
	{
		if (_timeElapsed < 0.0f)
		{
			return;
		}

		_updateDelegates.IfIsNotNullThen(o => o.ForEach(u => u()));
		StartCoroutine(RaiseEndOfFrameCallbacks());

		if (_timeElapsed >= Duration)
		{
			Action prevOnFinishedOnce = _onFinishedOnce;
			ResetVars();
			enabled = false;
			_gameObjectsToChangeLayerOfDuringTween.ForEach(g => g.layer = _outOfTweenLayer);
			_tweens.ForEach(t => t.TweenHolder = null);
			_tweens.Clear();
			_updateDelegates.Clear();
			_endOfFrameDelegates = null;
			prevOnFinishedOnce.Raise();
		}
	}

	private IEnumerator RaiseEndOfFrameCallbacks()
	{
		yield return new WaitForEndOfFrame();
		_endOfFrameDelegates.IfIsNotNullThen(d => d());
	}

	private void AddDelegates(Tween tweenToAdd)
	{
		Action updateDelegate = tweenToAdd.GetUpdateDelegate();
		Action endOfFrameDelegate = tweenToAdd.GetEndOfFrameDelegate();

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
			delegateList.FirstOrDefault(l => object.ReferenceEquals(l.Target, tween)).IfIsNotNullThen(d => _endOfFrameDelegates -= (Action)d);
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
	public	TweenHolder	TweenHolder	{ protected get; set; }

	public virtual int GetExecutionOrder() // For tweens which operate on the same property.
	{
		return 0;
	}

	public virtual Action GetUpdateDelegate() { return null; }

	public virtual Action GetEndOfFrameDelegate() { return null; }
}