using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Nx;

public class EasyAIPlayerModtroller : AIPlayerModtroller
{
	public override void BeginCardSelection(Action<int[]> onTurnEnded)
	{
		List<int> allowedCardIndexes = GetAllowedCardIndexes();
		if (_mainGameModtroller.Direction == MainGameModtroller.PlayDirection.UNDECIDED || allowedCardIndexes.Count > 0)
		{
			MainCardSelectionAlgorithm(onTurnEnded);
		}
		else
		{
			onTurnEnded(null);
		}
	}
}
