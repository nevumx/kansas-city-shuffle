using UnityEngine;
using System;
using System.Collections;

public struct Card
{
	public enum CardValue : byte
	{
		ACE = 1,
		TWO = 2,
		THREE = 3,
		FOUR = 4,
		FIVE = 5,
		SIX = 6,
		SEVEN = 7,
		EIGHT = 8,
		NINE = 9,
		TEN = 10,
		JACK = 11,
		QUEEN = 12,
		KING = 13,
	}

	public enum CardSuit : byte
	{
		SPADES,
		CLUBS,
		HEARTS,
		DIAMONDS,
	}

	public	readonly	CardValue	Value;
	public	readonly	CardSuit	Suit;

	public string CardValueString
	{
		get
		{
			switch (Value)
			{
				case Card.CardValue.ACE:
					return "A\n";
				case Card.CardValue.TWO:
					return "2\n";
				case Card.CardValue.THREE:
					return "3\n";
				case Card.CardValue.FOUR:
					return "4\n";
				case Card.CardValue.FIVE:
					return "5\n";
				case Card.CardValue.SIX:
					return "6\n";
				case Card.CardValue.SEVEN:
					return "7\n";
				case Card.CardValue.EIGHT:
					return "8\n";
				case Card.CardValue.NINE:
					return "9\n";
				case Card.CardValue.TEN:
					return "10\n";
				case Card.CardValue.JACK:
					return "J\n";
				case Card.CardValue.QUEEN:
					return "Q\n";
				case Card.CardValue.KING:
					return "K\n";
				default:
					break;
			}
			return "?";
		}
	}

	public string CardSuitString
	{
		get
		{
			switch (Suit)
			{
				case Card.CardSuit.SPADES:
					return "\n\u2660";
				case Card.CardSuit.CLUBS:
					return "\n\u2663";
				case Card.CardSuit.HEARTS:
					return "\n\u2665";
				case Card.CardSuit.DIAMONDS:
					return "\n\u2666";
				default:
					break;
			}
			return "\n?";
		}
	}

	public Card(CardValue value, CardSuit suit)
	{
		Value = value;
		Suit = suit;
	}
}