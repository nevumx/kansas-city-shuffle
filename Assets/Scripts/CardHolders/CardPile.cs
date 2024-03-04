using UnityEngine;
using System.Collections.Generic;
using Nx;

#pragma warning disable IDE0044 // Add readonly modifier

public class CardPile : CardHolder
{
	[SerializeField]	private	float	_pileMaxHeightInUnits			= 0.4f;
	[SerializeField]	private	float	_maxDistBetweenCardsInUnits		= 0.05f;
	[SerializeField]	private	bool	_keepAllButTopCardTextInvisible;

	protected override void RepositionCards()
	{
		int cardCount = ReadOnlyCards.Count;
		float distBetweenCards = cardCount == 1 ? 0.0f : Mathf.Min(_maxDistBetweenCardsInUnits, _pileMaxHeightInUnits / (cardCount - 1));
		CardController bestCardToMimic = _CardsInTransition.Best((a, b) => a.Holder.TimeRemaining < b.Holder.TimeRemaining);
		for (int i = 0; i < cardCount; ++i)
		{
			TweenHolder cardShiftTween = ReadOnlyCards[i].Holder;
			Vector3 targetPosition = transform.position + distBetweenCards * i * Vector3.up;

			IncrementalPositionTween posTweenToShift;
			if ((posTweenToShift = cardShiftTween.GetTweenOfType<IncrementalPositionTween>()) == null) // Careful (=)
			{
				ReadOnlyCards[i].AddIncrementalPositionTween(targetPosition)
								.SetShouldChangeLayer(false);
			}
			else
			{
				posTweenToShift.PositionTo = targetPosition;
			}

			if (!ReadOnlyCards[i].Holder.enabled)
			{
				if (bestCardToMimic != null)
				{
					if (Mathf.Approximately(ReadOnlyCards[i].Holder.Duration, ReadOnlyCards[i].Holder.TimeRemaining))
					{
						ReadOnlyCards[i].Holder.SetDuration(bestCardToMimic.Holder.TimeRemaining);
					}
					else
					{
						ReadOnlyCards[i].Holder.SetDuration(bestCardToMimic.Holder.Duration);
					}
				}
				else
				{
					ReadOnlyCards[i].Holder.SetDuration(_CardAnimationData.GeneralCardMoveDuration);
				}
			}
		}
	}

	protected override void OnCardSent(CardController sentCard)
	{
		base.OnCardSent(sentCard);
		UpdateCardVisibilities();
	}

	protected override void OnCardRecieveTweenFinished(CardController card)
	{
		base.OnCardRecieveTweenFinished(card);
		UpdateCardVisibilities();
	}

	private void UpdateCardVisibilities()
	{
		if (_keepAllButTopCardTextInvisible)
		{
			var pileCards = new List<CardController>(ReadOnlyCards);
			pileCards.RemoveAll(c => _CardsInTransition.Contains(c));
			pileCards.ForEach(c => c.ViewFSM.SetTextVisibility(false));
			pileCards.Last().IfIsNotNullThen(c => c.ViewFSM.SetTextVisibility(true));
		}
	}

	protected override Vector3 GetCardPositionAtIndex(int index)
	{
		int cardCount = ReadOnlyCards.Count;
		float distBetweenCards = cardCount == 1 ? 0.0f : Mathf.Min(_maxDistBetweenCardsInUnits, _pileMaxHeightInUnits / (cardCount - 1));
		return transform.position + distBetweenCards * index * transform.up;
	}
}

#pragma warning restore IDE0044 // Add readonly modifier