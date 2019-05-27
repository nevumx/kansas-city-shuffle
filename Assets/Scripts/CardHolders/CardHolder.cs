using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nx;

public abstract class CardHolder : MonoBehaviour
{

	[SerializeField]	private		CardAnimationData							_cardAnimationData;
						protected	CardAnimationData							_CardAnimationData				{ get { return _cardAnimationData; } }

						private		List<CardViewtroller>						_cards							= new List<CardViewtroller>();
						public		int											CardCount						{ get { return _Cards.Count; } }

						private		ReadOnlyCollection<CardViewtroller>			_cachedReadOnlyCards;
	[SerializeField]	private		CardViewtroller.CardViewFSM.AnimState		_cardsAnimState;
						public		CardViewtroller.CardViewFSM.AnimState		CardsAnimState					{ get { return _cardsAnimState; } }
	[SerializeField]	private		bool										_cardsTextVisibility;
						protected	bool										_CardsTextVisibility			{ get { return _cardsTextVisibility; } }
	[SerializeField]	private		TweenHolder									_shuffleAnimationCamera;
						public		TweenHolder									ShuffleAnimationCamera			{ get { return _shuffleAnimationCamera; } }
	[SerializeField]	private		Transform									_shuffleAnimationOriginPoint;
						private		LinkedList<CardViewtroller>					_cardsInTransition				= new LinkedList<CardViewtroller>();
						protected	LinkedList<CardViewtroller>					_CardsInTransition				{ get { return _cardsInTransition; } }

						public		AudioSource									CardFlipAudio;
	[SerializeField]	private		AudioClip									_cardShuffleClip;
	[SerializeField]	private		AudioClip									_cardRuffleClip;

	public ReadOnlyCollection<CardViewtroller> ReadOnlyCards
	{
		get
		{
			if (_cachedReadOnlyCards == null && _Cards != null)
			{
				_cachedReadOnlyCards = _Cards.AsReadOnly();
			}
			return _cachedReadOnlyCards;
		}
	}

	private List<CardViewtroller> _Cards
	{
		get
		{
			return _cards;
		}
		set
		{
			if ((_cards = value) != null) // Careful (=)
			{
				_cachedReadOnlyCards = _cards.AsReadOnly();
			}
			else
			{
				_cachedReadOnlyCards = null;
			}
		}
	}

	public bool CardsTextVisibility
	{
		get
		{
			return _cardsTextVisibility;
		}
		set
		{
			_cardsTextVisibility = value;
			_Cards.ForEach(c => c.ViewFSM.SetTextVisibility(value));
		}
	}

	public void SetCardsAnimStates(CardViewtroller.CardViewFSM.AnimState newState, Action onFinished)
	{
		_cardsAnimState = newState;
		var animGroupWaiter = new FinishableGroupWaiter(onFinished);
		for (int i = 0, iMax = _Cards.Count; i < iMax; ++i)
		{
			animGroupWaiter.AddFinishable(_Cards[i].ViewFSM.SetAnimState(newState));
		}
		animGroupWaiter.Ready = true;
	}

	public void SetIntendedIncomingCardAnimState(CardViewtroller.CardViewFSM.AnimState newState)
	{
		_cardsAnimState = newState;
	}

	public void IntroduceCard(CardViewtroller cardToIntroduce, out TweenHolder outTween,
						   bool fancyEntrance = false, float angleOffsetForFancyEntrance = 0.0f)
	{
		outTween = null;
		AddCard(cardToIntroduce);
		cardToIntroduce.ViewFSM.SetAnimState(_cardsAnimState, performTweens: false);

		if (fancyEntrance)
		{
			float cardCreationRadius = _cardAnimationData.DeckFillFancyIntroCardSpawnDistance;
			float rightDistance = cardCreationRadius * Mathf.Cos(angleOffsetForFancyEntrance);
			float forwardDistance = cardCreationRadius * Mathf.Sin(angleOffsetForFancyEntrance);

			cardToIntroduce.transform.position = transform.position
					+ Vector3.right * rightDistance
					+ Vector3.forward * forwardDistance;

			outTween = cardToIntroduce.AddIncrementalPositionTween(GetCardPositionAtIndex(_Cards.LastIndex()), true)
									  .AddOffsetHeightTween(_cardAnimationData.DeckFillFancyIntroTweenHeight)
									  .AddLocalRotationTween(360.0f * Vector3.one + cardToIntroduce.ViewFSM.GetAnimRotationOffset())
									  .SetDuration(_cardAnimationData.DeckFillDurationPerCard)
									  .SetShouldChangeLayer(true)
									  .AddToOnFinishedOnce(() => OnCardRecieveTweenFinished(cardToIntroduce));

			OnCardRecieveTweenBegan(cardToIntroduce);
		}
		else
		{
			cardToIntroduce.transform.ResetLocal();
		}

		if (CardFlipAudio != null)
		{
			CardFlipAudio.Play();
		}
	}

