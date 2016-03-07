using System;

public interface Finishable
{
	void AddToOnFinished(Action toAdd);
}
