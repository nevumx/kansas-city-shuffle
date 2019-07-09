using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator

namespace Nx
{
	public class NxDynamicButton : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
							private	static	readonly	float						DOUBLE_CLICK_THRESHOLD_TIME	= 0.5f;
							private	static	readonly	float						CLICK_EXPIRE_THRESHOLD_TIME	= 1.0f;

							private						Action						_onClicked;
							private						Action						_onDoubleClicked;
							private						Action						_onClickedHard;
							private						Action						_onBeginDrag;
							private						Action<PointerEventData>	_onDrag;
							private						Action<PointerEventData>	_onDrop;
		[SerializeField]	private						Collider					_collider;

							private						PointerEventData			_cachedEventData;

							private						float						_timeLastClickBegan;
							private						float						_timeLastClicked;

							public						bool						IsBeingDragged				{ get; private set; }

		public void AddToOnClicked(Action toAdd)
		{
			_onClicked += toAdd;
			_collider.enabled = true;
		}

		public void AddToOnDoubleClicked(Action toAdd)
		{
			_onDoubleClicked += toAdd;
			_collider.enabled = true;
		}

		public void AddToOnClickedHard(Action toAdd)
		{
			_onClickedHard += toAdd;
			_collider.enabled = true;

		}

		public void AddToOnBeginDrag(Action toAdd)
		{
			_onBeginDrag += toAdd;
			_collider.enabled = true;
		}

		public void AddToOnDrag(Action<PointerEventData> toAdd)
		{
			_onDrag += toAdd;
			_collider.enabled = true;
		}

		public void AddToOnDrop(Action<PointerEventData> toAdd)
		{
			_onDrop += toAdd;
			_collider.enabled = true;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (!IsBeingDragged && _cachedEventData == null)
			{
				_timeLastClickBegan = Time.unscaledTime;
				_cachedEventData = eventData;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!IsBeingDragged && eventData.pointerId == _cachedEventData.pointerId)
			{
				if (Time.unscaledTime - _timeLastClickBegan < CLICK_EXPIRE_THRESHOLD_TIME)
				{
					if (_timeLastClicked == 0.0f || Time.unscaledTime - _timeLastClicked > DOUBLE_CLICK_THRESHOLD_TIME)
					{
						_onClicked.Raise();
						_timeLastClicked = Time.unscaledTime;
					}
					else
					{
						_onDoubleClicked.Raise();
						_timeLastClicked = 0.0f;
					}
				}
				_cachedEventData = null;
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (!IsBeingDragged && eventData.pointerId == _cachedEventData.pointerId)
			{
				IsBeingDragged = true;
				_onBeginDrag.Raise();
				_cachedEventData = eventData;
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (IsBeingDragged && eventData.pointerId == _cachedEventData.pointerId)
			{
				_cachedEventData = eventData;
			}
		}

		private void Update()
		{
			if (IsBeingDragged)
			{
				_onDrag.Raise(_cachedEventData);
			}

			if (Input.touchPressureSupported && _cachedEventData != null)
			{
				Touch touch = Input.touches.First(t => t.fingerId == _cachedEventData.pointerId);
				if (touch.pressure >= touch.maximumPossiblePressure * 0.75f)
				{
					_onClickedHard.Raise();
				}
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (IsBeingDragged && eventData.pointerId == _cachedEventData.pointerId)
			{
				IsBeingDragged = false;
				_onDrop.Raise(eventData);
				_cachedEventData = null;
			}
		}

		public void ClearAllDelegates()
		{
			_onClicked = null;
			_onClickedHard = null;
			_onDoubleClicked = null;
			_onBeginDrag = null;
			_onDrag = null;
			_onDrop = null;
			_collider.enabled = false;
		}

		public void CancelDrag()
		{
			IsBeingDragged = false;
			_cachedEventData = null;
		}
	}
}

#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
#pragma warning restore IDE0044 // Add readonly modifier