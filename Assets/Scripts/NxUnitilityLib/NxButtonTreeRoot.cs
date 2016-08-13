using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace Nx
{
	[Serializable]
	public class PointerTriggerEvent : UnityEvent<PointerEventData> {}

	public class NxButtonTreeRoot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
							private	static	readonly	float				BUTTON_LEAF_ANIM_TIME				= 0.2f;
							private	static	readonly	int					NO_BUTTON_ID						= -2;

		[SerializeField]	private						RectTransform		_rectTransform;
		[SerializeField]	private						NxButtonTreeLeaf[]	_buttonLeaves;
							private						NxButtonTreeLeaf	_buttonLeafPointerIsCurrentlyInside	= null;
		[SerializeField]	private						CircleCollider2D	_buttonCollider;
							private						int					_currentPointerId					= NO_BUTTON_ID;
							private						bool				_currentPointerIsInside				= false;
		[SerializeField]	private						PointerTriggerEvent	_onClicked;

		private void Start()
		{
			_buttonCollider.radius = Mathf.Min(_rectTransform.rect.width, _rectTransform.rect.height) / 2.0f;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_currentPointerId == NO_BUTTON_ID)
			{
				_buttonLeaves.ForEach(b =>
				{
					b.TweenableGraphics.AddIncrementalAnchoredPositionTween(b.TweenToPosition.localPosition - _rectTransform.localPosition)
									   .AddIncrementalAlphaTween(1.0f).TweenHolder
									   .SetDuration(BUTTON_LEAF_ANIM_TIME)
									   .AddToOnFinishedOnce(() => b.ButtonCollider.enabled = true);
				});

				_currentPointerId = eventData.pointerId;
				_currentPointerIsInside = true;
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId == _currentPointerId)
			{
				_buttonLeaves.ForEach(b =>
				{
					b.ButtonCollider.enabled = false;
					b.TweenableGraphics.AddIncrementalAnchoredPositionTween(Vector2.zero)
									   .AddIncrementalAlphaTween(0.0f).TweenHolder
									   .SetDuration(BUTTON_LEAF_ANIM_TIME)
									   .ClearOnFinishedOnce();

				});

				if (_currentPointerIsInside)
				{
					ResetAllButtonTreeButtons();
					_onClicked.Invoke(eventData);
				}
				else if (_buttonLeafPointerIsCurrentlyInside != null)
				{
					NxButtonTreeLeaf buttonLeafBeingClicked = _buttonLeafPointerIsCurrentlyInside;
					ResetAllButtonTreeButtons();
					buttonLeafBeingClicked.FireOnClickedEvent(eventData);
				}

				_currentPointerId = NO_BUTTON_ID;
				_currentPointerIsInside = false;
				_buttonLeafPointerIsCurrentlyInside = null;
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (eventData.pointerId == _currentPointerId)
			{
				_currentPointerIsInside = true;
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (eventData.pointerId == _currentPointerId)
			{
				_currentPointerIsInside = false;
			}
		}

		public void OnPointerEnteredLeaf(int pointerId, NxButtonTreeLeaf leaf)
		{
			if (_currentPointerId == pointerId && _buttonLeaves.Exists(b => object.ReferenceEquals(b, leaf)))
			{
				_buttonLeafPointerIsCurrentlyInside = leaf;
			}
		}

		public void OnPointerExitedLeaf(int pointerId, NxButtonTreeLeaf leaf)
		{
			if (_currentPointerId == pointerId && _buttonLeaves.Exists(b => object.ReferenceEquals(b, leaf)))
			{
				_buttonLeafPointerIsCurrentlyInside = null;
			}
		}

		private void ResetAllButtonTreeButtons()
		{
			NxButtonTreeRoot[] buttonTreeRoots = FindObjectsOfType<NxButtonTreeRoot>();
			buttonTreeRoots.ForEach(b => b.ResetLeafButtons());
		}

		private void ResetLeafButtons()
		{
			_buttonLeaves.ForEach(b =>
			{
				b.TweenHolder.Finish();
				b.ButtonCollider.enabled = false;
				b.TweenableGraphics.Graphics.ForEach(g =>
				{
					Color buttonColor = g.color;
					buttonColor.a = 0.0f;
					g.color = buttonColor;
				});
				b.RectTransform.anchoredPosition = Vector2.zero;
			});
			_buttonLeafPointerIsCurrentlyInside = null;
			_currentPointerId = NO_BUTTON_ID;
			_currentPointerIsInside = false;
		}
	}
}