using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

	public	static	readonly	string			SAVED_SETTINGS_FILE_NAME	= "/KCSGameSettings.nxs";

	public						bool			WildCardRule;
	public						bool			EliminationRule;
	public						bool			OptionalPlayRule;
	public						bool			RefillHandRule;
	public						bool			AllOrNothingRule;
	public						bool			MaxDeviationRule;
	public						bool			LoseBestCardRule;
	public						bool			SeeAICards;
	public						PlayerType[]	Players;
	public						int				NumberOfDecks;
	public						int				NumberOfPointsToWin;
	public						int				MaxDeviationThreshold;
	public						float			TimeScalePercentage;

	public int NumValidPlayers
	{
		get
		{
			int numValidPlayers = 0;
			for (int i = 0, iMax = Players.Length; i < iMax; ++i)
			{
				if (Players[i] != PlayerType.NONE)
				{
					++numValidPlayers;
				}
			}
			return numValidPlayers;
		}
	}

	public GameSettings()
	{
		WildCardRule			= false;
		EliminationRule			= false;
		OptionalPlayRule		= false;
		RefillHandRule			= false;
		AllOrNothingRule		= false;
		MaxDeviationRule		= false;
		LoseBestCardRule		= false;
		SeeAICards				= false;
		Players					= new PlayerType[]
									{
										PlayerType.HUMAN,
										PlayerType.NONE,
										PlayerType.AI_EASY,
										PlayerType.NONE,
									};
		NumberOfDecks			= 2;
		NumberOfPointsToWin		= 5;
		MaxDeviationThreshold	= 3;
		TimeScalePercentage		= (1.0f - MainGameModtroller.MIN_TIMESCALE)
								/ (MainGameModtroller.MAX_TIMESCALE - MainGameModtroller.MIN_TIMESCALE);
	}

	public static GameSettings ReadOldFileFromDisk()
	{
		try
		{
			string settingsFilePath = Application.persistentDataPath + SAVED_SETTINGS_FILE_NAME;
			using (var stream = new FileStream(settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return (GameSettings)new BinaryFormatter().Deserialize(stream);
			}
		}
		catch
		{
			return null;
		}
	}

	public void SetRulesFromOtherGameSettings(GameSettings other)
	{
		WildCardRule			= other.WildCardRule;
		EliminationRule			= other.EliminationRule;
		OptionalPlayRule		= other.OptionalPlayRule;
		RefillHandRule			= other.RefillHandRule;
		AllOrNothingRule		= other.AllOrNothingRule;
		MaxDeviationRule		= other.MaxDeviationRule;
		LoseBestCardRule		= other.LoseBestCardRule;
		SeeAICards				= other.SeeAICards;
		NumberOfDecks			= other.NumberOfDecks;
		NumberOfPointsToWin		= other.NumberOfPointsToWin;
		MaxDeviationThreshold	= other.MaxDeviationThreshold;
	}

	public bool OtherGameSettingsRulesAreEqual(GameSettings other)
	{
		return WildCardRule				== other.WildCardRule
			&& EliminationRule			== other.EliminationRule
			&& OptionalPlayRule			== other.OptionalPlayRule
			&& RefillHandRule			== other.RefillHandRule
			&& AllOrNothingRule			== other.AllOrNothingRule
			&& MaxDeviationRule			== other.MaxDeviationRule
			&& LoseBestCardRule			== other.LoseBestCardRule
			&& SeeAICards				== other.SeeAICards
			&& NumberOfDecks			== other.NumberOfDecks
			&& NumberOfPointsToWin		== other.NumberOfPointsToWin
			&& MaxDeviationThreshold	== other.MaxDeviationThreshold;
	}
}