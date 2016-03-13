using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Nx
{
	public class NxDynamicButton : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IDragHandler, IEndDragHandler
	{
		private	readonly	float						DOUBLE_CLICK_THRESHOLD_TIME	= 0.5f;
		private	readonly	float						CLICK_EXPIRE_THRESHOLD_TIME	= 1.0f;

		private				Action						_onClicked;
		private				Action						_onDoubleClicked;
		private				Action<PointerEventData>	_onDragged;
		private				Action						_onDropped;

		private				float						_timeLastClickBegan			= 0.0f;
		private				float						_timeLastClicked			= 0.0f;
		private				bool						_isBeingDragged				= false;

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

		public void AddToOnDragged(Action<PointerEventData> toAdd)
		{
#if NX_DEBUG
			if (_onDragged != null && _onDragged.GetInvocationList().Exists(d => d.Method == toAdd.Method && d.Target == toAdd.Target))
			{
				NxUtils.LogWarning("Warning: Adding two identical delegates to a NxButton");
			}
#endif
			_onDragged += toAdd;
		}

		public void AddToOnDropped(Action toAdd)
		{
#if NX_DEBUG
			if (_onDropped != null && _onDropped.GetInvocationList().Exists(d => d.Method == toAdd.Method && d.Target == toAdd.Target))
			{
				NxUtils.LogWarning("Warning: Adding two identical delegates to a NxButton");
			}
#endif
			_onDropped += toAdd;
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
					NxUtils.Log("Click");
				}
				else
				{
					_onDoubleClicked.Raise();
					_timeLastClicked = 0.0f;
					NxUtils.Log("DoubleClick");
				}
			}
		}

		public void OnButtonClick()
		{
			if (_timeLastClicked == 0.0f || Time.realtimeSinceStartup - _timeLastClicked > DOUBLE_CLICK_THRESHOLD_TIME)
			{
				_onClicked.Raise();
				_timeLastClicked = Time.realtimeSinceStartup;
				NxUtils.Log("ButtonClick");
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			_isBeingDragged = true;
			_onDragged.Raise(eventData);
			NxUtils.Log("Drag");
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_isBeingDragged = false;
			_onDropped.Raise();
			NxUtils.Log("Drop");
		}

		public void ClearOnClickedDelegates()
		{
			_onClicked = null;
		}

		public void ClearOnDoubleClickedDelegates()
		{
			_onDoubleClicked = null;
		}

		public void ClearOnDraggedDelegates()
		{
			_onDragged = null;
		}

		public void ClearOnDroppedDelegates()
		{
			_onDropped = null;
		}

		public void ClearAllDelegates()
		{
			ClearOnClickedDelegates();
			ClearOnDoubleClickedDelegates();
			ClearOnDraggedDelegates();
			ClearOnDroppedDelegates();
		}
	}
}