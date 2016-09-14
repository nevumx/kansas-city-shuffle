using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace Nx
{
	public class NxHoldButton : NxCornerButton, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
								private	static	readonly	float				HOLD_TEXT_FADE_DURATION	= 1.0f;

		[SerializeField]		private						CircleCollider2D	_buttonCollider;
		[SerializeField]		private						TweenableImages		_radialFillImages;
		[SerializeField]		private						TweenableGraphics	_holdText;
		[SerializeField]
		[Range(0.0f, 1.0f)]		private						float				_radialFillAmount		= 0.5f;
		[SerializeField]
		[Range(0.1f, 10.0f)]	private						float				_holdDuration			= 1.0f;
		[SerializeField]		private						UnityEvent			_onHeld;
								protected					UnityEvent			_OnHeld					{ get { return _onHeld; } }

								private						int					_currentPointerId		= NxSimpleButton.NO_BUTTON_ID;

		protected override void Start()
		{
			base.Start();
			_buttonCollider.radius = Mathf.Min(_RectTransform.rect.width, _RectTransform.rect.height) / 2.0f;
		}

		private void OnDisable()
		{
			_radialFillImages.TweenHolder.ClearOnFinishedOnce().Finish();
			_radialFillImages.Images.ForEach(i => i.fillAmount = 0.0f);
			if (_currentPointerId != NxSimpleButton.NO_BUTTON_ID)
			{
				Release();
			}
		}

		private void OnApplicationPause(bool isPaused)
		{
			if (isPaused && _currentPointerId != NxSimpleButton.NO_BUTTON_ID)
			{
				Release();
			}
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
				Release();
			}
		}

		private void Release()
		{
			_currentPointerId = NxSimpleButton.NO_BUTTON_ID;

			if (_radialFillImages.TweenHolder.enabled)
			{
				TweenRadialImageOut();
			}
			else
			{
				_radialFillImages.Images.ForEach(i => i.fillAmount = 0.0f);
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
			_holdText.AddIncrementalAlphaTween(1.0f).TweenHolder
			.SetDuration(HOLD_TEXT_FADE_DURATION).AddToOnFinishedOnce(() =>
					_holdText.AddIncrementalAlphaTween(0.0f).TweenHolder
							 .SetDuration(HOLD_TEXT_FADE_DURATION));
		}
	}
}