using System;
using Nx;

public class FinishableGroupWaiter
{
	private	int		_numFinishableToWaitFor	= 0;
	public	int		NumFinishableToWaitFor	{ get { return _numFinishableToWaitFor; } }

	private	Action	_onAllFinished;

	public	bool	Done					{ get; private set; }

	private	bool	_ready					= false;
	public	bool	Ready
	{
		get
		{
			return _ready;
		}
		set
		{
			_ready = value;
			if (_numFinishableToWaitFor <= 0 && !Done)
			{
				Finish();
			}
		}
	}

	public FinishableGroupWaiter() {}

	public FinishableGroupWaiter(Action toAdd)
	{
		AddToOnAllFinished(toAdd);
	}

	public void AddFinishable(Finishable finishable)
	{
		if (Done)
		{
			NxUtils.LogWarning("Trying to add a tween to a TweenGroupWaiter that has already expired.");
			return;
		}
		if (finishable != null)
		{
			finishable.AddToOnFinished(TweenDone);
			++_numFinishableToWaitFor;
		}
	}

	public void AddToOnAllFinished(Action toAdd)
	{
		toAdd.IfIsNotNullThen(a => _onAllFinished += a);
	}

	private void TweenDone()
	{
		if (--_numFinishableToWaitFor <= 0 && Ready)
		{
			Finish();
		}
	}

	private void Finish()
	{
		Done = true;
		_onAllFinished.Raise();
		_onAllFinished = null;
	}
}