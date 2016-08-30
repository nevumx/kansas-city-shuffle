using UnityEngine;
using System;
using Nx;

public class CardModViewtroller : MonoBehaviour, ITweenable
{
	[SerializeField]	private	static	readonly	float					QUALITY_REDUCTION_SHRINK_FACTOR		= 0.95f;

	[SerializeField]	private						CardAnimationData		_cardAnimationData;
						public						CardAnimationData		CardAnimationData	{ get { return _cardAnimationData; } }

	[SerializeField]	private						TextMesh				_cardValueText;
	[SerializeField]	private						TextMesh				_cardSuitText;
	[SerializeField]	private						GameObject				_shadowObject;
	[SerializeField]	private						MeshMaterialSwapInfo[]	_qualityLoweringSwapInfos;
	[SerializeField]	private						Transform[]				_transformsToShrinkForQualityReduction;

	[NonSerialized]		public						CardHolder				ParentCardHolder	= null;

	[SerializeField]	private						NxDynamicButton			_button;
						private						CardViewFSM				_viewFSM;
						public						NxDynamicButton			Button				{ get { return _button; } }
						public						CardViewFSM				ViewFSM				{ get { return _viewFSM; } }

						private						Card					_card;
						public						Card					Card				{ get { return _card; } }
						public						int 					CardValue			{ get { return (int) _card.Value; } }

	[SerializeField]	private						TweenHolder				_tweenHolder;
						public						TweenHolder				TweenHolder			{ get { return _tweenHolder; } }

	public CardModViewtroller Init(Card.CardValue value, Card.CardSuit suit, Camera eventCamera)
	{
		_card = new Card(value, suit);
		_cardValueText.text = _card.CardValueString;
		_cardSuitText.text = _card.CardSuitString;
		_cardValueText.color = _cardSuitText.color = suit == Card.CardSuit.SPADES || suit == Card.CardSuit.CLUBS ? Color.black : Color.red;
		return this;
	}

	public void DestroyShadowObject()
	{
		Destroy(_shadowObject);
	}

	public void ReduceQuality()
	{
		_qualityLoweringSwapInfos.ForEach(q => q.MeshRenderer.material = q.SwapMaterial);
		_transformsToShrinkForQualityReduction.ForEach(o => o.IfIsNotNullThen(t => t.localScale *= QUALITY_REDUCTION_SHRINK_FACTOR));
	}

	private void Awake()
	{
		_viewFSM = new CardViewFSM(this, CardViewFSM.AnimState.VISIBLE);
	}

	public class CardViewFSM
	{
		public enum AnimState : byte
		{
			OBSCURED,
			VISIBLE,
			ABLE_TO_BE_SELECTED,
			SELECTED,
		}

		private CardModViewtroller _parentModViewtroller;

		private AnimState _state;
		public AnimState State { get { return _state; } }

		private static readonly Vector3 TURNED_OVER_ROTATION_OFFSET = new Vector3(0.0f, 0.0f, 180.0f);
		private static readonly Vector3 SELECTED_SCALE = Vector3.one * 1.5f;

		private CardViewFSM() {}
		public CardViewFSM(CardModViewtroller parentModViewtroller, AnimState initialState)
		{
			_parentModViewtroller = parentModViewtroller;
			SetAnimState(initialState, performTweens: false);
		}

		public Vector3 GetAnimPositionOffset()
		{
			return _state == AnimState.ABLE_TO_BE_SELECTED || _state == AnimState.SELECTED
				? Vector3.up * _parentModViewtroller._cardAnimationData.CardFloatingHeight : Vector3.zero;
		}

		public Vector3 GetAnimRotationOffset()
		{
			return _state == AnimState.OBSCURED ? TURNED_OVER_ROTATION_OFFSET : Vector3.zero;
		}

		public Vector3 GetAnimScale()
		{
			return _state == AnimState.SELECTED ? SELECTED_SCALE : Vector3.one;
		}

		public void SetTextVisibility(bool visible)
		{
			_parentModViewtroller._cardValueText.gameObject.SetActive(visible);
			_parentModViewtroller._cardSuitText.gameObject.SetActive(visible);
		}

