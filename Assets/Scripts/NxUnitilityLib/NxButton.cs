using UnityEngine;
using System;

namespace Nx
{
	public class NxButton : MonoBehaviour
	{
		[SerializeField]
		private KeyCode[] _hotKeys;

		private Action _onClicked;

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

		public void OnClick()
		{
			_onClicked.Raise();
		}

		public void ClearOnClickedDelegates()
		{
			_onClicked = null;
		}

#if UNITY_STANDALONE
		private void Update()
		{
			if (_hotKeys.Exists(k => Input.GetKeyDown(k)))
			{
				OnClick();
			}
		}
#endif
	}
}