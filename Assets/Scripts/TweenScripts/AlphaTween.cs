using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using Nx;

public class AlphaTween : Tween
{
	private	Graphic[]	_targetGraphics;
	public	float		AlphaFrom		= 1.0f;
	public	float		AlphaTo			= 1.0f;

	private AlphaTween() {}

	public AlphaTween(Graphic[] targetGraphics, float from, float to)
	{
		_targetGraphics = targetGraphics;
		AlphaFrom = from;
		AlphaTo = to;
	}

	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		_targetGraphics.ForEach(t =>
		{
			Color nextColor = t.color;
			nextColor.a = Mathf.Lerp(AlphaFrom, AlphaTo, TweenHolder.PercentDone);
			t.color = nextColor;
			Text tText = t as Text;
			var alphaAsByte = (byte)Mathf.RoundToInt(nextColor.a * 255.0f);
			tText.IfIsNotNullThen(() => tText.text = Regex.Replace(tText.text, "([0-9]|[a-f]){2}>", BitConverter.ToString(new byte[] {alphaAsByte}).ToLower() + ">"));
		});
	}
}

public static class AlphaTweenHelperFunctions
{
	public static TweenableGraphics AddAlphaTween(this TweenableGraphics tweenableGraphics, float to)
	{
		tweenableGraphics.TweenHolder.AddTween(new AlphaTween(tweenableGraphics.Graphics, tweenableGraphics.Graphics[0].color.a, to)).Play();
		return tweenableGraphics;
	}
	public static TweenableGraphics AddAlphaTween(this TweenableGraphics tweenableGraphics, float from, float to)
	{
		tweenableGraphics.TweenHolder.AddTween(new AlphaTween(tweenableGraphics.Graphics, from, to)).Play();
		return tweenableGraphics;
	}
	public static TweenableImages AddAlphaTween(this TweenableImages tweenableGraphics, float to)
	{
		tweenableGraphics.TweenHolder.AddTween(new AlphaTween(tweenableGraphics.Images, tweenableGraphics.Images[0].color.a, to)).Play();
		return tweenableGraphics;
	}
	public static TweenableImages AddAlphaTween(this TweenableImages tweenableGraphics, float from, float to)
	{
		tweenableGraphics.TweenHolder.AddTween(new AlphaTween(tweenableGraphics.Images, from, to)).Play();
		return tweenableGraphics;
	}
}