		public TweenHolder SetAnimState(AnimState newState, bool performTweens = true)
		{
			TweenHolder returnTween = _parentModViewtroller.TweenHolder;
			if (_state != newState)
			{
				AnimState lastState = _state;
				_state = newState;
				if (performTweens)
				{
					switch (_state)
					{
					case AnimState.OBSCURED:
						ObscuredState(lastState, ref returnTween);
						break;
					case AnimState.VISIBLE:
						VisibleState(lastState, ref returnTween);
						break;
					case AnimState.ABLE_TO_BE_SELECTED:
						AbleToBeSelectedState(lastState, ref returnTween);
						break;
					case AnimState.SELECTED:
						SelectedState(lastState, ref returnTween);
						break;
					default:
						NxUtils.LogWarning("Invalid or Unimplemented CardViewState");
						break;
					}
					return returnTween;
				}
			}
			return null;
		}

		private void ObscuredState(AnimState lastState, ref TweenHolder returnTween)
		{
			returnTween.AddLocalRotationTween(TURNED_OVER_ROTATION_OFFSET)
				.SetDuration(_parentModViewtroller._cardAnimationData.CardStateChangeDuration);
			Vector3 finalPosition = _parentModViewtroller.ParentCardHolder.GetFinalPositionOfCard(_parentModViewtroller);
			switch (lastState)
			{
				case AnimState.SELECTED:
					returnTween.AddIncrementalScaleTween(Vector3.one);
					goto case AnimState.ABLE_TO_BE_SELECTED;
				case AnimState.ABLE_TO_BE_SELECTED:
					returnTween.RemoveTweenOfType<PositionPingPongTween>().AddIncrementalPositionTween(finalPosition);
					break;
				case AnimState.VISIBLE:
				default:
					if (returnTween.GetTweenOfType<PositionPingPongTween>() != null)
					{
						goto case AnimState.ABLE_TO_BE_SELECTED;
					}
					returnTween.AddPositionPingPongTween(finalPosition
						+ Vector3.up * _parentModViewtroller._cardAnimationData.CardFloatingHeight, finalPosition);
					break;
			}
		}

		private void VisibleState(AnimState lastState, ref TweenHolder returnTween)
		{
			Vector3 finalPosition = _parentModViewtroller.ParentCardHolder.GetFinalPositionOfCard(_parentModViewtroller);
			switch (lastState)
			{
				case AnimState.SELECTED:
					returnTween.AddIncrementalScaleTween(Vector3.one);
					goto case AnimState.ABLE_TO_BE_SELECTED;
				case AnimState.ABLE_TO_BE_SELECTED:
					returnTween.RemoveTweenOfType<PositionPingPongTween>().AddIncrementalPositionTween(finalPosition);
					break;
				case AnimState.OBSCURED:
				default:
					if (returnTween.GetTweenOfType<PositionPingPongTween>() != null)
					{
						goto case AnimState.ABLE_TO_BE_SELECTED;
					}
					returnTween.AddPositionPingPongTween(finalPosition
								+ Vector3.up * _parentModViewtroller._cardAnimationData.CardFloatingHeight, finalPosition)
						.AddLocalRotationTween(Vector3.zero);
					break;
			}
			returnTween.SetDuration(_parentModViewtroller._cardAnimationData.CardStateChangeDuration);
		}

		private void AbleToBeSelectedState(AnimState lastState, ref TweenHolder returnTween)
		{
			switch (lastState)
			{
				case AnimState.SELECTED:
					returnTween.AddIncrementalScaleTween(Vector3.one);
					break;
				case AnimState.OBSCURED:
					returnTween.AddLocalRotationTween(Vector3.zero);
					goto case AnimState.VISIBLE;
				case AnimState.VISIBLE:
				default:
					returnTween.AddIncrementalPositionTween(_parentModViewtroller.ParentCardHolder.GetFinalPositionOfCard(_parentModViewtroller));
					break;
			}
			returnTween.SetDuration(_parentModViewtroller._cardAnimationData.CardStateChangeDuration);
		}

		private void SelectedState(AnimState lastState, ref TweenHolder returnTween)
		{
			switch (lastState)
			{
				case AnimState.OBSCURED:
					returnTween.AddLocalRotationTween(Vector3.zero);
					goto case AnimState.VISIBLE;
				case AnimState.VISIBLE:
					returnTween.AddIncrementalPositionTween(_parentModViewtroller.ParentCardHolder.GetFinalPositionOfCard(_parentModViewtroller));
					goto case AnimState.ABLE_TO_BE_SELECTED;
				case AnimState.ABLE_TO_BE_SELECTED:
				default:
					returnTween.AddIncrementalScaleTween(SELECTED_SCALE)
						.SetDuration(_parentModViewtroller._cardAnimationData.CardStateChangeDuration);
				break;
			}
		}
	}
}