	public void MoveCard(int cardIndex, CardHolder other, out TweenHolder outTween, bool? visibleDuringTween, int indexToInsertAt = -1)
	{
		CardViewtroller cardBeingMoved = _Cards[cardIndex];
		_Cards.RemoveAt(cardIndex);
		if (!other._Cards.InsertionIndexIsValid(indexToInsertAt))
		{
			indexToInsertAt = other._Cards.Count;
		}
		other.AddCard(cardBeingMoved, indexToInsertAt);

		cardBeingMoved.ViewFSM.SetAnimState(other._cardsAnimState, performTweens: false);
		cardBeingMoved.ViewFSM.SetTextVisibility(visibleDuringTween.HasValue ? visibleDuringTween.Value : other._cardsTextVisibility);

		outTween = cardBeingMoved.AddIncrementalPositionTween(other.GetFinalPositionOfCardAtIndex(indexToInsertAt), boostSpeed: true)
								 .AddOffsetHeightTween(_cardAnimationData.GeneralCardMoveHeight)
								 .AddLocalRotationTween(Vector3.one * 360.0f + cardBeingMoved.ViewFSM.GetAnimRotationOffset())
								 .AddIncrementalScaleTween(cardBeingMoved.ViewFSM.GetAnimScale())
								 .SetDuration(_cardAnimationData.GeneralCardMoveDuration)
								 .SetShouldChangeLayer(true)
								 .AddToOnFinishedOnce(() => other.OnCardRecieveTweenFinished(cardBeingMoved));

		OnCardSent(cardBeingMoved);
		other.OnCardRecieveTweenBegan(cardBeingMoved);

		if (CardFlipAudio != null)
		{
			CardFlipAudio.Play();
		}
	}

	private void AddCard(CardViewtroller card, int indexToInsertAt = -1)
	{
		if (_Cards.InsertionIndexIsValid(indexToInsertAt))
		{
			_Cards.Insert(indexToInsertAt, card);
		}
		else
		{
			_Cards.Add(card);
		}

		card.ParentCardHolder = this;
		card.transform.parent = transform;
	}

	protected virtual Vector3 GetCardPositionAtIndex(int index)
	{
		return transform.position;
	}

	public void Shuffle(out int[] unShuffleData, Action onFinished)
	{
		var cardsToShuffleFrom = new List<CardViewtroller>(_Cards);
		var cardsToShuffleTo = new List<CardViewtroller>(_Cards.Count);
		unShuffleData = new int[_Cards.Count];
		var shuffleAnimationWaiter = new FinishableGroupWaiter(onFinished);
		for (int i = 0; cardsToShuffleFrom.Count > 0; ++i)
		{
			int index = Mathf.RoundToInt(UnityEngine.Random.value * (cardsToShuffleFrom.Count - 1));
			cardsToShuffleTo.Add(cardsToShuffleFrom[index]);
			cardsToShuffleFrom.RemoveAt(index);
			unShuffleData[i] = _Cards.IndexOf(cardsToShuffleTo.Last());

			CardViewtroller lastCard = cardsToShuffleTo.Last();

			lastCard.ViewFSM.SetTextVisibility(true);

			AnimateShuffle(lastCard, i);

			shuffleAnimationWaiter.AddFinishable(lastCard.Holder);
		}

		if (_shuffleAnimationCamera != null && _shuffleAnimationOriginPoint != null)
		{
			shuffleAnimationWaiter.AddFinishable(
					_shuffleAnimationCamera.AddPositionPingPongTween(_shuffleAnimationCamera.transform.position + Vector3.up * _cardAnimationData.CameraShuffleTweenUpAmount)
										   .SetDuration(_cardAnimationData.DeckShuffleExplosionDuration));
		}

		_Cards.Clear();
		_Cards = cardsToShuffleTo;
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
		var unShuffledCards = new CardViewtroller[unShuffleData.Length];
		for (int i = 0, iMax = unShuffleData.Length; i < iMax; ++i)
		{
			CardViewtroller lastCard = unShuffledCards[unShuffleData[i]] = _Cards[i];

			lastCard.ViewFSM.SetTextVisibility(true);

			AnimateShuffle(lastCard, i);

			shuffleAnimationWaiter.AddFinishable(lastCard.Holder);
		}

		if (_shuffleAnimationCamera != null && _shuffleAnimationOriginPoint != null)
		{
			shuffleAnimationWaiter.AddFinishable(
					_shuffleAnimationCamera.AddPositionPingPongTween(_shuffleAnimationCamera.transform.position + Vector3.up * _cardAnimationData.CameraShuffleTweenUpAmount)
										   .SetDuration(_cardAnimationData.DeckShuffleExplosionDuration));
		}

		_Cards.Clear();
		_Cards = new List<CardViewtroller>(unShuffledCards);
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
		var reShuffledCards = new CardViewtroller[unShuffleData.Length];
		for (int i = 0, iMax = unShuffleData.Length; i < iMax; ++i)
		{
			CardViewtroller lastCard = reShuffledCards[i] = _Cards[unShuffleData[i]];

			lastCard.ViewFSM.SetTextVisibility(true);

			AnimateShuffle(lastCard, i);

			shuffleAnimationWaiter.AddFinishable(lastCard.Holder);
		}

		if (_shuffleAnimationCamera != null && _shuffleAnimationOriginPoint != null)
		{
			shuffleAnimationWaiter.AddFinishable(
					_shuffleAnimationCamera.AddPositionPingPongTween(_shuffleAnimationCamera.transform.position + Vector3.up * _cardAnimationData.CameraShuffleTweenUpAmount)
										   .SetDuration(_cardAnimationData.DeckShuffleExplosionDuration));
		}

		_Cards.Clear();
		_Cards = new List<CardViewtroller>(reShuffledCards);
		shuffleAnimationWaiter.Ready = true;

		if (CardFlipAudio != null)
		{
			CardFlipAudio.PlayOneShot(_cardShuffleClip);
			StartCoroutine(PlayCardRuffleClip());
		}
	}

