using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nx;

public class HumanPlayerModtroller : AbstractPlayerModtroller
{
						public	event		Action			OnHumanTurnBegan;

						private				List<int>		_selectedCardIndexes	= new List<int>();

						private				List<int>		_allowedCardIndexes		= null;

	[SerializeField]	private				GameObject		_submitCardsButton;

						public	override	bool			IsHuman					{ get { return true; } }

	public override AbstractPlayerModtroller Init(MainGameModtroller mainGameModtroller)
	{
		AbstractPlayerModtroller toReturn = base.Init(mainGameModtroller);
		_submitCardsButton = _MainGameModtroller.SubmitCardsButton;
		return toReturn;
	}

	public override void BeginCardSelection()
	{
		_allowedCardIndexes = GetAllowedCardIndexes();
		if (_allowedCardIndexes.Count > 0)
		{
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
			_submitCardsButton.SetActive(_MainGameModtroller.OptionalPlayRule);
		}
		else
		{
			_MainGameModtroller.EndPlayerTurn(null);
		}
	}

	public void SubmitCards()
	{
		Hand.ReadOnlyCards.ForEach(c => 
		{
			c.Button.CancelDrag();
			c.Button.ClearAllDelegates();
		});

		_submitCardsButton.SetActive(false);
		_MainGameModtroller.EndPlayerTurn(_selectedCardIndexes.ToArray());
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
				if (_selectedCardIndexes.IsEmpty())
				{
					_submitCardsButton.SetActive(_MainGameModtroller.OptionalPlayRule && !cards.Exists(c => c.Button.IsBeingDragged));
				}
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
				_submitCardsButton.SetActive(!cards.Exists(c => c.Button.IsBeingDragged));
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
			_submitCardsButton.SetActive(false);
		});

		cards[index].Button.AddToOnDrag(p =>
		{
			Ray ray = _MainGameModtroller.MainCamera.ScreenPointToRay(p.position); float distance;
			new Plane(cards[index].transform.up, cards[index].transform.position).Raycast(ray, out distance);
			cards[index].transform.position = ray.GetPoint(distance);
		});

		cards[index].Button.AddToOnDrop(p =>
		{
			if (p.delta.y > 0)
			{
				SubmitCards();
			}
			else
			{
				cards[index].AddIncrementalPositionTween(Hand.GetFinalPositionOfCardAtIndex(index))
							.AddIncrementalScaleTween(cards[index].ViewFSM.GetAnimScale())
							.SetDuration(cards[index].CardAnimationData.CardStateChangeDuration);
				_submitCardsButton.SetActive(!cards.Exists(c => c.Button.IsBeingDragged));
			}
		});
	}

	private void SetCardStates()
	{
		for (int i = 0, iMax = Hand.ReadOnlyCards.Count; i < iMax; ++i)
		{
			if (_selectedCardIndexes.Contains(i))
			{
				Hand.ReadOnlyCards[i].ViewFSM.SetAnimState(CardModViewtroller.CardViewFSM.AnimState.SELECTED);
			}
			else if (_allowedCardIndexes.Contains(i) && _selectedCardIndexes.IsEmpty())
			{
				Hand.ReadOnlyCards[i].ViewFSM.SetAnimState(CardModViewtroller.CardViewFSM.AnimState.ABLE_TO_BE_SELECTED);
			}
			else
			{
				Hand.ReadOnlyCards[i].ViewFSM.SetAnimState(CardModViewtroller.CardViewFSM.AnimState.VISIBLE);
			}
		}
	}
}