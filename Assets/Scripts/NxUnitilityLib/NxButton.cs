using UnityEngine;
using System;

namespace Nx
{
	public class NxButton : MonoBehaviour
	{
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
	}
}