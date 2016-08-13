using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nx;

public abstract class AbstractPlayerModtroller : MonoBehaviour
{
	[SerializeField]	private						Hand				_hand;
						public						Hand				Hand						{ get { return _hand; } }

	[SerializeField]	private						TextMesh			_scoreText;

						private						MainGameModtroller	_mainGameModtroller;
						protected					MainGameModtroller	_MainGameModtroller			{ get { return _mainGameModtroller; } }

	[NonSerialized]		public						string				PlayerName					= "Player ";
						private	static	readonly	string				PLAYER_ELIMINATED_TAG_LABEL = " - Out";

	[NonSerialized]		private						bool				_eliminated					= false;
	public bool Eliminated 
	{
		get
		{
			return _eliminated;
		}
		set
		{
			if (value && !_eliminated)
			{
				_scoreText.text = _scoreText.text.Insert(_scoreText.text.IndexOf('\n'), PLAYER_ELIMINATED_TAG_LABEL);
			}
			else if (!value && _eliminated)
			{
				_scoreText.text = _scoreText.text.Replace(PLAYER_ELIMINATED_TAG_LABEL, "");
			}
			_eliminated = value;
		}
	}

	private int _points = 0;
	public int Points
	{
		get
		{
			return _points;
		}
		set
		{
			_points = value;
			_scoreText.text = PlayerName + "\nScore: " + _points;
		}
	}

	public virtual AbstractPlayerModtroller Init(MainGameModtroller mainGameModtroller)
	{
		_mainGameModtroller = mainGameModtroller;
		return this;
	}

	public abstract bool IsHuman { get; }

	public abstract void BeginCardSelection();

	protected List<int> GetAllowedCardIndexes()
	{
		int cardValue = _MainGameModtroller.DiscardPileLastValue;
		MainGameModtroller.PlayDirection direction = _MainGameModtroller.Direction;
		ReadOnlyCollection<CardModViewtroller> handCards = Hand.ReadOnlyCards;
		List<int> allowedCardIndexes = new List<int>();
		if (_MainGameModtroller.MaxDeviationRule && direction != MainGameModtroller.PlayDirection.UNDECIDED)
		{
			allowedCardIndexes.AddRange(handCards.AllIndexesSuchThat(
										c => Mathf.Abs(c.CardValue - cardValue) < _MainGameModtroller.MaxDeviationThreshold));

			if (allowedCardIndexes.Count <= 0)
			{
				Func<CardModViewtroller, CardModViewtroller, bool> AIsCloserToCardValueThanB =
					(a, b) => Mathf.Abs(a.CardValue - cardValue) < Mathf.Abs(b.CardValue - cardValue);

				Func<CardModViewtroller, CardModViewtroller, bool> AIsSameDistanceToCardValueAsB =
					(a, b) => Mathf.Abs(a.CardValue - cardValue) == Mathf.Abs(b.CardValue - cardValue);

				allowedCardIndexes.AddRange(handCards.AllBestIndexes(AIsCloserToCardValueThanB, AIsSameDistanceToCardValueAsB));
			}

			if (_MainGameModtroller.WildcardRule)
			{
				allowedCardIndexes.AddRange(handCards.AllIndexesSuchThat(
					(c, i) => c.CardValue == _MainGameModtroller.WildCardValue && !allowedCardIndexes.Contains(i)));
			}
		}
		else
		{
			for (int i = 0, iMax = handCards.Count; i < iMax; ++i)
			{
				allowedCardIndexes.Add(i);
			}
		}

		for (int i = allowedCardIndexes.LastIndex(); i >= 0; --i) // Reverse iterate to not invalidate
		{
			if (!((direction == MainGameModtroller.PlayDirection.UNDECIDED)
			   || (_MainGameModtroller.WildcardRule && handCards[allowedCardIndexes[i]].CardValue == _MainGameModtroller.WildCardValue)
			   || (direction == MainGameModtroller.PlayDirection.DOWN && handCards[allowedCardIndexes[i]].CardValue <= cardValue)
			   || (direction == MainGameModtroller.PlayDirection.UP && handCards[allowedCardIndexes[i]].CardValue >= cardValue)))
			{
				allowedCardIndexes.RemoveAt(i);
			}
		}
		return allowedCardIndexes;
	}

	public int GetMostAdvantageousCardIndex()
	{
		return _hand.ReadOnlyCards.BestIndex((a, b) => _MainGameModtroller.Direction == MainGameModtroller.PlayDirection.DOWN ?
													   a.CardValue < b.CardValue : a.CardValue > b.CardValue);
	}
}
