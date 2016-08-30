using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public struct TweenableGraphics
{
	[SerializeField]	private	TweenHolder		_tweenHolder;
						public	TweenHolder		TweenHolder			{ get { return _tweenHolder; } }

	[SerializeField]	private	Graphic[]		_graphics;
						public	Graphic[]		Graphics			{ get { return _graphics; } }

	[SerializeField]	private	RectTransform	_rootRectTransform;
						public	RectTransform	RootRectTransform	{ get { return _rootRectTransform; } }
}

[Serializable]
public struct TweenableImages
{
	[SerializeField]	private	TweenHolder	_tweenHolder;
						public	TweenHolder	TweenHolder	{ get { return _tweenHolder; } }

	[SerializeField]	private	Image[]		_images;
						public	Image[]		Images		{ get { return _images; } }
}
