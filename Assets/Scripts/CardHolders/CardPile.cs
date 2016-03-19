using UnityEngine;
using System.Collections.Generic;
using Nx;

public class CardPile : DynamicCardHolder
{
	[SerializeField]	private	float	_pileMaxHeightInUnits			= 0.4f;
	[SerializeField]	private	float	_maxDistBetweenCardsInUnits		= 0.05f;
	[SerializeField]	private	bool	_keepAllButTopCardTextInvisible	= false;

	protected override void RepositionCards(CardModViewtroller inOrOutCard)
	{
		int cardCount = ReadOnlyCards.Count;
		float distBetweenCards = cardCount == 1 ? 0.0f : Mathf.Min(_maxDistBetweenCardsInUnits, _pileMaxHeightInUnits / (cardCount - 1));
		CardModViewtroller bestCardToMimic = _CardsInTransition.Best((a, b) => a.TweenHolder.TimeRemaining < b.TweenHolder.TimeRemaining);
		for (int i = 0; i < cardCount; ++i)
		{
			TweenHolder cardShiftTween = ReadOnlyCards[i].TweenHolder;
			Vector3 targetPosition = transform.position + Vector3.up * distBetweenCards * i;

			IncrementalPositionTween posTweenToShift;
			if ((posTweenToShift = cardShiftTween.GetTweenOfType<IncrementalPositionTween>()) == null) // Careful (=)
			{
				ReadOnlyCards[i].AddIncrementalPositionTween(targetPosition);
			}
			else
			{
				posTweenToShift.PositionTo = targetPosition;
			}

			if (!ReadOnlyCards[i].TweenHolder.enabled)
			{
				if (bestCardToMimic != null)
				{
					if (Mathf.Approximately(ReadOnlyCards[i].TweenHolder.Duration, ReadOnlyCards[i].TweenHolder.TimeRemaining))
					{
						ReadOnlyCards[i].TweenHolder.SetDuration(bestCardToMimic.TweenHolder.TimeRemaining);
					}
					else
					{
						ReadOnlyCards[i].TweenHolder.SetDuration(bestCardToMimic.TweenHolder.Duration);
					}
				}
				else
				{
					ReadOnlyCards[i].TweenHolder.SetDuration(CardAnimationData.GeneralCardMoveDuration);
				}
			}
		}
	}

	protected override void OnCardSent(CardModViewtroller sentCard)
	{
		base.OnCardSent(sentCard);
		if (_keepAllButTopCardTextInvisible)
		{
			ReadOnlyCards.ForEach(r => r.ViewFSM.SetTextVisibility(false));
			ReadOnlyCards.Last().IfIsNotNullThen(c => c.ViewFSM.SetTextVisibility(true));
		}
	}

	protected override void OnCardRecieveTweenFinished(CardModViewtroller card)
	{
		if (_keepAllButTopCardTextInvisible)
		{
			ReadOnlyCards.ForEach(r =>
			{
				if (!_CardsInTransition.Contains(r))
				{
					r.ViewFSM.SetTextVisibility(false);
				}
			});
			card.ViewFSM.SetTextVisibility(true);
		}
		else
		{
			base.OnCardRecieveTweenFinished(card);
		}
	}

	protected override Vector3 GetCardPositionAtIndex(int index)
	{
		int cardCount = ReadOnlyCards.Count;
		float distBetweenCards = cardCount == 1 ? 0.0f : Mathf.Min(_maxDistBetweenCardsInUnits, _pileMaxHeightInUnits / (cardCount - 1));
		return transform.position + transform.up * distBetweenCards * index;
	}
}