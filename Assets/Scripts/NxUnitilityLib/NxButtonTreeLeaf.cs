using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace Nx
{
	public class NxButtonTreeLeaf : MonoBehaviour, ITweenable, IPointerEnterHandler, IPointerExitHandler
	{
							public	RectTransform		RectTransform		{ get { return _tweenableGraphics.RootRectTransform; } }

		[SerializeField]	private	NxButtonTreeRoot	_parentButtonTreeRoot;

		[SerializeField]	private	TweenableGraphics	_tweenableGraphics;
							public	TweenableGraphics	TweenableGraphics	{ get { return _tweenableGraphics; } }

							public	TweenHolder			TweenHolder			{ get { return _tweenableGraphics.TweenHolder; } }

		[SerializeField]	private	RectTransform		_tweenToPosition;
							public	RectTransform		TweenToPosition		{ get { return _tweenToPosition; } }

		[SerializeField]	private	CircleCollider2D	_buttonCollider;
							public	CircleCollider2D	ButtonCollider		{ get { return _buttonCollider; } }

		[SerializeField]	private	UnityEvent			_onClicked;

		private void Start()
		{
			_buttonCollider.radius = Mathf.Min(RectTransform.rect.width, RectTransform.rect.height) / 2.0f;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			_parentButtonTreeRoot.OnPointerEnteredLeaf(eventData.pointerId, this);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_parentButtonTreeRoot.OnPointerExitedLeaf(eventData.pointerId, this);
		}

		public void FireOnClickedEvent(PointerEventData eventData)
		{
			_onClicked.Invoke();
		}
	}
}