	private IEnumerator PlayCardRuffleClip()
	{
		yield return new WaitForSeconds(Mathf.Max(0.0f, _cardAnimationData.DeckShuffleExplosionDuration - _cardRuffleClip.length));
		CardFlipAudio.PlayOneShot(_cardRuffleClip);
	}

	private void AnimateShuffle(CardViewtroller card, int cardIndex)
	{
		float animationDuration = UnityEngine.Random.Range(2.0f, _cardAnimationData.DeckShuffleExplosionDuration / 2.0f);
		Vector3 rotationVector = (Mathf.Round(UnityEngine.Random.Range(1.0f, _cardAnimationData.DeckShuffleExplosionMaxRotations / 2.0f)) * 2.0f + 1.0f) * 360.0f * Vector3.one;
		rotationVector.z -= card.ViewFSM.GetAnimRotationOffset().z;
		card.AddPositionPingPongTween(card.gameObject.transform.position + Vector3.up * _cardAnimationData.DeckShuffleExplosionSphereRadius
					+ UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(0.1f, Mathf.Max(1.0f, _cardAnimationData.DeckShuffleExplosionSphereRadius)),
				GetCardPositionAtIndex(cardIndex))
			.SetDuration(animationDuration)
			.SetDelay(UnityEngine.Random.Range(0.0f, _cardAnimationData.DeckShuffleExplosionDuration - animationDuration))
			.AddLocalRotationTween(rotationVector, true)
			.SetShouldChangeLayer(true)
			.AddToOnFinishedOnce(() => OnCardRecieveTweenFinished(card));
	}

	public void DestroyAllCards()
	{
		for (int i = 0, iMax = _Cards.Count; i < iMax; ++i)
		{
			Destroy(_Cards[i].gameObject);
		}
		_Cards.Clear();
	}

	protected virtual void OnCardSent(CardViewtroller sentCard)
	{
		RepositionCards();
	}

	protected virtual void OnCardRecieveTweenBegan(CardViewtroller incomingCard)
	{
		_cardsInTransition.AddLast(incomingCard);
		RepositionCards();
	}

	protected virtual void OnCardRecieveTweenFinished(CardViewtroller card)
	{
		card.ViewFSM.SetTextVisibility(_cardsTextVisibility);
		_cardsInTransition.Remove(card);
	}

	protected abstract void RepositionCards();

	public Vector3 GetFinalPositionOfCardAtIndex(int index)
	{
		return GetCardPositionAtIndex(index) + _Cards[index].ViewFSM.GetAnimPositionOffset();
	}

	public Vector3 GetFinalPositionOfCard(CardViewtroller card)
	{
		return GetFinalPositionOfCardAtIndex(_Cards.IndexOf(card));
	}
}