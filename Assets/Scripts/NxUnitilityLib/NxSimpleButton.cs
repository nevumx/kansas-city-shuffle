using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace Nx
{
	public class NxSimpleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
							public	static	readonly	int					NO_BUTTON_ID			= -2;
							private	static	readonly	float				DIMMED_GRAPHIC_ALPHA	= 0.25f;

		[SerializeField]	private						RectTransform		_rectTransform;
							protected					RectTransform		_RectTransform			{ get { return _rectTransform; } }
		[SerializeField]	private						CircleCollider2D	_buttonCollider;
		[SerializeField]	protected					PointerTriggerEvent	_OnClicked;
		[SerializeField]	private						Graphic[]			_graphicsToDimOnDisable;

							protected					int					_CurrentPointerId		= NO_BUTTON_ID;
							protected					bool				_CurrentPointerIsInside	= false;

		private void Start()
		{
			_buttonCollider.radius = Mathf.Min(_rectTransform.rect.width, _rectTransform.rect.height) / 2.0f;
		}

		public virtual void OnPointerDown(PointerEventData eventData)
		{
			if (_CurrentPointerId == NO_BUTTON_ID)
			{
				_CurrentPointerId = eventData.pointerId;
				_CurrentPointerIsInside = true;
			}
		}

		public virtual void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId == _CurrentPointerId)
			{
				if (_CurrentPointerIsInside)
				{
					_OnClicked.Invoke(eventData);
				}

				_CurrentPointerId = NO_BUTTON_ID;
				_CurrentPointerIsInside = false;
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (eventData.pointerId == _CurrentPointerId)
			{
				_CurrentPointerIsInside = true;
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (eventData.pointerId == _CurrentPointerId)
			{
				_CurrentPointerIsInside = false;
			}
		}

		public void EnableInteraction()
		{
			_buttonCollider.enabled = true;
			_graphicsToDimOnDisable.ForEach(g => 
			{
				Color graphicColor = g.color;
				graphicColor.a = 1.0f;
				g.color = graphicColor;
			});
		}

		public void DisableInteraction()
		{
			_graphicsToDimOnDisable.ForEach(g => 
			{
				Color graphicColor = g.color;
				graphicColor.a = DIMMED_GRAPHIC_ALPHA;
				g.color = graphicColor;
			});
			_buttonCollider.enabled = false;
			_CurrentPointerId = NO_BUTTON_ID;
			_CurrentPointerIsInside = false;
		}
	}
}