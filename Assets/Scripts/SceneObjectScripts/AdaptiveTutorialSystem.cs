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
		WILD_CARD_TUTORIAL,
		OPTIONAL_PLAY_TUTORIAL,
		MAX_DEVIATION_TUTORIAL,
	}

	[Serializable]
	private struct TutorialData
	{
		public	int[]	_numberOfTimesTutorialTypeShown;
		public	bool	_holdForMoreOptionsTextHasBeenShown;
	}

						private	static	readonly	string				SAVED_TUTORIAL_DATA_FILE_NAME		= "/KCSTutorialData.ntd";
						private	static	readonly	int					NUMBER_OF_TIMES_TO_SHOW_TUTORIALS	= 2;
						private	static	readonly	float				TUTORIAL_ALPHA_TRANSISTION_TIME		= 1.0f;


	[SerializeField]	private						TweenableGraphics	_tutorialGraphics;
	[SerializeField]	private						Text				_tutorialText;
	[SerializeField]	private						LocalizationData	_localizationData;
						private						bool				_isShowingTutorial					= false;

						private						TutorialData		_tutorialData;

	public bool HoldForMoreOptionsTextHasBeenShown
	{
		get
		{
			return _tutorialData._holdForMoreOptionsTextHasBeenShown;
		}
		set
		{
			_tutorialData._holdForMoreOptionsTextHasBeenShown = value;
			WriteToDisk();
		}
	}

	private void Awake()
	{
		string settingsFilePath = Application.persistentDataPath + SAVED_TUTORIAL_DATA_FILE_NAME;
		FileStream stream = null;
		var formatter = new BinaryFormatter();

		try
		{
			stream = new FileStream(settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			_tutorialData = (TutorialData)formatter.Deserialize(stream);
			stream.Close();
		}
		catch
		{
			stream.IfIsNotNullThen(s => s.Close());
			stream = new FileStream(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
			_tutorialData._numberOfTimesTutorialTypeShown = new int[Enum.GetValues(typeof(TutorialType)).Length];
			formatter.Serialize(stream, _tutorialData);
			stream.Close();
		}
	}

	public void ResetAllTutorials()
	{
		_tutorialData = new TutorialData();
		_tutorialData._numberOfTimesTutorialTypeShown = new int[Enum.GetValues(typeof(TutorialType)).Length];
		WriteToDisk();
	}

	public void FinishAllTutorials()
	{
		_tutorialData._numberOfTimesTutorialTypeShown = new int[Enum.GetValues(typeof(TutorialType)).Length];
		for (int i = 0, iMax = _tutorialData._numberOfTimesTutorialTypeShown.Length; i < iMax; ++i)
		{
			_tutorialData._numberOfTimesTutorialTypeShown[i] = int.MaxValue;
		}
		_tutorialData._holdForMoreOptionsTextHasBeenShown = true;
		WriteToDisk();
	}

	public void ShowTutorialIfNecessary(TutorialType tutorialType)
	{
		if (!_isShowingTutorial && _tutorialData._numberOfTimesTutorialTypeShown[(int)tutorialType] < NUMBER_OF_TIMES_TO_SHOW_TUTORIALS)
		{
			_tutorialText.text = _localizationData.GetLocalizedStringForKey(tutorialType.ToString());
			_tutorialGraphics.AddAlphaTween(1.0f).TweenHolder
							 .SetDuration(TUTORIAL_ALPHA_TRANSISTION_TIME);
			_isShowingTutorial = true;
			++_tutorialData._numberOfTimesTutorialTypeShown[(int)tutorialType];
			WriteToDisk();
		}
	}

	public void HideTutorial()
	{
		if (_isShowingTutorial)
		{
			_tutorialGraphics.AddAlphaTween(0.0f).TweenHolder
							 .SetDuration(TUTORIAL_ALPHA_TRANSISTION_TIME);
			_isShowingTutorial = false;
		}
	}

	private void WriteToDisk()
	{
		string settingsFilePath = Application.persistentDataPath + SAVED_TUTORIAL_DATA_FILE_NAME;
		var formatter = new BinaryFormatter();
		var stream = new FileStream(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, _tutorialData);
		stream.Close();
	}
}