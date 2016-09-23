using UnityEngine;
using UnityEngine.UI;
using System;

namespace Nx
{
	public class NxButtonTreeRoot : NxSimpleButton
	{
							public	static	readonly	float				BUTTON_LEAF_ANIM_TIME	= 0.2f;

		[SerializeField]	private						NxButtonTreeLeaf[]	_buttonLeaves;

		protected override void Start()
		{
			base.Start();
			_buttonLeaves.ForEach(b =>
			{
				Vector2 relativeTweenToPosition = b.TweenToPosition.localPosition - _RectTransform.localPosition;
				b.ButtonConnectorBar.RootRectTransform.localEulerAngles
					= Vector3.forward * Mathf.Rad2Deg * Mathf.Atan2(relativeTweenToPosition.y, relativeTweenToPosition.x);
				b.ButtonConnectorBar.RootRectTransform.sizeDelta = new Vector2(relativeTweenToPosition.magnitude, 0.0f);
			});
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			_buttonLeaves.ForEach(b =>
			{
				b.TweenHolder.Finish();
				b.ButtonCollider.enabled = false;
				Action<Graphic> resetAlpha = g =>
				{
					Color buttonColor = g.color;
					buttonColor.a = 0.0f;
					g.color = buttonColor;
				};
				b.TweenableGraphics.Graphics.ForEach(resetAlpha);
				b.ButtonConnectorBar.Graphics.ForEach(resetAlpha);
				b.RectTransform.anchoredPosition = Vector2.zero;
			});
		}

		public void ExpandButtonTreeLeaves()
		{
			_buttonLeaves.ForEach(b =>
			{
				b.TweenableGraphics.AddIncrementalAnchoredPositionTween(b.TweenToPosition.localPosition - _RectTransform.localPosition)
								   .AddAlphaTween(1.0f).TweenHolder
								   .SetDuration(BUTTON_LEAF_ANIM_TIME)
								   .AddToOnFinishedOnce(() =>
								{
									b.ButtonCollider.enabled = true;
									b.ButtonConnectorBar.AddAlphaTween(1.0f).TweenHolder
														.SetDuration(BUTTON_LEAF_ANIM_TIME);
								});
			});
		}
	}
}