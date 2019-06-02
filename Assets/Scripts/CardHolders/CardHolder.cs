using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nx;

public abstract class CardHolder : MonoBehaviour
{

	[SerializeField]	private		CardAnimationData							_cardAnimationData;
						protected	CardAnimationData							_CardAnimationData				{ get { return _cardAnimationData; } }

						private		List<CardController>						_cards							= new List<CardController>();
						public		int											CardCount						{ get { return _Cards.Count; } }

						private		ReadOnlyCollection<CardController>			_cachedReadOnlyCards;
	[SerializeField]	private		CardController.CardViewFSM.AnimState		_cardsAnimState;
						public		CardController.CardViewFSM.AnimState		CardsAnimState					{ get { return _cardsAnimState; } }
	[SerializeField]	private		bool										_cardsTextVisibility;
						protected	bool										_CardsTextVisibility			{ get { return _cardsTextVisibility; } }
						private		LinkedList<CardController>					_cardsInTransition				= new LinkedList<CardController>();
						protected	LinkedList<CardController>					_CardsInTransition				{ get { return _cardsInTransition; } }

						public		AudioSource									CardFlipAudio					{ set; protected get; }

	public ReadOnlyCollection<CardController> ReadOnlyCards
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

	protected List<CardController> _Cards
	{
		get
		{
			return _cards;
		}
		private set
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

	public void SetCardsAnimStates(CardController.CardViewFSM.AnimState newState, Action onFinished)
	{
		_cardsAnimState = newState;
		var animGroupWaiter = new FinishableGroupWaiter(onFinished);
		for (int i = 0, iMax = _Cards.Count; i < iMax; ++i)
		{
			animGroupWaiter.AddFinishable(_Cards[i].ViewFSM.SetAnimState(newState));
		}
		animGroupWaiter.Ready = true;
	}

	public void SetIntendedIncomingCardAnimState(CardController.CardViewFSM.AnimState newState)
	{
		_cardsAnimState = newState;
	}

	public void IntroduceCard(CardController cardToIntroduce, out TweenHolder outTween,
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

			outTween = cardToIntroduce.AddOffsetHeightTween(_cardAnimationData.DeckFillFancyIntroTweenHeight,
											GetCardPositionAtIndex(_Cards.LastIndex()), true)
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
		CardController cardBeingMoved = _Cards[cardIndex];
		_Cards.RemoveAt(cardIndex);
		if (!other._Cards.InsertionIndexIsValid(indexToInsertAt))
		{
			indexToInsertAt = other._Cards.Count;
		}
		other.AddCard(cardBeingMoved, indexToInsertAt);

		cardBeingMoved.ViewFSM.SetAnimState(other._cardsAnimState, performTweens: false);
		cardBeingMoved.ViewFSM.SetTextVisibility(visibleDuringTween.HasValue ? visibleDuringTween.Value : other._cardsTextVisibility);

		outTween = cardBeingMoved.AddOffsetHeightTween(_cardAnimationData.GeneralCardMoveHeight,
										other.GetFinalPositionOfCardAtIndex(indexToInsertAt), true)
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

	private void AddCard(CardController card, int indexToInsertAt = -1)
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

	public void DestroyAllCards()
	{
		for (int i = 0, iMax = _Cards.Count; i < iMax; ++i)
		{
			Destroy(_Cards[i].gameObject);
		}
		_Cards.Clear();
	}

	protected virtual void OnCardSent(CardController sentCard)
	{
		RepositionCards();
	}

	protected virtual void OnCardRecieveTweenBegan(CardController incomingCard)
	{
		_cardsInTransition.AddLast(incomingCard);
		RepositionCards();
	}

	protected virtual void OnCardRecieveTweenFinished(CardController card)
	{
		card.ViewFSM.SetTextVisibility(_cardsTextVisibility);
		_cardsInTransition.Remove(card);
	}

	protected abstract void RepositionCards();

	public Vector3 GetFinalPositionOfCardAtIndex(int index)
	{
		return GetCardPositionAtIndex(index) + _Cards[index].ViewFSM.GetAnimPositionOffset();
	}

	public Vector3 GetFinalPositionOfCard(CardController card)
	{
		return GetFinalPositionOfCardAtIndex(_Cards.IndexOf(card));
	}
}