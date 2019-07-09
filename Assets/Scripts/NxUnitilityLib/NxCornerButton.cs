using UnityEngine;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0044 // Add readonly modifier

namespace Nx
{
	public class NxCornerButton : MonoBehaviour
	{
		private enum ScreenCorner : byte
		{
			CENTER,
			UPPER_LEFT,
			UPPER_RIGHT,
			LOWER_LEFT,
			LOWER_RIGHT,
		}

		[SerializeField]	private		RectTransform	_rectTransform;
							protected	RectTransform	_RectTransform			{ get { return _rectTransform; } }
		[SerializeField]	private		ScreenCorner	_cornerToSnapCenterTo	= ScreenCorner.CENTER;

		protected virtual void Start()
		{
			Vector2 cornerOffset = _rectTransform.rect.size;
			switch (_cornerToSnapCenterTo)
			{
				case ScreenCorner.UPPER_LEFT:
					cornerOffset.Scale(new Vector2(-0.5f, 0.5f));
					break;
				case ScreenCorner.UPPER_RIGHT:
					cornerOffset.Scale(new Vector2(0.5f, 0.5f));
					break;
				case ScreenCorner.LOWER_LEFT:
					cornerOffset.Scale(new Vector2(-0.5f, -0.5f));
					break;
				case ScreenCorner.LOWER_RIGHT:
					cornerOffset.Scale(new Vector2(0.5f, -0.5f));
					break;
				default: // case ScreenCorner.CENTER:
					cornerOffset = Vector2.zero;
					break;
			}
			_rectTransform.anchoredPosition += cornerOffset;
		}
	}
}

#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE1006 // Naming Styles