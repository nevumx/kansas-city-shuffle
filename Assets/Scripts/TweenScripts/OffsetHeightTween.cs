using UnityEngine;

public class OffsetHeightTween : IncrementalPositionTween
{
	public	float	Height;

	public OffsetHeightTween() {}

	public OffsetHeightTween(float height, Vector3 to, bool boostSpeed)
		: base(to, boostSpeed)
	{
		Height = height;
	}

	protected override float GetOffsetHeight()
	{
		return -4.0f * Height * TweenHolder.PercentDone * (TweenHolder.PercentDone - 1.0f);
	}
}

public static class OffsetHeightTweenHelperFunctions
{
	public static TweenHolder AddOffsetHeightTween(this ITweenable tweenable, float height, Vector3 to, bool boostSpeed = false)
	{
		return tweenable.Holder.AddTween(new OffsetHeightTween(height, to, boostSpeed)).Play();
	}
}