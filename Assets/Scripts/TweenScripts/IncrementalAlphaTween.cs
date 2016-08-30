using UnityEngine;
using UnityEngine.UI;
using System;
using Nx;

public class IncrementalAlphaTween : Tween
{
	private	Graphic[]	_targetGraphics;
	public	float		AlphaTo			= 1.0f;

	private IncrementalAlphaTween() {}

	public IncrementalAlphaTween(Graphic[] targetGraphics, float to)
	{
		_targetGraphics = targetGraphics;
		AlphaTo = to;
	}

	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		if (Mathf.Approximately(TweenHolder.TimeRemaining, 0.0f))
		{
			_targetGraphics.ForEach(t =>
			{
				Color finalColor = t.color;
				finalColor.a = AlphaTo;
				t.color = finalColor;
			});
			return;
		}

		float alphaRemaining = AlphaTo - _targetGraphics[0].color.a;
		float speed = alphaRemaining / TweenHolder.TimeRemaining;
		_targetGraphics.ForEach(t =>
		{
			Color nextColor = t.color;
			nextColor.a += speed * TweenHolder.DeltaTime;
			t.color = nextColor;
		});
	}
}

public static class IncrementalAlphaTweenHelperFunctions
{
	public static TweenableGraphics AddIncrementalAlphaTween(this TweenableGraphics tweenableGraphics, float to)
	{
		tweenableGraphics.TweenHolder.AddTween(new IncrementalAlphaTween(tweenableGraphics.Graphics, to)).Play();
		return tweenableGraphics;
	}
}