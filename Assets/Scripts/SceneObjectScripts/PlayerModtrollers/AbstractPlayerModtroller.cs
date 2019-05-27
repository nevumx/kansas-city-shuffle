using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nx;

public abstract class AbstractPlayerModtroller : MonoBehaviour
{
						private	static	readonly	float				DIMMED_PLAYER_SYMBOL_ALPHA	= 0.25f;

	[SerializeField]	private						Hand				_hand;
						public						Hand				Hand						{ get { return _hand; } }

	[SerializeField]	private						TextMesh			_playerSymbolText;
						public						TextMesh			PlayerSymbolText			{ get { return _playerSymbolText; } }
	[SerializeField]	private						TextMesh			_scoreText;

						protected					MainGameModtroller	_MainGameModtroller			{ get; private set; }

	[NonSerialized]		private						bool				_eliminated;
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
				_playerSymbolText.SetAlpha(DIMMED_PLAYER_SYMBOL_ALPHA);
				_scoreText.SetAlpha(DIMMED_PLAYER_SYMBOL_ALPHA);
			}
			else if (!value && _eliminated)
			{
				_playerSymbolText.SetAlpha(1.0f);
				_scoreText.SetAlpha(1.0f);
			}
			_eliminated = value;
		}
	}

	private int _points;
	public int Points
	{
		get
		{
			return _points;
		}
		set
		{
			_points = value;
			_scoreText.text = _points.ToString();
		}
	}

	public virtual AbstractPlayerModtroller Init(MainGameModtroller mainGameModtroller)
	{
		_MainGameModtroller = mainGameModtroller;
		Hand.CardFlipAudio = _MainGameModtroller.CardFlipAudio;
		return this;
	}

	public abstract bool IsHuman { get; }

	public abstract void BeginCardSelection();

	protected List<int> GetAllowedCardIndexes()
	{
		int lastCardValue = _MainGameModtroller.DiscardPileLastValue;
		MainGameModtroller.PlayDirection direction = _MainGameModtroller.Direction;
		ReadOnlyCollection<CardViewtroller> handCards = Hand.ReadOnlyCards;
		List<int> allowedCardIndexes = new List<int>();
		if (_MainGameModtroller.MaxDeviationRule && direction != MainGameModtroller.PlayDirection.UNDECIDED)
		{
			allowedCardIndexes.AddRange(handCards.AllIndexesSuchThat(
										c => Mathf.Abs(c.CardValue - lastCardValue) <= _MainGameModtroller.MaxDeviationThreshold));

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
			   || (direction == MainGameModtroller.PlayDirection.DOWN && handCards[allowedCardIndexes[i]].CardValue <= lastCardValue)
			   || (direction == MainGameModtroller.PlayDirection.UP && handCards[allowedCardIndexes[i]].CardValue >= lastCardValue)))
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
