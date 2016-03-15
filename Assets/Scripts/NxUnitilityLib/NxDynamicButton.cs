using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Nx
{
	public class NxDynamicButton : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		private	readonly	float						DOUBLE_CLICK_THRESHOLD_TIME	= 0.5f;
		private	readonly	float						CLICK_EXPIRE_THRESHOLD_TIME	= 1.0f;

		private				Action						_onClicked;
		private				Action						_onDoubleClicked;
		private				Action						_onBeginDrag;
		private				Action<PointerEventData>	_onDrag;
		private				Action<PointerEventData>	_onDrop;

		private				float						_timeLastClickBegan			= 0.0f;
		private				float						_timeLastClicked			= 0.0f;
		private				bool						_isBeingDragged				= false;
		public				bool						IsBeingDragged				{ get { return _isBeingDragged; } }

		public void AddToOnClicked(Action toAdd)
		{
#if NX_DEBUG
			if (_onClicked != null && _onClicked.GetInvocationList().Exists(d => d.Method == toAdd.Method && d.Target == toAdd.Target))
			{
				NxUtils.LogWarning("Warning: Adding two identical delegates to a NxButton");
			}
#endif
			_onClicked += toAdd;
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
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			_timeLastClickBegan = Time.realtimeSinceStartup;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!_isBeingDragged && Time.realtimeSinceStartup - _timeLastClickBegan < CLICK_EXPIRE_THRESHOLD_TIME)
			{
				if (_timeLastClicked == 0.0f || Time.realtimeSinceStartup - _timeLastClicked > DOUBLE_CLICK_THRESHOLD_TIME)
				{
					_onClicked.Raise();
					_timeLastClicked = Time.realtimeSinceStartup;
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
			if (_timeLastClicked == 0.0f || Time.realtimeSinceStartup - _timeLastClicked > DOUBLE_CLICK_THRESHOLD_TIME)
			{
				_onClicked.Raise();
				_timeLastClicked = Time.realtimeSinceStartup;
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			_isBeingDragged = true;
			_onBeginDrag.Raise();
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (_isBeingDragged)
			{
				_onDrag.Raise(eventData);
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

		public void ClearOnClickedDelegates()
		{
			_onClicked = null;
		}

		public void ClearOnDoubleClickedDelegates()
		{
			_onDoubleClicked = null;
		}

		public void ClearOnBeginDragDelegates()
		{
			_onBeginDrag = null;
		}

		public void ClearOnDragDelegates()
		{
			_onDrag = null;
		}

		public void ClearOnDropDelegates()
		{
			_onDrop = null;
		}

		public void ClearAllDelegates()
		{
			ClearOnClickedDelegates();
			ClearOnDoubleClickedDelegates();
			ClearOnBeginDragDelegates();
			ClearOnDragDelegates();
			ClearOnDropDelegates();
		}

		public void CancelDrag()
		{
			_isBeingDragged = false;
		}
	}
}