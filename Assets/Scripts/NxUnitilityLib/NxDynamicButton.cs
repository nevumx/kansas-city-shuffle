using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Nx
{
	public class NxDynamicButton : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
							private	static	readonly	float						DOUBLE_CLICK_THRESHOLD_TIME	= 0.5f;
							private	static	readonly	float						CLICK_EXPIRE_THRESHOLD_TIME	= 1.0f;

							private						Action						_onClicked;
							private						Action						_onDoubleClicked;
							private						Action						_onBeginDrag;
							private						Action<PointerEventData>	_onDrag;
							private						Action<PointerEventData>	_onDrop;
		[SerializeField]	private						Collider					_collider;

							private						PointerEventData			_cachedDragEventData;

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
			_timeLastClickBegan = Time.unscaledTime;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!_isBeingDragged && Time.unscaledTime - _timeLastClickBegan < CLICK_EXPIRE_THRESHOLD_TIME)
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
		}

		public void OnButtonClick()
		{
			if (_timeLastClicked == 0.0f || Time.unscaledTime - _timeLastClicked > DOUBLE_CLICK_THRESHOLD_TIME)
			{
				_onClicked.Raise();
				_timeLastClicked = Time.unscaledTime;
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			_isBeingDragged = true;
			_onBeginDrag.Raise();
			_cachedDragEventData = eventData;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (_isBeingDragged)
			{
				_cachedDragEventData = eventData;
			}
		}

		private void Update()
		{
			if (_isBeingDragged)
			{
				_onDrag.Raise(_cachedDragEventData);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (_isBeingDragged)
			{
				_isBeingDragged = false;
				_onDrop.Raise(eventData);
			}
		}

		public void ClearAllDelegates()
		{
			_onClicked = null;
			_onDoubleClicked = null;
			_onBeginDrag = null;
			_onDrag = null;
			_onDrop = null;
			_collider.enabled = false;
		}

		public void CancelDrag()
		{
			_isBeingDragged = false;
		}
	}
}