using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public struct TweenableGraphics : ITweenable
{
	[SerializeField]	private	TweenHolder		_tweenHolder;
						public	TweenHolder		Holder				{ get { return _tweenHolder; } }

	[SerializeField]	private	Graphic[]		_graphics;
						public	Graphic[]		Graphics			{ get { return _graphics; } }

	[SerializeField]	private	RectTransform	_rootRectTransform;
						public	RectTransform	RootRectTransform	{ get { return _rootRectTransform; } }
						public	GameObject		gameObject			{ get { return _rootRectTransform.gameObject; } }
}

[Serializable]
public struct AlphaMultipliedGraphic
{
	[SerializeField]	private	Graphic	_graphic;
						public	Graphic	Graphic			{ get { return _graphic; } }

	[SerializeField]
	[Range(0.0f, 1.0f)]	private	float	_alphaMultiplier;
						public	float	AlphaMultiplier	{ get { return _alphaMultiplier; } }

	public AlphaMultipliedGraphic(Graphic graphic)
	{
		_graphic = graphic;
		_alphaMultiplier = 1.0f;
	}

	public static implicit operator AlphaMultipliedGraphic(Graphic toCopyFrom)
	{
		return new AlphaMultipliedGraphic(toCopyFrom);
	}
}

[Serializable]
public struct TweenableAlphaMultipliedGraphics : ITweenable
{
	[SerializeField]	private	TweenHolder					_tweenHolder;
						public	TweenHolder					Holder				{ get { return _tweenHolder; } }

	[SerializeField]	private	AlphaMultipliedGraphic[]	_graphics;
						public	AlphaMultipliedGraphic[]	Graphics			{ get { return _graphics; } }

	[SerializeField]	private	RectTransform				_rootRectTransform;
						public	RectTransform				RootRectTransform	{ get { return _rootRectTransform; } }
						public	GameObject					gameObject			{ get { return _rootRectTransform.gameObject; } }
}

[Serializable]
public struct TweenableImages
{
	[SerializeField]	private	TweenHolder	_tweenHolder;
						public	TweenHolder	TweenHolder	{ get { return _tweenHolder; } }

	[SerializeField]	private	Image[]		_images;
						public	Image[]		Images		{ get { return _images; } }
}
