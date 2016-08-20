using System;

[Serializable]
public class GameSettings
{
	public enum PlayerType : byte
	{
		HUMAN,
		AI_EASY,
		AI_HARD,
		NONE,
	}

	public	bool						WildCardRule;
	public	bool						EliminationRule;
	public	bool						OptionalPlayRule;
	public	bool						RefillHandRule;
	public	bool						AllOrNothingRule;
	public	bool						MaxDeviationRule;
	public	bool						DSwitchLBCRule;
	public	bool						SeeAICards;
	public	PlayerType[]				Players;
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
		Players					= new PlayerType[4]
									{
										PlayerType.HUMAN,
										PlayerType.NONE,
										PlayerType.AI_EASY,
										PlayerType.NONE,
									};
		NumberOfDecks			= 2;
		NumberOfCardsPerPlayer	= 7;
		NumberOfPointsToWin		= 5;
		MaxDeviationThreshold 	= 3;
	}

	public void SetupFor1PlayerGame()
	{
		Players = new PlayerType[4]
		{
			PlayerType.HUMAN,
			PlayerType.NONE,
			PlayerType.AI_EASY,
			PlayerType.NONE,
		};
	}
}