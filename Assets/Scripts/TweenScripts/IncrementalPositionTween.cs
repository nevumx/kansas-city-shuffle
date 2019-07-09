using UnityEngine;

public class IncrementalPositionTween : CachedTransformTween
{
	public	Vector3	PositionTo		= Vector3.zero;
	public	bool	BoostSpeed;

	private	Vector3	_prevPosition	= Vector3.zero;

	public IncrementalPositionTween() {}

	public IncrementalPositionTween(Vector3 to, bool boostSpeed)
	{
		PositionTo = to;
		BoostSpeed = boostSpeed;
	}

	public override void CacheNeededData()
	{
		base.CacheNeededData();
		_prevPosition = _CachedTransform.position;
	}

	protected virtual float GetOffsetHeight() { return 0.0f; }

	public override void OnUpdate()
	{
		if (Mathf.Approximately(TweenHolder.TimeRemaining, 0.0f))
		{
			_CachedTransform.position = PositionTo;
			return;
		}

		Vector3 destOffset = PositionTo - _prevPosition;
		float speed;
		if (BoostSpeed)
		{
			speed = destOffset.magnitude / TweenHolder.TimeRemaining 
				* -6.0f * TweenHolder.PercentDone * (TweenHolder.PercentDone - 1.0f) + 2.5f * TweenHolder.PercentDone;
		}
		else
		{
			speed = destOffset.magnitude / TweenHolder.TimeRemaining
				* Mathf.Max(-6.0f * TweenHolder.PercentDone * (TweenHolder.PercentDone - 1.0f), TweenHolder.PercentDone > 0.5f ? 1.0f : 0.0f);
		}
		Vector3 prevPrevPosition = _prevPosition;
		_prevPosition += destOffset.normalized * speed * TweenHolder.DeltaTime;

		if ((prevPrevPosition - _prevPosition).sqrMagnitude > (prevPrevPosition - PositionTo).sqrMagnitude)
		{
			_CachedTransform.position = PositionTo + GetOffsetHeight() * Vector3.up;
		}
		else
		{
			_CachedTransform.position = _prevPosition + GetOffsetHeight() * Vector3.up;
		}
	}
}

public static class IncrementalPositionTweenHelperFunctions
{
	public static TweenHolder AddIncrementalPositionTween(this ITweenable tweenable, Vector3 to, bool boostSpeed = false)
	{
		return tweenable.Holder.AddTween(new IncrementalPositionTween(to, boostSpeed)).Play();
	}
}