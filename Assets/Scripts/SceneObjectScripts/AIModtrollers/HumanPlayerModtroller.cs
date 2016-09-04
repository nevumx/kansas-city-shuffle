using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nx;

public class HumanPlayerModtroller : AbstractPlayerModtroller
{
	private	static	readonly	float			CARD_DRAG_ACCELERATION	= 3.0f;

	public	event				Action			OnHumanTurnBegan;

	private						List<int>		_selectedCardIndexes	= new List<int>();
	private						List<int>		_allowedCardIndexes		= null;

	private						GameObject		_submitCardsButton;

	public	override			bool			IsHuman					{ get { return true; } }

	public override AbstractPlayerModtroller Init(MainGameModtroller mainGameModtroller)
	{
		AbstractPlayerModtroller toReturn = base.Init(mainGameModtroller);
		_submitCardsButton = _MainGameModtroller.SubmitCardsButton;
		return toReturn;
	}

	public override void BeginCardSelection()
	{
		_allowedCardIndexes = GetAllowedCardIndexes();

		OnHumanTurnBegan.Raise();

		ReadOnlyCollection<CardModViewtroller> cards = Hand.ReadOnlyCards;
		cards.ForEach(c =>
		{
			c.Button.CancelDrag();
			c.Button.ClearAllDelegates();
		});

		for (int i = 0, iMax = cards.Count; i < iMax; ++i)
		{
			if (_allowedCardIndexes.Contains(i))
			{
				SetupInteractionDelegatesForCardAtIndex(i);
			}
		}

		Hand.CardsTextVisibility = true;
		SetCardStates();
	}

	public void SubmitCards(bool handCardsWereDragged = false)
	{
		Hand.ReadOnlyCards.ForEach(c => 
		{
			c.Button.CancelDrag();
			c.Button.ClearAllDelegates();
		});

		_submitCardsButton.SetActive(false);
		_MainGameModtroller.EndPlayerTurn(_selectedCardIndexes.ToArray(), handCardsWereDragged);
		_selectedCardIndexes.Clear();
	}

	public void CancelCardSelection(Action onFinished)
	{
		Hand.ReadOnlyCards.ForEach(c => 
		{
			c.Button.CancelDrag();
			c.Button.ClearAllDelegates();
		});

		_selectedCardIndexes.Clear();
		_submitCardsButton.SetActive(false);
		Hand.SetCardsAnimStates(CardModViewtroller.CardViewFSM.AnimState.OBSCURED, onFinished: () =>
		{
			Hand.CardsTextVisibility = false;
			onFinished();
		});
	}

	private void SetupInteractionDelegatesForCardAtIndex(int index)
	{
		ReadOnlyCollection<CardModViewtroller> cards = Hand.ReadOnlyCards;

		cards[index].Button.AddToOnClicked(() =>
		{
			if (_selectedCardIndexes.Contains(index))
			{
				_selectedCardIndexes.Remove(index);
			}
			else
			{
				_selectedCardIndexes.ForEach(i =>
				{
					if (cards[i].CardValue != cards[index].CardValue)
					{
						cards[i].Button.CancelDrag();
					}
				});
				_selectedCardIndexes.Add(index);
				_selectedCardIndexes.RemoveAll(i => Hand.ReadOnlyCards[i].CardValue != cards[index].CardValue);
			}
			SetCardStates();
		});

		cards[index].Button.AddToOnDoubleClicked(() =>
		{
			if (!_selectedCardIndexes.Contains(index))
			{
				_selectedCardIndexes.Add(index);
				_selectedCardIndexes.RemoveAll(i => cards[i].CardValue != cards[index].CardValue);
			}
			SubmitCards();
		});

		cards[index].Button.AddToOnBeginDrag(() =>
		{
			if (!_selectedCardIndexes.Contains(index))
			{
				_selectedCardIndexes.ForEach(i =>
				{
					if (cards[i].CardValue != cards[index].CardValue)
					{
						cards[i].Button.CancelDrag();
					}
				});
				_selectedCardIndexes.Add(index);
				_selectedCardIndexes.RemoveAll(i => cards[i].CardValue != cards[index].CardValue);
			}
			SetCardStates();
		});

		cards[index].Button.AddToOnDrag(p =>
		{
			Ray ray = _MainGameModtroller.MainCamera.ScreenPointToRay(p.position); float distance;
			new Plane(cards[index].ParentCardHolder.transform.up, cards[index].ParentCardHolder.GetFinalPositionOfCard(cards[index])).Raycast(ray, out distance);
			IncrementalPositionTween posTween = null;
			Vector3 targetPosition = ray.GetPoint(distance);
			if ((posTween = cards[index].TweenHolder.GetTweenOfType<IncrementalPositionTween>()) != null)
			{
				posTween.PositionTo = targetPosition;
			}
			else
			{
				cards[index].transform.position += (targetPosition - cards[index].transform.position) * CARD_DRAG_ACCELERATION * Time.deltaTime;
			}
		});

		cards[index].Button.AddToOnDrop(p =>
		{
			if (p.delta.y > 0.0f || (p.delta.y == 0.0f && p.position.y > Screen.height / 2.0f))
			{
				SubmitCards(handCardsWereDragged: true);
			}
			else
			{
				if (!_selectedCardIndexes.Exists(i => cards[i].CardValue == cards[index].CardValue && i != index && !cards[i].Button.IsBeingDragged))
				{
					_selectedCardIndexes.Remove(index);
					SetCardStates();
				}
				cards[index].AddIncrementalPositionTween(Hand.GetFinalPositionOfCardAtIndex(index))
							.AddIncrementalScaleTween(cards[index].ViewFSM.GetAnimScale())
							.SetDuration(cards[index].CardAnimationData.CardStateChangeDuration);
			}
		});
	}

	private void SetCardStates(Action onFinished = null)
	{
		FinishableGroupWaiter cardStateTransitionWaiter = null;
		if (onFinished != null)
		{
			cardStateTransitionWaiter = new FinishableGroupWaiter(onFinished);
		}
		for (int i = 0, iMax = Hand.ReadOnlyCards.Count; i < iMax; ++i)
		{
			TweenHolder transitionTweenHolder;
			if (_selectedCardIndexes.Contains(i))
			{
				transitionTweenHolder = Hand.ReadOnlyCards[i].ViewFSM.SetAnimState(CardModViewtroller.CardViewFSM.AnimState.SELECTED);
			}
			else if (_allowedCardIndexes.Contains(i) && _selectedCardIndexes.IsEmpty())
			{
				transitionTweenHolder = Hand.ReadOnlyCards[i].ViewFSM.SetAnimState(CardModViewtroller.CardViewFSM.AnimState.ABLE_TO_BE_SELECTED);
			}
			else
			{
				transitionTweenHolder = Hand.ReadOnlyCards[i].ViewFSM.SetAnimState(CardModViewtroller.CardViewFSM.AnimState.VISIBLE);
			}
			cardStateTransitionWaiter.IfIsNotNullThen(c => c.AddFinishable(transitionTweenHolder));
		}
		_submitCardsButton.SetActive(_allowedCardIndexes.Count <= 0 || (!Hand.ReadOnlyCards.Exists(c => c.Button.IsBeingDragged)
									 && (_MainGameModtroller.OptionalPlayRule || !_selectedCardIndexes.IsEmpty())));
		cardStateTransitionWaiter.IfIsNotNullThen(c => c.Ready = true);
	}
}