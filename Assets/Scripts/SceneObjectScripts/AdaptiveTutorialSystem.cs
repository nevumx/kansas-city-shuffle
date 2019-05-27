using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Nx;

public class AdaptiveTutorialSystem : MonoBehaviour
{
	public enum TutorialType : byte
	{
		ANY_CARD_TUTORIAL,
		UP_CARD_TUTORIAL,
		DOWN_CARD_TUTORIAL,
		NO_CARD_TUTORIAL,
		MULTIPLE_CARD_TUTORIAL,
		OBJECTIVE_TUTORIAL,
		WILD_CARD_TUTORIAL,
		OPTIONAL_PLAY_TUTORIAL,
		MAX_DEVIATION_TUTORIAL,
		WIN_ROUND_TUTORIAL,
	}

						private	static	readonly	string								SAVED_TUTORIAL_DATA_FILE_NAME		= "/KCSTutorialData.ntd";
						private	static	readonly	int									NUMBER_OF_TIMES_TO_SHOW_TUTORIALS	= 2;
						private	static	readonly	float								TUTORIAL_ALPHA_TRANSISTION_TIME		= 0.75f;

	[SerializeField]	private						MainGameModtroller					_mainGameModtroller;
	[SerializeField]	private						TweenableAlphaMultipliedGraphics	_tutorialGraphics;
	[SerializeField]	private						Text								_tutorialText;
	[SerializeField]	private						LocalizationData					_localizationData;
						private						bool								_isShowingTutorial	= false;
						private						TutorialType?						_lastTutorialType	= null;

						private						int[]								_numberOfTimesTutorialTypeShown;

	private void Awake()
	{
		string settingsFilePath = Application.persistentDataPath + SAVED_TUTORIAL_DATA_FILE_NAME;
		FileStream stream = null;
		var formatter = new BinaryFormatter();
		int numberOfTutorialTypes = Enum.GetValues(typeof(AdaptiveTutorialSystem.TutorialType)).Length;

		try
		{
			stream = new FileStream(settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			_numberOfTimesTutorialTypeShown = (int[])formatter.Deserialize(stream);
			if (_numberOfTimesTutorialTypeShown.Length != numberOfTutorialTypes)
			{
				throw new IndexOutOfRangeException();
			}
			stream.Close();
		}
		catch
		{
			stream.IfIsNotNullThen(s => s.Close());
			stream = new FileStream(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
			_numberOfTimesTutorialTypeShown = new int[numberOfTutorialTypes];
			formatter.Serialize(stream, _numberOfTimesTutorialTypeShown);
			stream.Close();
		}
	}

	public void ResetAllTutorials()
	{
		_numberOfTimesTutorialTypeShown = new int[Enum.GetValues(typeof(AdaptiveTutorialSystem.TutorialType)).Length];
		WriteToDisk();
	}

	public void FinishAllTutorials()
	{
		_numberOfTimesTutorialTypeShown = new int[Enum.GetValues(typeof(AdaptiveTutorialSystem.TutorialType)).Length];
		for (int i = 0, iMax = _numberOfTimesTutorialTypeShown.Length; i < iMax; ++i)
		{
			_numberOfTimesTutorialTypeShown[i] = int.MaxValue;
		}
		WriteToDisk();
	}

	public void StartTutorialIfNecessary(AdaptiveTutorialSystem.TutorialType tutorialType)
	{
		if (!_isShowingTutorial && _numberOfTimesTutorialTypeShown[(int)tutorialType] < NUMBER_OF_TIMES_TO_SHOW_TUTORIALS)
		{
			_tutorialText.text = _localizationData.GetLocalizedStringForKey(GetTranslationKeyEquivalent(tutorialType));
			_isShowingTutorial = true;
			_lastTutorialType = tutorialType;
			if (!_mainGameModtroller.IsShowingHelpPage)
			{
				ShowTutorialIfNecessary();
			}
		}
	}

	public void HideTutorial()
	{
		if (_isShowingTutorial)
		{
			_tutorialGraphics.AddAlphaTween(0.0f).Holder
							 .SetDuration(MainGameModtroller.HELP_SCREEN_TRANSITION_TIME);
		}
	}

	public void ShowTutorialIfNecessary()
	{
		if (_isShowingTutorial)
		{
			_tutorialGraphics.AddAlphaTween(1.0f).Holder
							 .SetDuration(MainGameModtroller.HELP_SCREEN_TRANSITION_TIME);
			if (_lastTutorialType.HasValue)
			{
				++_numberOfTimesTutorialTypeShown[(int)_lastTutorialType];
				WriteToDisk();
				_lastTutorialType = null;
			}
		}
	}

	public void FinishTutorial()
	{
		if (_isShowingTutorial)
		{
			_tutorialGraphics.AddAlphaTween(0.0f).Holder
							 .SetDuration(TUTORIAL_ALPHA_TRANSISTION_TIME);
			_isShowingTutorial = false;
		}
	}

	private void WriteToDisk()
	{
		string settingsFilePath = Application.persistentDataPath + SAVED_TUTORIAL_DATA_FILE_NAME;
		var formatter = new BinaryFormatter();
		var stream = new FileStream(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, _numberOfTimesTutorialTypeShown);
		stream.Close();
	}

	private static LocalizationData.TranslationKey GetTranslationKeyEquivalent(TutorialType tutorialType)
	{
		switch (tutorialType)
		{
			case TutorialType.ANY_CARD_TUTORIAL:
				return LocalizationData.TranslationKey.ANY_CARD_TUTORIAL;
			case TutorialType.UP_CARD_TUTORIAL:
				return LocalizationData.TranslationKey.UP_CARD_TUTORIAL;
			case TutorialType.DOWN_CARD_TUTORIAL:
				return LocalizationData.TranslationKey.DOWN_CARD_TUTORIAL;
			case TutorialType.NO_CARD_TUTORIAL:
				return LocalizationData.TranslationKey.NO_CARD_TUTORIAL;
			case TutorialType.MULTIPLE_CARD_TUTORIAL:
				return LocalizationData.TranslationKey.MULTIPLE_CARD_TUTORIAL;
			case TutorialType.WILD_CARD_TUTORIAL:
				return LocalizationData.TranslationKey.WILD_CARD_TUTORIAL;
			case TutorialType.OPTIONAL_PLAY_TUTORIAL:
				return LocalizationData.TranslationKey.OPTIONAL_PLAY_TUTORIAL;
			case TutorialType.MAX_DEVIATION_TUTORIAL:
				return LocalizationData.TranslationKey.MAX_DEVIATION_TUTORIAL;
			case TutorialType.WIN_ROUND_TUTORIAL:
				return LocalizationData.TranslationKey.WIN_ROUND_TUTORIAL;
			default: // TutorialType.OBJECTIVE_TUTORIAL:
				break;
		}
		return LocalizationData.TranslationKey.OBJECTIVE_TUTORIAL;
	}
}