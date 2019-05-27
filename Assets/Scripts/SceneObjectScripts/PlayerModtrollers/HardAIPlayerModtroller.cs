using System.Collections.Generic;

public class HardAIPlayerModtroller : AIPlayerModtroller
{
	public override void BeginCardSelection()
	{
		MainGameModtroller.PlayDirection direction = _MainGameModtroller.Direction;
		List<int> allowedCardIndexes = GetAllowedCardIndexes();

		if (allowedCardIndexes.Count > 0)
		{
			var allOtherCards = new List<CardViewtroller>();
			if (direction != MainGameModtroller.PlayDirection.UNDECIDED)
			{
				AbstractPlayerModtroller[] allPlayers = _MainGameModtroller.Players;
				for (int i = 0, iMax = allPlayers.Length; i < iMax; ++i)
				{
					if (!ReferenceEquals(allPlayers[i], this) && allPlayers[i] != null)
					{
						for (int j = 0, jMax = allPlayers[i].Hand.ReadOnlyCards.Count; j < jMax; ++j)
						{
							allOtherCards.Add(allPlayers[i].Hand.ReadOnlyCards[j]);
						}
					}
				}

				for (int i = 0, iMax = allowedCardIndexes.Count; i < iMax; ++i)
				{
					if ((direction == MainGameModtroller.PlayDirection.UP
						&& !allOtherCards.Exists(c => c.CardValue >= Hand.ReadOnlyCards[allowedCardIndexes[i]].CardValue))
						|| (direction == MainGameModtroller.PlayDirection.DOWN
						&& !allOtherCards.Exists(c => c.CardValue <= Hand.ReadOnlyCards[allowedCardIndexes[i]].CardValue)))
					{
						_MainGameModtroller.EndPlayerTurn(new int[1] { allowedCardIndexes[i] });
						return;
					}
				}
			}

			MainCardSelectionAlgorithm();
		}
		else
		{
			_MainGameModtroller.EndPlayerTurn(null);
		}
	}
}
