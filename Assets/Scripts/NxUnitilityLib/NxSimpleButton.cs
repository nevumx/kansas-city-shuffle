using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace Nx
{
	public class NxSimpleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
							public	static	readonly	int					NO_BUTTON_ID			= -2;

		[SerializeField]	private						RectTransform		_rectTransform;
							protected					RectTransform		_RectTransform			{ get { return _rectTransform; } }
		[SerializeField]	private						Collider2D			_buttonCollider;
							protected					Collider2D			_ButtonCollider			{ get { return _buttonCollider; } }
		[SerializeField]	private						UnityEvent			_onClicked;
							public						UnityEvent			OnClicked				{ get { return _onClicked; } }

							private						int					_currentPointerId		= NO_BUTTON_ID;
							protected					int					_CurrentPointerId		{ get { return _currentPointerId; } }
							private						bool				_currentPointerIsInside	= false;
							protected					bool				_CurrentPointerIsInside	{ get { return _currentPointerIsInside; } }

		protected virtual void Start()
		{
			(_buttonCollider as CircleCollider2D).IfIsNotNullThen(b => b.radius = Mathf.Min(_rectTransform.rect.width, _rectTransform.rect.height) / 2.0f);
			(_buttonCollider as BoxCollider2D).IfIsNotNullThen(b => b.size = _rectTransform.rect.size);
		}

		protected virtual void OnDisable()
		{
			ResetCurrentPointerVariables();
		}

		private void OnApplicationPause(bool isPaused)
		{
			if (isPaused)
			{
				ResetCurrentPointerVariables();
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_currentPointerId == NO_BUTTON_ID)
			{
				_currentPointerId = eventData.pointerId;
				_currentPointerIsInside = true;
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId == _currentPointerId)
			{
				if (_currentPointerIsInside)
				{
					_onClicked.Invoke();
				}

				ResetCurrentPointerVariables();
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

		protected void ResetCurrentPointerVariables()
		{
			_currentPointerId = NO_BUTTON_ID;
			_currentPointerIsInside = false;
		}
	}
}