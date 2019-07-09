using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator

namespace Nx
{
	public class NxHoldButton : NxCornerButton, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
								private	static	readonly	float				HOLD_TEXT_FADE_DURATION	= 1.0f;

		[SerializeField]		private						Collider2D			_buttonCollider;
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
			(_buttonCollider as CircleCollider2D).IfIsNotNullThen(b => b.radius = Mathf.Min(_RectTransform.rect.width, _RectTransform.rect.height) / 2.0f);
			(_buttonCollider as BoxCollider2D).IfIsNotNullThen(b => b.size = _RectTransform.rect.size);
		}

		private void OnDisable()
		{
			Cancel();
		}

		private void OnApplicationPause(bool isPaused)
		{
			if (isPaused)
			{
				Cancel();
			}
		}

		private void Cancel()
		{
			_radialFillImages.TweenHolder.ClearOnFinishedOnce().Finish();
			_radialFillImages.Images.ForEach(i => i.fillAmount = 0.0f);
			_currentPointerId = NxSimpleButton.NO_BUTTON_ID;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_currentPointerId == NxSimpleButton.NO_BUTTON_ID)
			{
				_currentPointerId = eventData.pointerId;
				if (_radialFillImages.Images[0].fillAmount == 1.0f)
				{
					_radialFillImages.TweenHolder.Finish();
				}
				_radialFillImages.Images.ForEach(i =>
				{
					i.SetAlpha(1.0f);
					i.transform.localScale = Vector3.one;
					if (i.fillAmount == 1.0f)
					{
						i.fillAmount = 0.0f;
					}
				});
				TweenRadialImageIn();
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId == _currentPointerId)
			{
				_currentPointerId = NxSimpleButton.NO_BUTTON_ID;

				if (_radialFillImages.TweenHolder.enabled)
				{
					TweenRadialImageOut();
				}
				else
				{
					_onHeld.Invoke();
					_radialFillImages.AddAlphaTween(0.0f).TweenHolder
									 .AddIncrementalScaleTween(Vector3.one * 2.0f)
									 .SetDuration(0.5f);
				}
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
							 .SetDuration(_holdDuration);
		}

		private void TweenRadialImageOut()
		{
			_radialFillImages.AddIncrementalRadialFillTween(0.0f).TweenHolder
							 .SetDuration(_holdDuration)
							 .ClearOnFinishedOnce();
			_holdText.AddAlphaTween(1.0f).Holder
					 .SetDuration(HOLD_TEXT_FADE_DURATION)
					 .AddToOnFinishedOnce(() =>
							_holdText.AddAlphaTween(0.0f).Holder
									 .SetDuration(HOLD_TEXT_FADE_DURATION));
		}
	}
}

#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE1006 // Naming Styles