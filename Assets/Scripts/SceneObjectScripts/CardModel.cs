public struct CardModel
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
				case CardValue.ACE:
					return "A\n";
				case CardValue.TWO:
					return "2\n";
				case CardValue.THREE:
					return "3\n";
				case CardValue.FOUR:
					return "4\n";
				case CardValue.FIVE:
					return "5\n";
				case CardValue.SIX:
					return "6\n";
				case CardValue.SEVEN:
					return "7\n";
				case CardValue.EIGHT:
					return "8\n";
				case CardValue.NINE:
					return "9\n";
				case CardValue.TEN:
					return "10\n";
				case CardValue.JACK:
					return "J\n";
				case CardValue.QUEEN:
					return "Q\n";
				case CardValue.KING:
					return "K\n";
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
				case CardSuit.SPADES:
					return "\n\u2660";
				case CardSuit.CLUBS:
					return "\n\u2663";
				case CardSuit.HEARTS:
					return "\n\u2665";
				case CardSuit.DIAMONDS:
					return "\n\u2666";
			}
			return "\n?";
		}
	}

	public CardModel(CardValue value, CardSuit suit)
	{
		Value = value;
		Suit = suit;
	}
}