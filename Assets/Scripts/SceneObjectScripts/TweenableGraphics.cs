using UnityEngine;
using UnityEngine.UI;

public class TweenableGraphics : MonoBehaviour
{
	[SerializeField]	public	TweenHolder		_tweenHolder;
						public	TweenHolder		TweenHolder			{ get { return _tweenHolder; } }

	[SerializeField]	public	Graphic[]		_graphics;
						public	Graphic[]		Graphics			{ get { return _graphics; } }

	[SerializeField]	public	RectTransform	_rootRectTransform;
						public	RectTransform	RootRectTransform	{ get { return _rootRectTransform; } }
}
