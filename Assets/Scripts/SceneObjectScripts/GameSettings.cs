using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Nx;

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

	private	static	readonly	string			SAVED_SETTINGS_FILE_NAME	= "/KCSGameSettings.nxs";

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
				if (Players[i] != GameSettings.PlayerType.NONE)
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
		Players					= new PlayerType[4]
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

	public void WriteToDisk()
	{
		string settingsFilePath = Application.persistentDataPath + SAVED_SETTINGS_FILE_NAME;
		var formatter = new BinaryFormatter();
		var stream = new FileStream(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, this);
		stream.Close();
	}

	public static GameSettings ReadFromDisk()
	{
		string settingsFilePath = Application.persistentDataPath + SAVED_SETTINGS_FILE_NAME;
		FileStream stream = null;
		var formatter = new BinaryFormatter();
		GameSettings toReturn;

		try
		{
			stream = new FileStream(settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			toReturn = (GameSettings)formatter.Deserialize(stream);
			stream.Close();
		}
		catch
		{
			stream.IfIsNotNullThen(s => s.Close());
			stream = new FileStream(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
			toReturn = new GameSettings();
			formatter.Serialize(stream, toReturn);
			stream.Close();
		}

		return toReturn;
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