using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0044 // Add readonly modifier

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

							protected					bool				_CurrentPointerIsInside	{ get; private set; }

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
				_CurrentPointerIsInside = true;
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId == _currentPointerId)
			{
				if (_CurrentPointerIsInside)
				{
					_onClicked.Invoke();
				}

				ResetCurrentPointerVariables();
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			_CurrentPointerIsInside |= eventData.pointerId == _currentPointerId;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_CurrentPointerIsInside &= eventData.pointerId != _currentPointerId;
		}

		protected void ResetCurrentPointerVariables()
		{
			_currentPointerId = NO_BUTTON_ID;
			_CurrentPointerIsInside = false;
		}
	}
}

#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE1006 // Naming Styles