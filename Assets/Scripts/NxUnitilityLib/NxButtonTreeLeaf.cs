using UnityEngine;

namespace Nx
{
	public class NxButtonTreeLeaf : NxSimpleButton
	{
							public	RectTransform		RectTransform		{ get { return _tweenableGraphics.RootRectTransform; } }

		[SerializeField]	private	TweenableGraphics	_tweenableGraphics;
							public	TweenableGraphics	TweenableGraphics	{ get { return _tweenableGraphics; } }

							public	TweenHolder			TweenHolder			{ get { return _tweenableGraphics.TweenHolder; } }

		[SerializeField]	private	RectTransform		_tweenToPosition;
							public	RectTransform		TweenToPosition		{ get { return _tweenToPosition; } }

							public	Collider2D			ButtonCollider		{ get { return _ButtonCollider; } }

		[SerializeField]	private	TweenableGraphics	_buttonConnectorBar;
							public	TweenableGraphics	ButtonConnectorBar	{ get { return _buttonConnectorBar; } }
	}
}