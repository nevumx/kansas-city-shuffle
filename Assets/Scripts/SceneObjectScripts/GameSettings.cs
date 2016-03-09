using System;

[Serializable]
public class GameSettings
{
	public	bool						WildCardRule;
	public	bool						EliminationRule;
	public	bool						OptionalPlayRule;
	public	bool						RefillHandRule;
	public	bool						AllOrNothingRule;
	public	bool						MaxDeviationRule;
	public	bool						DSwitchLBCRule; // LBC = lose best card
	public	bool						SeeAICards;
	public	CustomPlayer.PlayerType[]	Players;
	public	int							NumberOfDecks;
	public	int							NumberOfCardsPerPlayer;
	public	int							NumberOfPointsToWin;
	public	int							MaxDeviationThreshold;

	public GameSettings()
	{
		WildCardRule			= false;
		EliminationRule			= false;
		OptionalPlayRule		= false;
		RefillHandRule			= false;
		AllOrNothingRule		= false;
		MaxDeviationRule		= false;
		DSwitchLBCRule			= false;
		SeeAICards				= false;
		Players					= new CustomPlayer.PlayerType[4]
									{
										CustomPlayer.PlayerType.HUMAN,
										CustomPlayer.PlayerType.NONE,
										CustomPlayer.PlayerType.AI_EASY,
										CustomPlayer.PlayerType.NONE,
									};
		NumberOfDecks			= 2;
		NumberOfCardsPerPlayer	= 7;
		NumberOfPointsToWin		= 5;
		MaxDeviationThreshold 	= 3;
	}

	public void SetupFor1PlayerGame()
	{
		Players = new CustomPlayer.PlayerType[4]
		{
			CustomPlayer.PlayerType.HUMAN,
			CustomPlayer.PlayerType.NONE,
			CustomPlayer.PlayerType.AI_EASY,
			CustomPlayer.PlayerType.NONE,
		};
	}

	public void SetupFor2PlayerGame()
	{
		Players = new CustomPlayer.PlayerType[4]
		{
			CustomPlayer.PlayerType.HUMAN,
			CustomPlayer.PlayerType.NONE,
			CustomPlayer.PlayerType.HUMAN,
			CustomPlayer.PlayerType.NONE,
		};
	}
}
