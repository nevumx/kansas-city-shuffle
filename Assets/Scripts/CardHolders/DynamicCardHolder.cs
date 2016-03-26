using UnityEngine;
using System.Collections.Generic;

public abstract class DynamicCardHolder : CardHolder
{
	private		LinkedList<CardModViewtroller>	_cardsInTransition	= new LinkedList<CardModViewtroller>();
	protected	LinkedList<CardModViewtroller>	_CardsInTransition	{ get { return _cardsInTransition; } }

	protected override void OnCardSent(CardModViewtroller sentCard)
	{
		RepositionCards();
	}

	protected override void OnCardRecieveTweenBegan(CardModViewtroller incomingCard)
	{
		_cardsInTransition.AddLast(incomingCard);
		RepositionCards();
	}

	protected override void OnCardRecieveTweenFinished(CardModViewtroller card)
	{
		base.OnCardRecieveTweenFinished(card);
		_cardsInTransition.Remove(card);
	}

	protected abstract void RepositionCards();
}
