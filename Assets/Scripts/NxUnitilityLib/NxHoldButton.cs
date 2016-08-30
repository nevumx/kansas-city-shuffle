using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace Nx
{
	public class NxHoldButton : NxCornerButton, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]		private		CircleCollider2D	_buttonCollider;
		[SerializeField]		private		TweenableImages		_radialFillImages;
		[SerializeField]
		[Range(0.0f, 1.0f)]		private		float				_radialFillAmount		= 0.5f;
		[SerializeField]
		[Range(0.1f, 10.0f)]	private		float				_holdDuration			= 1.0f;
		[SerializeField]		private		UnityEvent			_onHeld;
								protected	UnityEvent			_OnHeld					{ get { return _onHeld; } }

								private		int					_currentPointerId		= NxSimpleButton.NO_BUTTON_ID;

		protected override void Start()
		{
			base.Start();
			_buttonCollider.radius = Mathf.Min(_RectTransform.rect.width, _RectTransform.rect.height) / 2.0f;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_currentPointerId == NxSimpleButton.NO_BUTTON_ID)
			{
				_currentPointerId = eventData.pointerId;

				TweenRadialImageIn();
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId == _currentPointerId)
			{
				_currentPointerId = NxSimpleButton.NO_BUTTON_ID;

				TweenRadialImageOut();
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (eventData.pointerId == _currentPointerId)
			{
				TweenRadialImageIn();
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (eventData.pointerId == _currentPointerId)
			{
				TweenRadialImageOut();
			}
		}

		private void TweenRadialImageIn()
		{
			_radialFillImages.AddIncrementalRadialFillTween(_radialFillAmount).TweenHolder
								 .SetDuration(_holdDuration)
								 .AddToOnFinishedOnce(() => _onHeld.Invoke());
		}

		private void TweenRadialImageOut()
		{
			_radialFillImages.AddIncrementalRadialFillTween(0.0f).TweenHolder
								 .SetDuration(_holdDuration)
								 .ClearOnFinishedOnce();
		}
	}
}