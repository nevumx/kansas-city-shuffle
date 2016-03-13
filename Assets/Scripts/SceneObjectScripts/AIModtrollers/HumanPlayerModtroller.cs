using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nx;

public class HumanPlayerModtroller : AbstractPlayerModtroller
{
						public	event		Action			OnHumanTurnBegan;

						private				LinkedList<int>	_selectedCardIndexes		= new LinkedList<int>();

	[SerializeField]	private				GameObject		_submitCardsButton;

						public	override	bool			IsHuman	{ get { return true; } }

	public override AbstractPlayerModtroller Init(MainGameModtroller mainGameModtroller)
	{
		AbstractPlayerModtroller toReturn = base.Init(mainGameModtroller);
		_submitCardsButton = _MainGameModtroller.SubmitCardsButton;
		return toReturn;
	}

	public override void BeginCardSelection()
	{
		List<int> allowedCardIndexes = GetAllowedCardIndexes();
		if (allowedCardIndexes.Count > 0)
		{
			OnHumanTurnBegan.Raise();
			Hand.CardsTextVisibility = true;
			if (allowedCardIndexes.Count == 1)
			{
				_selectedCardIndexes.AddLast(allowedCardIndexes[0]);
			}
			UpdateOptions(allowedCardIndexes);
		}
		else
		{
			_MainGameModtroller.EndPlayerTurn(null);
		}
	}

	public void OnSubmitCardsButtonClicked()
	{
		Hand.ReadOnlyCards.ForEach(c => c.Button.ClearOnClickedDelegates());
		_submitCardsButton.SetActive(false);
		_MainGameModtroller.EndPlayerTurn(_selectedCardIndexes.ToArray());
		_selectedCardIndexes.Clear();
	}

	public void CancelCardSelection(Action onFinished)
	{
		_selectedCardIndexes.Clear();
		_submitCardsButton.SetActive(false);
		Hand.SetCardsAnimStates(CardModViewtroller.CardViewFSM.AnimState.OBSCURED, onFinished: () =>
		{
			Hand.CardsTextVisibility = false;
			onFinished();
		});
	}

	private void UpdateOptions(List<int> allowedCardIndexes)
	{
		Action updateOptionsDelegate = () => UpdateOptions(allowedCardIndexes);
		ReadOnlyCollection<CardModViewtroller> cards = Hand.ReadOnlyCards;
		cards.ForEach(c => c.Button.ClearOnClickedDelegates());
		if (_selectedCardIndexes.Count <= 0)
		{
			for (int i = 0, iMax = cards.Count; i < iMax; ++i)
			{
				if (_MainGameModtroller.Direction == MainGameModtroller.PlayDirection.UNDECIDED
					|| allowedCardIndexes.Exists(n => n == i))
				{
					SetButtonActiveToAdd(cards[i], updateOptionsDelegate);
				}
				else
				{
					SetButtonInactive(cards[i]);
				}
			}
			_submitCardsButton.SetActive(_MainGameModtroller.OptionalPlayRule);
		}
		else
		{
			for (int i = 0, iMax = cards.Count; i < iMax; ++i)
			{
				if (_selectedCardIndexes.Exists(n => n == i))
				{
					SetButtonActiveToRemove(cards[i], updateOptionsDelegate);
				}
				else
				{
					if (cards[i].CardValue == cards[_selectedCardIndexes.First.Value].CardValue)
					{
						SetButtonActiveToAdd(cards[i], updateOptionsDelegate);
					}
					else if (allowedCardIndexes.Exists(n => n == i))
					{
						SetButtonInactiveToReplace(cards[i], updateOptionsDelegate);
					}
					else
					{
						SetButtonInactive(cards[i]);
					}
				}
			}
			_submitCardsButton.gameObject.SetActive(true);
		}
	}

	private void SetButtonActiveToAdd(CardModViewtroller card, Action updateOptionsLambda)
	{
		card.ViewFSM.SetAnimState(CardModViewtroller.CardViewFSM.AnimState.ABLE_TO_BE_SELECTED);
		card.Button.AddToOnClicked(() =>
		{
			_selectedCardIndexes.AddLast(Hand.ReadOnlyCards.IndexOf(card));
			updateOptionsLambda();
		});
	}

	private void SetButtonActiveToRemove(CardModViewtroller card, Action updateOptionsLambda)
	{
		card.ViewFSM.SetAnimState(CardModViewtroller.CardViewFSM.AnimState.SELECTED);
		card.Button.AddToOnClicked(() =>
		{
			_selectedCardIndexes.Remove(Hand.ReadOnlyCards.IndexOf(card));
			updateOptionsLambda();
		});
	}

	private void SetButtonInactiveToReplace(CardModViewtroller card, Action updateOptionsLambda)
	{
		card.ViewFSM.SetAnimState(CardModViewtroller.CardViewFSM.AnimState.VISIBLE);
		card.Button.AddToOnClicked(() =>
		{
			_selectedCardIndexes.Clear();
			_selectedCardIndexes.AddLast(Hand.ReadOnlyCards.IndexOf(card));
			updateOptionsLambda();
		});
	}

	private void SetButtonInactive(CardModViewtroller card)
	{
		card.ViewFSM.SetAnimState(CardModViewtroller.CardViewFSM.AnimState.VISIBLE);
	}
}
