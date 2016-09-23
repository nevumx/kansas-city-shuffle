using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace Nx
{
	public class NxButtonTreeExpander : NxSimpleButton
	{
		[SerializeField]	TweenableGraphics	_graphicsToDimOnExpand;
		[SerializeField]	NxButtonTreeRoot[]	_buttonTreesToExpand;

		public void ExpandButtonTrees()
		{
			_ButtonCollider.enabled = false;
			_buttonTreesToExpand.ForEach(b => b.ExpandButtonTreeLeaves());
			_graphicsToDimOnExpand.AddAlphaTween(0.0f).TweenHolder
								  .SetDuration(NxButtonTreeRoot.BUTTON_LEAF_ANIM_TIME);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			_graphicsToDimOnExpand.TweenHolder.Finish();
			_ButtonCollider.enabled = true;
			_graphicsToDimOnExpand.Graphics.ForEach(g =>
			{
				Color buttonColor = g.color;
				buttonColor.a = 1.0f;
				g.color = buttonColor;
			});
		}
	}
}