using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Nx
{
	public class NxButtonTreeRoot : NxSimpleButton
	{
							private	static	readonly	float				BUTTON_LEAF_ANIM_TIME				= 0.2f;

		[SerializeField]	private						NxButtonTreeLeaf[]	_buttonLeaves;
							private						NxButtonTreeLeaf	_buttonLeafPointerIsCurrentlyInside	= null;

		public override void OnPointerDown(PointerEventData eventData)
		{
			if (_CurrentPointerId == NO_BUTTON_ID)
			{
				_buttonLeaves.ForEach(b =>
				{
					b.TweenableGraphics.AddIncrementalAnchoredPositionTween(b.TweenToPosition.localPosition - _RectTransform.localPosition)
									   .AddIncrementalAlphaTween(1.0f).TweenHolder
									   .SetDuration(BUTTON_LEAF_ANIM_TIME)
									   .AddToOnFinishedOnce(() => b.ButtonCollider.enabled = true);
				});

				_CurrentPointerId = eventData.pointerId;
				_CurrentPointerIsInside = true;
			}
		}

		public override void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId == _CurrentPointerId)
			{
				_buttonLeaves.ForEach(b =>
				{
					b.ButtonCollider.enabled = false;
					b.TweenableGraphics.AddIncrementalAnchoredPositionTween(Vector2.zero)
									   .AddIncrementalAlphaTween(0.0f).TweenHolder
									   .SetDuration(BUTTON_LEAF_ANIM_TIME)
									   .ClearOnFinishedOnce();
				});

				if (_CurrentPointerIsInside)
				{
					ResetAllButtonTreeButtons();
					_OnClicked.Invoke(eventData);
				}
				else if (_buttonLeafPointerIsCurrentlyInside != null)
				{
					NxButtonTreeLeaf buttonLeafBeingClicked = _buttonLeafPointerIsCurrentlyInside;
					ResetAllButtonTreeButtons();
					buttonLeafBeingClicked.FireOnClickedEvent(eventData);
				}

				_CurrentPointerId = NO_BUTTON_ID;
				_CurrentPointerIsInside = false;
				_buttonLeafPointerIsCurrentlyInside = null;
			}
		}

		public void OnPointerEnteredLeaf(int pointerId, NxButtonTreeLeaf leaf)
		{
			if (_CurrentPointerId == pointerId && _buttonLeaves.Exists(b => object.ReferenceEquals(b, leaf)))
			{
				_buttonLeafPointerIsCurrentlyInside = leaf;
			}
		}

		public void OnPointerExitedLeaf(int pointerId, NxButtonTreeLeaf leaf)
		{
			if (_CurrentPointerId == pointerId && _buttonLeaves.Exists(b => object.ReferenceEquals(b, leaf)))
			{
				_buttonLeafPointerIsCurrentlyInside = null;
			}
		}

		private void ResetAllButtonTreeButtons()
		{
			NxButtonTreeRoot[] buttonTreeRoots = FindObjectsOfType<NxButtonTreeRoot>();
			buttonTreeRoots.ForEach(b => b.ResetLeafButtons());
		}

		private void ResetLeafButtons()
		{
			_buttonLeaves.ForEach(b =>
			{
				b.TweenHolder.Finish();
				b.ButtonCollider.enabled = false;
				b.TweenableGraphics.Graphics.ForEach(g =>
				{
					Color buttonColor = g.color;
					buttonColor.a = 0.0f;
					g.color = buttonColor;
				});
				b.RectTransform.anchoredPosition = Vector2.zero;
			});
			_buttonLeafPointerIsCurrentlyInside = null;
			_CurrentPointerId = NO_BUTTON_ID;
			_CurrentPointerIsInside = false;
		}
	}
}