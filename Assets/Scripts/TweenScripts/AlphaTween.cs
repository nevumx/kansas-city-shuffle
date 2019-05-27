using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using Nx;

public class AlphaTween : Tween
{
	private	AlphaMultipliedGraphic[]	_targetGraphics;
	public	float						AlphaFrom		= 1.0f;
	public	float						AlphaTo			= 1.0f;

	private AlphaTween() {}

	public AlphaTween(AlphaMultipliedGraphic[] targetGraphics, float from, float to)
	{
		_targetGraphics = targetGraphics;
		AlphaFrom = from;
		AlphaTo = to;
	}

	public AlphaTween(Graphic[] targetGraphics, float from, float to)
	{
		_targetGraphics = new AlphaMultipliedGraphic[targetGraphics.Length];
		for (int i = 0, iMax = _targetGraphics.Length; i < iMax; ++i)
		{
			_targetGraphics[i] = targetGraphics[i];
		}
		AlphaFrom = from;
		AlphaTo = to;
	}

	public override Action GetUpdateDelegate() { return OnUpdate; }

	private void OnUpdate()
	{
		_targetGraphics.ForEach(t =>
		{
			t.Graphic.SetAlpha(Mathf.Lerp(AlphaFrom, AlphaTo, TweenHolder.PercentDone) * t.AlphaMultiplier);
			Text tText = t.Graphic as Text;
			var alphaAsByte = (byte)Mathf.RoundToInt(t.Graphic.color.a * 255.0f);
			tText.IfIsNotNullThen(() => tText.text = Regex.Replace(tText.text, "([0-9]|[a-f]){2}>", BitConverter.ToString(new byte[] { alphaAsByte }).ToLower() + ">"));
		});
	}
}

public static class AlphaTweenHelperFunctions
{
	public static TweenableGraphics AddAlphaTween(this TweenableGraphics tweenableGraphics, float to)
	{
		tweenableGraphics.Holder.AddTween(new AlphaTween(tweenableGraphics.Graphics, tweenableGraphics.Graphics[0].color.a, to)).Play();
		return tweenableGraphics;
	}
	public static TweenableGraphics AddAlphaTween(this TweenableGraphics tweenableGraphics, float from, float to)
	{
		tweenableGraphics.Holder.AddTween(new AlphaTween(tweenableGraphics.Graphics, from, to)).Play();
		return tweenableGraphics;
	}
	public static TweenableAlphaMultipliedGraphics AddAlphaTween(this TweenableAlphaMultipliedGraphics tweenableGraphics, float to)
	{
		tweenableGraphics.Holder.AddTween(new AlphaTween(tweenableGraphics.Graphics, tweenableGraphics.Graphics[0].Graphic.color.a / tweenableGraphics.Graphics[0].AlphaMultiplier, to)).Play();
		return tweenableGraphics;
	}
	public static TweenableAlphaMultipliedGraphics AddAlphaTween(this TweenableAlphaMultipliedGraphics tweenableGraphics, float from, float to)
	{
		tweenableGraphics.Holder.AddTween(new AlphaTween(tweenableGraphics.Graphics, from, to)).Play();
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