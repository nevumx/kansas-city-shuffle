using System;

public interface IFinishable
{
	void AddToOnFinished(Action toAdd);
}
