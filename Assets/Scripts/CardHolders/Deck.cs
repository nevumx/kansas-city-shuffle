using UnityEngine;
using System;
using System.Collections;
using Nx;

public class Deck : CardPile
{
	public void DealTo(CardHolder holder, out TweenHolder outTween, bool? visibleDuringTween)
	{
		MoveCard(ReadOnlyCards.LastIndex(), holder, out outTween, visibleDuringTween);
	}

	public void RefillWith(CardHolder otherHolder, int numCardsToLeave, Action onFinished)
	{
		StartCoroutine(RefillWithInternal(otherHolder, numCardsToLeave, onFinished));
	}

	private IEnumerator RefillWithInternal(CardHolder otherHolder, int numCardsToLeave, Action onFinished)
	{
		if (numCardsToLeave < otherHolder.ReadOnlyCards.Count)
		{
			var deckRefillTweenWaiter = new FinishableGroupWaiter(onFinished);
			while (otherHolder.ReadOnlyCards.Count > numCardsToLeave)
			{
				TweenHolder refillTween;
				otherHolder.MoveCard(0, this, out refillTween, true);
				deckRefillTweenWaiter.AddFinishable(refillTween);
				refillTween.SetDuration(_CardAnimationData.DeckFillDurationPerCard);
				yield return new WaitForSeconds(_CardAnimationData.DeckRefillDelayPerCard);
			}
			deckRefillTweenWaiter.Ready = true;
		}
		else
		{
			onFinished();
		}
	}

	public void Refill(CardHolder otherHolder, int numCardsToFill, Action onFinished)
	{
		StartCoroutine(RefillInternal(otherHolder, numCardsToFill, onFinished));
	}

	private IEnumerator RefillInternal(CardHolder otherHolder, int numCardsToFill, Action onFinished)
	{
		var deckRefillTweenWaiter = new FinishableGroupWaiter(onFinished);
		for (int i = 0; i < numCardsToFill; ++i)
		{
			TweenHolder refillTween;
			MoveCard(ReadOnlyCards.LastIndex(), otherHolder, out refillTween, true, 0);
			deckRefillTweenWaiter.AddFinishable(refillTween);
			refillTween.SetDuration(_CardAnimationData.DeckFillDurationPerCard);
			yield return new WaitForSeconds(_CardAnimationData.DeckRefillDelayPerCard);
		}
		deckRefillTweenWaiter.Ready = true;
	}
}