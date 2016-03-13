using UnityEngine;
using System;
using System.Collections.Generic;
using Nx;

public class EasyAIPlayerModtroller : AIPlayerModtroller
{
	public override void BeginCardSelection()
	{
		List<int> allowedCardIndexes = GetAllowedCardIndexes();
		if (_MainGameModtroller.Direction == MainGameModtroller.PlayDirection.UNDECIDED || allowedCardIndexes.Count > 0)
		{
			MainCardSelectionAlgorithm();
		}
		else
		{
			_MainGameModtroller.EndPlayerTurn(null);
		}
	}
}
