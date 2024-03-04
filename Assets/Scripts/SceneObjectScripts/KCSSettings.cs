using UnityEngine;

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

[DataContract, Serializable]
public class KCSSettings : IExtensibleDataObject
{
	public enum PlayerType : byte
	{
		HUMAN,
		AI_EASY,
		AI_HARD,
		NONE,
	}

					private	static	readonly	string			SAVED_SETTINGS_FILE_NAME	= "/KCSSettings.nxs";

	[DataMember]	public						bool			WildCardRule;
	[DataMember]	public						bool			EliminationRule;
	[DataMember]	public						bool			OptionalPlayRule;
	[DataMember]	public						bool			RefillHandRule;
	[DataMember]	public						bool			AllOrNothingRule;
	[DataMember]	public						bool			MaxDeviationRule;
	[DataMember]	public						bool			LoseBestCardRule;
	[DataMember]	public						bool			SeeAICards;
	[DataMember]	public						PlayerType[]	Players;
	[DataMember]	public						int				NumberOfDecks;
	[DataMember]	public						int				NumberOfPointsToWin;
	[DataMember]	public						int				MaxDeviationThreshold;
	[DataMember]	public						float			TimeScalePercentage;

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

	
	public KCSSettings()
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

	private ExtensionDataObject extensionDatavalue;

	public ExtensionDataObject ExtensionData
	{
		get { return extensionDatavalue; }
		set { extensionDatavalue = value; }
	}

	private KCSSettings(GameSettings other)
	{
		WildCardRule			= other.WildCardRule;
		EliminationRule			= other.EliminationRule;
		OptionalPlayRule		= other.OptionalPlayRule;
		RefillHandRule			= other.RefillHandRule;
		AllOrNothingRule		= other.AllOrNothingRule;
		MaxDeviationRule		= other.MaxDeviationRule;
		LoseBestCardRule		= other.LoseBestCardRule;
		SeeAICards				= other.SeeAICards;
		Players					= other.Players.Select(p => (PlayerType) p).ToArray();
		NumberOfDecks			= other.NumberOfDecks;
		NumberOfPointsToWin		= other.NumberOfPointsToWin;
		MaxDeviationThreshold	= other.MaxDeviationThreshold;
		TimeScalePercentage		= other.TimeScalePercentage;
	}

	public void SetRulesFromOtherGameSettings(KCSSettings other)
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
	
	public bool OtherGameSettingsRulesAreEqual(KCSSettings other)
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

	public void WriteToDisk()
	{
		string settingsFilePath = Application.persistentDataPath + SAVED_SETTINGS_FILE_NAME;
		var serializer = new DataContractSerializer(typeof(KCSSettings));
		using (var stream = new FileStream(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
		{
			serializer.WriteObject(stream, this);
		}
	}

	public static KCSSettings ReadFromDisk()
	{
		string newSettingsFilePath = Application.persistentDataPath + SAVED_SETTINGS_FILE_NAME;
		string oldSettingsFilePath = Application.persistentDataPath + GameSettings.SAVED_SETTINGS_FILE_NAME;
		if (File.Exists(oldSettingsFilePath))
		{
			if (!File.Exists(newSettingsFilePath))
			{
				var oldGameSettings = GameSettings.ReadOldFileFromDisk();
				if (oldGameSettings != null)
				{
					var newSettings = new KCSSettings(oldGameSettings);
					newSettings.WriteToDisk();
					File.Delete(oldSettingsFilePath);
					return newSettings;
				}
			}
			else
			{
				File.Delete(oldSettingsFilePath);
			}
		}

		try
		{
			var deserializer = new DataContractSerializer(typeof(KCSSettings));
			using (var stream = new FileStream(newSettingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				var newSettings = (KCSSettings) deserializer.ReadObject(stream);
				newSettings.WriteToDisk();
				return newSettings;
			}
		}
		catch
		{
			var newSettings = new KCSSettings();
			newSettings.WriteToDisk();
			return newSettings;
		}
	}
}
