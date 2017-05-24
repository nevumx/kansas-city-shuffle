using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;

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

							private						float						_timeLastClickBegan			= 0.0f;
							private						float						_timeLastClicked			= 0.0f;
							private						bool						_isBeingDragged				= false;
							public						bool						IsBeingDragged				{ get { return _isBeingDragged; } }

		public void AddToOnClicked(Action toAdd)
		{
#if NX_DEBUG
			if (_onClicked != null && _onClicked.GetInvocationList().Exists(d => d.Method == toAdd.Method && d.Target == toAdd.Target))
			{
				NxUtils.LogWarning("Warning: Adding two identical delegates to a NxButton");
			}
#endif
			_onClicked += toAdd;
			_collider.enabled = true;
		}

		public void AddToOnDoubleClicked(Action toAdd)
		{
#if NX_DEBUG
			if (_onDoubleClicked != null && _onDoubleClicked.GetInvocationList().Exists(d => d.Method == toAdd.Method && d.Target == toAdd.Target))
			{
				NxUtils.LogWarning("Warning: Adding two identical delegates to a NxButton");
			}
#endif
			_onDoubleClicked += toAdd;
			_collider.enabled = true;
		}

		public void AddToOnClickedHard(Action toAdd)
		{
#if NX_DEBUG
			if (_onClickedHard != null && _onClickedHard.GetInvocationList().Exists(d => d.Method == toAdd.Method && d.Target == toAdd.Target))
			{
				NxUtils.LogWarning("Warning: Adding two identical delegates to a NxButton");
			}
#endif
			_onClickedHard += toAdd;
			_collider.enabled = true;

		}

		public void AddToOnBeginDrag(Action toAdd)
		{
#if NX_DEBUG
			if (_onBeginDrag != null && _onBeginDrag.GetInvocationList().Exists(d => d.Method == toAdd.Method && d.Target == toAdd.Target))
			{
				NxUtils.LogWarning("Warning: Adding two identical delegates to a NxButton");
			}
#endif
			_onBeginDrag += toAdd;
			_collider.enabled = true;
		}

		public void AddToOnDrag(Action<PointerEventData> toAdd)
		{
#if NX_DEBUG
			if (_onDrag != null && _onDrag.GetInvocationList().Exists(d => d.Method == toAdd.Method && d.Target == toAdd.Target))
			{
				NxUtils.LogWarning("Warning: Adding two identical delegates to a NxButton");
			}
#endif
			_onDrag += toAdd;
			_collider.enabled = true;
		}

		public void AddToOnDrop(Action<PointerEventData> toAdd)
		{
#if NX_DEBUG
			if (_onDrop != null && _onDrop.GetInvocationList().Exists(d => d.Method == toAdd.Method && d.Target == toAdd.Target))
			{
				NxUtils.LogWarning("Warning: Adding two identical delegates to a NxButton");
			}
#endif
			_onDrop += toAdd;
			_collider.enabled = true;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (!_isBeingDragged && _cachedEventData == null)
			{
				_timeLastClickBegan = Time.unscaledTime;
				_cachedEventData = eventData;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!_isBeingDragged && eventData.pointerId == _cachedEventData.pointerId)
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
			if (!_isBeingDragged && eventData.pointerId == _cachedEventData.pointerId)
			{
				_isBeingDragged = true;
				_onBeginDrag.Raise();
				_cachedEventData = eventData;
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (_isBeingDragged && eventData.pointerId == _cachedEventData.pointerId)
			{
				_cachedEventData = eventData;
			}
		}

		private void Update()
		{
			if (_isBeingDragged)
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
			if (_isBeingDragged && eventData.pointerId == _cachedEventData.pointerId)
			{
				_isBeingDragged = false;
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
			_isBeingDragged = false;
			_cachedEventData = null;
		}
	}
}