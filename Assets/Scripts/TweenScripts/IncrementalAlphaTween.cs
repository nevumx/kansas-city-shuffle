using UnityEngine;
using UnityEngine.UI;
using System;
using Nx;

public class IncrementalAlphaTween : Tween
{
	private	Graphic[]	TargetGraphics;
	public	float		AlphaTo			= 1.0f;

	private IncrementalAlphaTween() {}

	public IncrementalAlphaTween(Graphic[] targetGraphics, float to)
	{
		TargetGraphics = targetGraphics;
		AlphaTo = to;
	}

	public override Action<Transform, float, float> GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate(Transform gameObjTransform, float percentDone, float timeRemaining)
	{
		if (Mathf.Approximately(timeRemaining, 0.0f))
		{
			Color finalColor = TargetGraphics[0].color;
			finalColor.a = AlphaTo;
			TargetGraphics.ForEach(t => t.color = finalColor);
			return;
		}

		float alphaRemaining = AlphaTo - TargetGraphics[0].color.a;
		float speed = alphaRemaining / timeRemaining;
		Color nextColor = TargetGraphics[0].color;
		nextColor.a += speed * Time.deltaTime;
		TargetGraphics.ForEach(t => t.color = nextColor);
	}
}

public static class IncrementalAlphaTweenHelperFunctions
{
	public static TweenHolder AddIncrementalAlphaTween(this TweenableGraphics tweenableGraphic, float to)
	{
		return tweenableGraphic.TweenHolder.AddTween(new IncrementalAlphaTween(tweenableGraphic.Graphics, to)).Play();
	}
}