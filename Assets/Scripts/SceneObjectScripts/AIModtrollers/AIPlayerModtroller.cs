using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nx;

public abstract class AIPlayerModtroller : AbstractPlayerModtroller
{
	protected class ExtremeAverageCardSplitter
	{
		public	readonly	LinkedList<int[]>	AverageCardIndexes;
		public	readonly	LinkedList<int[]>	ExtremeLowCardsIndexes;
		public	readonly	LinkedList<int[]>	ExtremeHighCardIndexes;

		private				AIPlayerModtroller	_parentModtroller;

		private ExtremeAverageCardSplitter() {}

		public ExtremeAverageCardSplitter(AIPlayerModtroller parentModtroller, List<int> cardIndexesToSplit)
		{
			_parentModtroller = parentModtroller;
			AverageCardIndexes = new LinkedList<int[]>();
			ExtremeLowCardsIndexes = new LinkedList<int[]>();
			ExtremeHighCardIndexes = new LinkedList<int[]>();

			ReadOnlyCollection<CardModViewtroller> handCards = _parentModtroller.Hand.ReadOnlyCards;
			
			for (int i = 0, iMax = cardIndexesToSplit.Count; i < iMax; ++i)
			{
				if (_parentModtroller.CardIsLowExtreme(handCards[cardIndexesToSplit[i]].CardValue))
				{
					ExtremeLowCardsIndexes.AddLast(new int[1] { cardIndexesToSplit[i] });
				}
				else if (_parentModtroller.CardIsHighExtreme(handCards[cardIndexesToSplit[i]].CardValue))
				{
					ExtremeHighCardIndexes.AddLast(new int[1] { cardIndexesToSplit[i] });
				}
				else // NOTE! Average cards are grouped because they suck and should be replaced by as many other cards as posssible
				{
					if (!AverageCardIndexes.Exists(idxs => handCards[idxs[0]].CardValue == handCards[cardIndexesToSplit[i]].CardValue))
					{
						AverageCardIndexes.AddLast(cardIndexesToSplit.AllSuchThat(idx => handCards[idx].CardValue == handCards[cardIndexesToSplit[i]].CardValue));
					}
				}
			}
		}
	}

	public override bool IsHuman { get { return false; } }

	protected void MainCardSelectionAlgorithm()
	{
		List<int> allowedCardIndexes = GetAllowedCardIndexes();
		MainGameModtroller.PlayDirection direction = _MainGameModtroller.Direction;
		float cardValueMidpoint = Enum.GetValues(typeof(Card.CardValue)).Length / 2.0f;
		ExtremeAverageCardSplitter splitter = new ExtremeAverageCardSplitter(this, allowedCardIndexes);

		if (splitter.AverageCardIndexes.Count > 0)
		{
			_MainGameModtroller.EndPlayerTurn(splitter.AverageCardIndexes.AllBest((a, b) => a.Length > b.Length, (a, b) => a.Length == b.Length)
						.Best((a, b) => Math.Abs(Hand.ReadOnlyCards[a[0]].CardValue - cardValueMidpoint) < Math.Abs(Hand.ReadOnlyCards[b[0]].CardValue - cardValueMidpoint)));
		}
		else
		{
			if (direction == MainGameModtroller.PlayDirection.UP)
			{
				if (splitter.ExtremeHighCardIndexes.Count > 0)
				{
					_MainGameModtroller.EndPlayerTurn(splitter.ExtremeHighCardIndexes.Best((a, b) => Hand.ReadOnlyCards[a[0]].CardValue < Hand.ReadOnlyCards[b[0]].CardValue));
				}
				else
				{
					_MainGameModtroller.EndPlayerTurn(splitter.ExtremeLowCardsIndexes.Best((a, b) => Hand.ReadOnlyCards[a[0]].CardValue > Hand.ReadOnlyCards[b[0]].CardValue));
				}
			}
			else if (direction == MainGameModtroller.PlayDirection.DOWN)
			{
				if (splitter.ExtremeLowCardsIndexes.Count > 0)
				{
					_MainGameModtroller.EndPlayerTurn(splitter.ExtremeLowCardsIndexes.Best((a, b) => Hand.ReadOnlyCards[a[0]].CardValue > Hand.ReadOnlyCards[b[0]].CardValue));
				}
				else
				{
					_MainGameModtroller.EndPlayerTurn(splitter.ExtremeHighCardIndexes.Best((a, b) => Hand.ReadOnlyCards[a[0]].CardValue < Hand.ReadOnlyCards[b[0]].CardValue));
				}
			}
			else // if (direction == MainGameModtroller.PlayDirection.UNDECIDED)
			{
				_MainGameModtroller.EndPlayerTurn(splitter.ExtremeLowCardsIndexes.Union(splitter.ExtremeHighCardIndexes, new NxUtils.FalseEqualityComparer<int[]>())
							.Best((a, b) => Math.Abs(Hand.ReadOnlyCards[a[0]].CardValue - cardValueMidpoint) < Math.Abs(Hand.ReadOnlyCards[b[0]].CardValue - cardValueMidpoint)));
			}
		}
	}

	protected bool CardIsLowExtreme(int cardValue)
	{
		return cardValue <= Mathf.RoundToInt(Enum.GetValues(typeof(Card.CardValue)).Length * 0.25f);
	}

	protected bool CardIsHighExtreme(int cardValue)
	{
		return cardValue >= Mathf.RoundToInt(Enum.GetValues(typeof(Card.CardValue)).Length * 0.75f);
	}
}
