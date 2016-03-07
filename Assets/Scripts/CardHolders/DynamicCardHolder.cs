using UnityEngine;
using System.Collections.Generic;

public abstract class DynamicCardHolder : CardHolder
{
	private		LinkedList<CardModViewtroller>	_cardsInTransition	= new LinkedList<CardModViewtroller>();
	protected	LinkedList<CardModViewtroller>	_CardsInTransition	{ get { return _cardsInTransition; } }

	protected override void OnCardSent(CardModViewtroller sentCard)
	{
		base.OnCardSent(sentCard);
		LinkedListNode<CardModViewtroller> outgoingCardNode = _cardsInTransition.AddLast(sentCard);
		sentCard.TweenHolder.AddToOnFinishedOnce(() => _cardsInTransition.Remove(outgoingCardNode));
		RepositionCards(sentCard);
	}

	protected override void OnCardRecieveTweenBegan(CardModViewtroller incomingCard)
	{
		base.OnCardRecieveTweenBegan(incomingCard);
		LinkedListNode<CardModViewtroller> incomingCardNode = _cardsInTransition.AddLast(incomingCard);
		incomingCard.TweenHolder.AddToOnFinishedOnce(() => _cardsInTransition.Remove(incomingCardNode));
		RepositionCards(incomingCard);
	}

	protected abstract void RepositionCards(CardModViewtroller inOrOutCard);
}
