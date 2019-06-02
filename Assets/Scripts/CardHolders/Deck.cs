using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Nx;

public class Deck : CardPile
{
	[SerializeField]	private	TweenHolder _shuffleAnimationCamera;
						public	TweenHolder ShuffleAnimationCamera { get { return _shuffleAnimationCamera; } }
	[SerializeField]	private	Transform _shuffleAnimationOriginPoint;
	[SerializeField]	private	AudioClip _cardShuffleClip;
	[SerializeField]	private	AudioClip _cardRuffleClip;

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

	public void Shuffle(out int[] unShuffleData, Action onFinished)
	{
		var cardsToShuffleFrom = new List<CardController>(_Cards);
		var cardsToShuffleTo = new List<CardController>(_Cards.Count);
		unShuffleData = new int[_Cards.Count];
		var shuffleAnimationWaiter = new FinishableGroupWaiter(onFinished);
		for (int i = 0; cardsToShuffleFrom.Count > 0; ++i)
		{
			int index = Mathf.RoundToInt(UnityEngine.Random.value * (cardsToShuffleFrom.Count - 1));
			cardsToShuffleTo.Add(cardsToShuffleFrom[index]);
			cardsToShuffleFrom.RemoveAt(index);
			unShuffleData[i] = _Cards.IndexOf(cardsToShuffleTo.Last());

			CardController lastCard = cardsToShuffleTo.Last();

			lastCard.ViewFSM.SetTextVisibility(true);

			AnimateShuffle(lastCard, i);

			shuffleAnimationWaiter.AddFinishable(lastCard.Holder);
		}

		if (ShuffleAnimationCamera != null && _shuffleAnimationOriginPoint != null)
		{
			shuffleAnimationWaiter.AddFinishable(
					_shuffleAnimationCamera.AddPositionPingPongTween(_shuffleAnimationCamera.transform.position + Vector3.up * _CardAnimationData.CameraShuffleTweenUpAmount)
										   .SetDuration(_CardAnimationData.DeckShuffleExplosionDuration));
		}

		_Cards.Clear();
		_Cards.AddRange(cardsToShuffleTo);
		shuffleAnimationWaiter.Ready = true;

		if (CardFlipAudio != null)
		{
			CardFlipAudio.PlayOneShot(_cardShuffleClip);
			StartCoroutine(PlayCardRuffleClip());
		}
	}

	public void UnShuffle(int[] unShuffleData, Action onFinished)
	{
#if NX_DEBUG
		if (unShuffleData.Length != _Cards.Count)
		{
			NxUtils.LogError("Trying to UnShuffle with invalid unSuffleData length");
		}
#endif
		var shuffleAnimationWaiter = new FinishableGroupWaiter(onFinished);
		var unShuffledCards = new CardController[unShuffleData.Length];
		for (int i = 0, iMax = unShuffleData.Length; i < iMax; ++i)
		{
			CardController lastCard = unShuffledCards[unShuffleData[i]] = _Cards[i];

			lastCard.ViewFSM.SetTextVisibility(true);

			AnimateShuffle(lastCard, i);

			shuffleAnimationWaiter.AddFinishable(lastCard.Holder);
		}

		if (_shuffleAnimationCamera != null && _shuffleAnimationOriginPoint != null)
		{
			shuffleAnimationWaiter.AddFinishable(
					_shuffleAnimationCamera.AddPositionPingPongTween(_shuffleAnimationCamera.transform.position + Vector3.up * _CardAnimationData.CameraShuffleTweenUpAmount)
										   .SetDuration(_CardAnimationData.DeckShuffleExplosionDuration));
		}

		_Cards.Clear();
		_Cards.AddRange(unShuffledCards);
		shuffleAnimationWaiter.Ready = true;

		if (CardFlipAudio != null)
		{
			CardFlipAudio.PlayOneShot(_cardShuffleClip);
			StartCoroutine(PlayCardRuffleClip());
		}
	}

	public void ReShuffle(int[] unShuffleData, Action onFinished)
	{
#if NX_DEBUG
		if (unShuffleData.Length != _Cards.Count)
		{
			NxUtils.LogError("Trying to ReShuffle with invalid unSuffleData length");
		}
#endif
		var shuffleAnimationWaiter = new FinishableGroupWaiter(onFinished);
		var reShuffledCards = new CardController[unShuffleData.Length];
		for (int i = 0, iMax = unShuffleData.Length; i < iMax; ++i)
		{
			CardController lastCard = reShuffledCards[i] = _Cards[unShuffleData[i]];

			lastCard.ViewFSM.SetTextVisibility(true);

			AnimateShuffle(lastCard, i);

			shuffleAnimationWaiter.AddFinishable(lastCard.Holder);
		}

		if (_shuffleAnimationCamera != null && _shuffleAnimationOriginPoint != null)
		{
			shuffleAnimationWaiter.AddFinishable(
					_shuffleAnimationCamera.AddPositionPingPongTween(_shuffleAnimationCamera.transform.position + Vector3.up * _CardAnimationData.CameraShuffleTweenUpAmount)
										   .SetDuration(_CardAnimationData.DeckShuffleExplosionDuration));
		}

		_Cards.Clear();
		_Cards.AddRange(reShuffledCards);
		shuffleAnimationWaiter.Ready = true;

		if (CardFlipAudio != null)
		{
			CardFlipAudio.PlayOneShot(_cardShuffleClip);
			StartCoroutine(PlayCardRuffleClip());
		}
	}

	private IEnumerator PlayCardRuffleClip()
	{
		yield return new WaitForSeconds(Mathf.Max(0.0f, _CardAnimationData.DeckShuffleExplosionDuration - _cardRuffleClip.length));
		CardFlipAudio.PlayOneShot(_cardRuffleClip);
	}

	private void AnimateShuffle(CardController card, int cardIndex)
	{
		float animationDuration = UnityEngine.Random.Range(2.0f, _CardAnimationData.DeckShuffleExplosionDuration / 2.0f);
		Vector3 rotationVector = (Mathf.Round(UnityEngine.Random.Range(1.0f, _CardAnimationData.DeckShuffleExplosionMaxRotations / 2.0f)) * 2.0f + 1.0f) * 360.0f * Vector3.one;
		rotationVector.z -= card.ViewFSM.GetAnimRotationOffset().z;
		card.AddPositionPingPongTween(card.gameObject.transform.position + Vector3.up * _CardAnimationData.DeckShuffleExplosionSphereRadius
					+ UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(0.1f, Mathf.Max(1.0f, _CardAnimationData.DeckShuffleExplosionSphereRadius)),
				GetCardPositionAtIndex(cardIndex))
			.SetDuration(animationDuration)
			.SetDelay(UnityEngine.Random.Range(0.0f, _CardAnimationData.DeckShuffleExplosionDuration - animationDuration))
			.AddLocalRotationTween(rotationVector, true)
			.SetShouldChangeLayer(true)
			.AddToOnFinishedOnce(() => OnCardRecieveTweenFinished(card));
	}
}