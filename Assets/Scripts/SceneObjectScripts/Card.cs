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

	public Card(CardValue value, CardSuit suit)
	{
		Value = value;
		Suit = suit;
	}

	public override string ToString()
	{
		string cardValue = null;
		switch (Value)
		{
		case Card.CardValue.ACE:
			cardValue = "1";
			break;
		case Card.CardValue.TWO:
			cardValue = "2";
			break;
		case Card.CardValue.THREE:
			cardValue = "3";
			break;
		case Card.CardValue.FOUR:
			cardValue = "4";
			break;
		case Card.CardValue.FIVE:
			cardValue = "5";
			break;
		case Card.CardValue.SIX:
			cardValue = "6";
			break;
		case Card.CardValue.SEVEN:
			cardValue = "7";
			break;
		case Card.CardValue.EIGHT:
			cardValue = "8";
			break;
		case Card.CardValue.NINE:
			cardValue = "9";
			break;
		case Card.CardValue.TEN:
			cardValue = "10";
			break;
		case Card.CardValue.JACK:
			cardValue = "J";
			break;
		case Card.CardValue.QUEEN:
			cardValue = "Q";
			break;
		case Card.CardValue.KING:
			cardValue = "K";
			break;
		default:
			cardValue = "?";
			break;
		}

		switch (Suit)
		{
		case Card.CardSuit.SPADES:
			return cardValue + "\n\u2660";
		case Card.CardSuit.CLUBS:
			return cardValue + "\n\u2663";
		case Card.CardSuit.HEARTS:
			return cardValue + "\n\u2665";
		case Card.CardSuit.DIAMONDS:
			return cardValue + "\n\u2666";
		default:
			return cardValue + " ?";
		}
	}
}