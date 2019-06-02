using UnityEngine;
using UnityEngine.UI;
using System;
using Nx;

public class IncrementalRadialFillTween : Tween
{
	private	Image[]		_targetImages;
	public	float		FillTo			= 1.0f;

	public IncrementalRadialFillTween() {}

	public IncrementalRadialFillTween(Image[] targetImages, float to)
	{
		_targetImages = targetImages;
		FillTo = to;
	}

	public override void OnUpdate()
	{
		if (Mathf.Approximately(TweenHolder.TimeRemaining, 0.0f))
		{
			_targetImages.ForEach(t => t.fillAmount = FillTo);
			return;
		}

		float fillRemaining = FillTo - _targetImages[0].fillAmount;
		float speed = fillRemaining / TweenHolder.TimeRemaining;
		_targetImages.ForEach(t => t.fillAmount += speed * TweenHolder.DeltaTime);
	}
}

public static class IncrementalRadialFillTweenHelperFunctions
{
	public static TweenableImages AddIncrementalRadialFillTween(this TweenableImages tweenableImages, float to)
	{
		tweenableImages.TweenHolder.AddTween(new IncrementalRadialFillTween(tweenableImages.Images, to)).Play();
		return tweenableImages;
	}
}