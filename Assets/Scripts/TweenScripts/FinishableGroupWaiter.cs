using System;
using Nx;

public class FinishableGroupWaiter : IFinishable
{
	private	Action	_onAllFinished;

	public	int		NumFinishableToWaitFor	{ get; private set; }
	public	bool	Done					{ get; private set; }

	private	bool	_ready;
	public	bool	Ready
	{
		get
		{
			return _ready;
		}
		set
		{
			_ready = value;
			if (NumFinishableToWaitFor <= 0 && !Done)
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

	public void AddFinishable(IFinishable finishable)
	{
		if (Done)
		{
			NxUtils.LogWarning("Trying to add a tween to a TweenGroupWaiter that has already expired.");
			return;
		}
		if (finishable != null)
		{
			finishable.AddToOnFinished(TweenDone);
			++NumFinishableToWaitFor;
		}
	}

	public void AddToOnFinished(Action toAdd)
	{
		AddToOnAllFinished(toAdd);
	}

	public void AddToOnAllFinished(Action toAdd)
	{
		toAdd.IfIsNotNullThen(a => _onAllFinished += a);
	}

	private void TweenDone()
	{
		if (--NumFinishableToWaitFor <= 0 && Ready)
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