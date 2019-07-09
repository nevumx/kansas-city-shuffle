using System.Collections.Generic;

public class EasyAIPlayerModtroller : AIPlayerModtroller
{
	public override void BeginCardSelection()
	{
		List<int> allowedCardIndexes = GetAllowedCardIndexes();
		if (allowedCardIndexes.Count > 0)
		{
			MainCardSelectionAlgorithm();
		}
		else
		{
			_MainGameModtroller.EndPlayerTurn(null);
		}
	}
}