using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Nx
{
	public class NxKnobSlider : NxCornerButton, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
								private	static	readonly	float				PULL_TEXT_FADE_DURATION	= 1.0f;

		[SerializeField]		private						CircleCollider2D	_buttonCollider;
		[SerializeField]		private						FloatEvent			_onSlid;
		[SerializeField]		private						UnityEvent			_onReleased;
		[SerializeField]		private						Image				_radialFillImage;
		[SerializeField]		private						TweenableGraphics	_pullText;
		[SerializeField]		private						Graphic				_torqueBar;

		[SerializeField]
		[Range(0.0f, 360.0f)]	private						float				_knobStartAngle;
		[SerializeField]
		[Range(0.0f, 360.0f)]	private						float				_knobSpanAngle			= 360.0f;
		[SerializeField]		private						bool				_clockwise;

								private						int					_currentPointerId		= NxSimpleButton.NO_BUTTON_ID;

		protected override void Start()
		{
			base.Start();
			ResetButtonCollider();
		}

		private void OnApplicationPause(bool isPaused)
		{
			if (isPaused && _currentPointerId != NxSimpleButton.NO_BUTTON_ID)
			{
				Release(null);
			}
		}

		private void ResetButtonCollider()
		{
			_buttonCollider.radius = Mathf.Min(_RectTransform.rect.width, _RectTransform.rect.height) / 2.0f;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_currentPointerId == NxSimpleButton.NO_BUTTON_ID)
			{
				_currentPointerId = eventData.pointerId;
				_buttonCollider.radius = Screen.width + Screen.height;
				OnDrag(eventData);
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId == _currentPointerId)
			{
				Release(eventData);
			}
		}

		private void Release(PointerEventData eventData)
		{
			_currentPointerId = NxSimpleButton.NO_BUTTON_ID;
			ResetButtonCollider();
			_torqueBar.rectTransform.sizeDelta = Vector2.zero;

			float worldSpaceRadius = _buttonCollider.radius / Mathf.Min(Screen.width, Screen.height) * 2.0f;
			if (eventData != null && Vector2.Distance(eventData.pointerCurrentRaycast.worldPosition,
			 										  _RectTransform.position) <= worldSpaceRadius)
			{
				_pullText.AddAlphaTween(1.0f).TweenHolder
							 .SetDuration(PULL_TEXT_FADE_DURATION)
							 .AddToOnFinishedOnce(() =>
									_pullText.AddAlphaTween(0.0f).TweenHolder
											 .SetDuration(PULL_TEXT_FADE_DURATION));
			}

			_onReleased.Invoke();
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (eventData.pointerId == _currentPointerId)
			{
				float maxAngle = _knobStartAngle + _knobSpanAngle;
				Vector2 relativePointerPosition = eventData.pointerCurrentRaycast.worldPosition - _RectTransform.position;
				float pointerAngle = Mathf.Rad2Deg * Mathf.Atan2(relativePointerPosition.y * (_clockwise ? -1.0f : 1.0f), relativePointerPosition.x);
				if (pointerAngle < 0.0f)
				{
					pointerAngle += 360.0f;
				}
				if (maxAngle > 360.0f && pointerAngle < _knobStartAngle)
				{
					pointerAngle += 360.0f;
				}
				pointerAngle = Mathf.Clamp(pointerAngle, _knobStartAngle, maxAngle);
				_torqueBar.rectTransform.localEulerAngles = Vector3.forward * pointerAngle * (_clockwise ? -1.0f : 1.0f);
				_torqueBar.rectTransform.sizeDelta = new Vector2(Mathf.Max(Mathf.Min(_RectTransform.rect.width, _RectTransform.rect.height) * 0.75f,
						relativePointerPosition.magnitude * Screen.height / 2.0f), 0.0f);
				float percentFilled = Mathf.Approximately(_knobSpanAngle, 0.0f) ? 0.0f : (pointerAngle - _knobStartAngle) / _knobSpanAngle;
				_onSlid.Invoke(percentFilled);
			}
		}

		public void SetRadialFillPercentage(float percent)
		{
			float percentAngle = Mathf.Lerp(_knobStartAngle, _knobStartAngle + _knobSpanAngle, percent);
			if (percentAngle >= 360.0f)
			{
				percentAngle -= 360.0f;
			}
			_radialFillImage.fillAmount = percentAngle / 360.0f;
		}
	}
}