using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Nx;

public class MainMenuModtroller : MonoBehaviour
{
	public enum Menu : byte
	{
		MAIN_MENU,
		SETTINGS,
		CUSTOM_GAME,
		HELP1_SCREEN,
		HELP2_SCREEN,
		HELP3_SCREEN,
		ABOUT_SCREEN,
	}

						const	string						SAVED_SETTINGS_FILE_NAME		= "/TakkuSumSettings.nxs";
						const	float						MAX_TIME_BEFORE_SERVER_EXPIRES	= 6.0f;

						private	GameObject					_currentMenuCanvas				= null;

	[SerializeField]	private	GameObject					_mainMenuCanvas;
	[SerializeField]	private	GameObject					_settingsCanvas;
	[SerializeField]	private	GameObject					_customGameCanvas;
	[SerializeField]	private	GameObject					_help1ScreenCanvas;
	[SerializeField]	private	GameObject					_help2ScreenCanvas;
	[SerializeField]	private	GameObject					_help3ScreenCanvas;
	[SerializeField]	private	GameObject					_aboutScreenCanvas;

						private	GameSettings				_gameSettings;
						public	GameSettings				GameSettings					{ get { return _gameSettings; } }

	[SerializeField]	private	NxSimpleButton				_customPlayButton;

	[SerializeField]	private	Toggle						_wildCardRuleToggle;
	[SerializeField]	private	Toggle						_eliminationRuleToggle;
	[SerializeField]	private	Toggle						_optionalPlayToggle;
	[SerializeField]	private	Toggle						_refillHandRuleToggle;
	[SerializeField]	private	Toggle						_allOrNothingRuleToggle;
	[SerializeField]	private	Toggle						_maxDeviationRuleToggle;
	[SerializeField]	private	Toggle						_dSwitchLBCRuleToggle;
	[SerializeField]	private	Toggle						_seeAICardsToggle;

	[SerializeField]	private	Slider						_numberOfDecksSlider;
	[SerializeField]	private	Slider						_numberOfCardsPerPlayerSlider;
	[SerializeField]	private	Slider						_numberOfPointsToWinSlider;
	[SerializeField]	private	Slider						_maxDeviationThresholdSlider;

	[SerializeField]	private	Text						_numberOfDecksText;
	[SerializeField]	private	Text						_numberOfCardsPerPlayerText;
	[SerializeField]	private	Text						_numberOfPointsToWinText;
	[SerializeField]	private	Text						_maxDeviationThresholdText;

	[SerializeField]	private	Text[]						_customPlayerToggleButtonTexts;

	[SerializeField]	private	Image						_logoImage;
	[SerializeField]	private	Image						_blackFadeOutImage;
	[SerializeField]	private	float						_fadeOutTime					= 2.0f;

						public	bool						ShouldDestroyShadowsOfNewCards	= false;
						public	bool						ShouldReduceQualityOfNewCards	= false;

	private Menu _currentMenu = Menu.MAIN_MENU;
	public Menu CurrentMenu
	{
		get
		{
			return _currentMenu;
		}
		set
		{
			_blackFadeOutImage.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			_logoImage.color = Color.white;
			_currentMenuCanvas.IfIsNotNullThen(c => c.SetActive(false));
			switch (value)
			{
			case Menu.MAIN_MENU:
				_currentMenuCanvas = _mainMenuCanvas;
				break;
			case Menu.SETTINGS:
				_currentMenuCanvas = _settingsCanvas;
				break;
			case Menu.CUSTOM_GAME:
				_currentMenuCanvas = _customGameCanvas;
				break;
			case Menu.HELP1_SCREEN:
				_currentMenuCanvas = _help1ScreenCanvas;
				break;
			case Menu.HELP2_SCREEN:
				_currentMenuCanvas = _help2ScreenCanvas;
				break;
			case Menu.HELP3_SCREEN:
				_currentMenuCanvas = _help3ScreenCanvas;
				break;
			case Menu.ABOUT_SCREEN:
				_currentMenuCanvas = _aboutScreenCanvas;
				break;
			default:
				break;
			}
			_currentMenuCanvas.IfIsNotNullThen(c => c.SetActive(true));
			_currentMenu = value;
		}
	}

	private void Start()
	{
		DontDestroyOnLoad(this.gameObject);
		_currentMenuCanvas = _mainMenuCanvas;
		ReadSettings();
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
	}

	private void Update()
	{
		if (_logoImage.color.a < 1.0f)
		{
			_logoImage.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Min(_logoImage.color.a + Time.deltaTime, 1.0f));
		}
		else if (_blackFadeOutImage.color.a > 0.0f)
		{
			_blackFadeOutImage.color = new Color(0.0f, 0.0f, 0.0f, Mathf.Max(_blackFadeOutImage.color.a - Time.deltaTime / _fadeOutTime, 0.0f));
		}
	}

	public void On1PlayerGamePressed()
	{
		_gameSettings.SetupFor1PlayerGame();
		PlayGame();
	}

	public void OnBackToMainMenuPressed()
	{
		CurrentMenu = Menu.MAIN_MENU;
	}

	public void OnSettingsPressed()
	{
		CurrentMenu = Menu.SETTINGS;
	}

	public void OnHelp1Pressed()
	{
		CurrentMenu = Menu.HELP1_SCREEN;
	}

	public void OnHelp2Pressed()
	{
		CurrentMenu = Menu.HELP2_SCREEN;
	}

	public void OnHelp3Pressed()
	{
		CurrentMenu = Menu.HELP3_SCREEN;
	}

	public void OnAboutPressed()
	{
		CurrentMenu = Menu.ABOUT_SCREEN;
	}

	public void OnCustomGamePressed()
	{
		CurrentMenu = Menu.CUSTOM_GAME;
	}

	public void OnWildCardRuleChanged(bool newRule)
	{
		_gameSettings.WildCardRule = newRule;
	}

	public void OnEliminationRuleChanged(bool newRule)
	{
		_gameSettings.EliminationRule = newRule;
	}

	public void OnOptionalPlayRuleChanged(bool newRule)
	{
		_gameSettings.OptionalPlayRule = newRule;
	}

	public void OnRefillHandRuleChanged(bool newRule)
	{
		_gameSettings.RefillHandRule = newRule;
	}

	public void OnAllOrNothingRuleChanged(bool newRule)
	{
		_gameSettings.AllOrNothingRule = newRule;
	}

	public void OnMaxDeviationRuleChanged(bool newRule)
	{
		_gameSettings.MaxDeviationRule = newRule;
	}

	public void OnDSwitchLBCRuleChanged(bool newRule)
	{
		_gameSettings.DSwitchLBCRule = newRule;
	}

	public void OnSeeAICardsToggleChanged(bool newRule)
	{
		_gameSettings.SeeAICards = newRule;
	}

	public void OnNumberOfDecksSliderChanged(float newValue)
	{
		_gameSettings.NumberOfDecks = Mathf.RoundToInt(newValue);
		_numberOfDecksText.text = newValue.ToString();
	}

	public void OnNumberOfCardsPerPlayerSliderChanged(float newValue)
	{
		_gameSettings.NumberOfCardsPerPlayer = Mathf.RoundToInt(newValue);
		_numberOfCardsPerPlayerText.text = newValue.ToString();
	}

	public void OnNumberOfPointsToWinSliderChanged(float newValue)
	{
		_gameSettings.NumberOfPointsToWin = Mathf.RoundToInt(newValue);
		_numberOfPointsToWinText.text = newValue.ToString();
	}

	public void OnMaxDeviationThresholdSliderChanged(float newValue)
	{
		_gameSettings.MaxDeviationThreshold = Mathf.RoundToInt(newValue);
		_maxDeviationThresholdText.text = newValue.ToString();
	}

	public void OnCustomPlayerCycled(int playerIndex)
	{
		if (_gameSettings.Players.IndexIsValid(playerIndex))
		{
			_gameSettings.Players[playerIndex] = (GameSettings.PlayerType)(((byte)_gameSettings.Players[playerIndex] + 1) % Enum.GetValues(typeof(GameSettings.PlayerType)).Length);
		}
		DetermineCustomPlayerButtonTexts();
		ValidateSettings();
		WriteSettings();
	}

	private void DetermineCustomPlayerButtonTexts()
	{
		for (int i = 0, iMax = _customPlayerToggleButtonTexts.Length; i < iMax; ++i)
		{
			switch (_gameSettings.Players[i])
			{
			case GameSettings.PlayerType.HUMAN:
				_customPlayerToggleButtonTexts[i].text = "Human";
				break;
			case GameSettings.PlayerType.AI_EASY:
				_customPlayerToggleButtonTexts[i].text = "A.I.\nEasy";
				break;
			case GameSettings.PlayerType.AI_HARD:
				_customPlayerToggleButtonTexts[i].text = "A.I.\nHard";
				break;
			case GameSettings.PlayerType.NONE:
			default:
				_customPlayerToggleButtonTexts[i].text = "None";
				break;
			}
		}
	}

	public void OnBackToMainMenuFromSettingsPressed()
	{
		WriteSettings();
		CurrentMenu = Menu.MAIN_MENU;
	}

	public void OnPlayGamePressed()
	{
		PlayGame();
	}

	private void ReadSettings()
	{
		string settingsFilePath = Application.persistentDataPath + SAVED_SETTINGS_FILE_NAME;
		FileStream stream = null;
		var formatter = new BinaryFormatter();

		try
		{
			stream = new FileStream(settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			_gameSettings = (GameSettings) formatter.Deserialize(stream);
			stream.Close();
		}
		catch
		{
			stream.IfIsNotNullThen(s => s.Close());
			stream = new FileStream(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
			_gameSettings = new GameSettings();
			formatter.Serialize(stream, _gameSettings);
			stream.Close();
		}

		_wildCardRuleToggle.isOn = _gameSettings.WildCardRule;
		_eliminationRuleToggle.isOn = _gameSettings.EliminationRule;
		_optionalPlayToggle.isOn = _gameSettings.OptionalPlayRule;
		_refillHandRuleToggle.isOn = _gameSettings.RefillHandRule;
		_allOrNothingRuleToggle.isOn = _gameSettings.AllOrNothingRule;
		_maxDeviationRuleToggle.isOn = _gameSettings.MaxDeviationRule;
		_dSwitchLBCRuleToggle.isOn = _gameSettings.DSwitchLBCRule;
		_seeAICardsToggle.isOn = _gameSettings.SeeAICards;
		_numberOfDecksSlider.value = _gameSettings.NumberOfDecks;
		_numberOfCardsPerPlayerSlider.value = _gameSettings.NumberOfCardsPerPlayer;
		_numberOfPointsToWinSlider.value = _gameSettings.NumberOfPointsToWin;
		_maxDeviationThresholdSlider.value = _gameSettings.MaxDeviationThreshold;
		ValidateSettings();
		DetermineCustomPlayerButtonTexts();
	}

	private void WriteSettings()
	{
		string settingsFilePath = Application.persistentDataPath + SAVED_SETTINGS_FILE_NAME;
		var formatter = new BinaryFormatter();
		var stream = new FileStream(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, _gameSettings);
		stream.Close();
	}

	public void ValidateSettings()
	{
		int numValidPlayers = 0;
		for (int i = 0, iMax = _gameSettings.Players.Length; i < iMax; ++i)
		{
			if (_gameSettings.Players[i] != GameSettings.PlayerType.NONE)
			{
				++numValidPlayers;
			}
		}
		if (numValidPlayers >= 2)
		{
			_customPlayButton.EnableInteraction();
		}
		else
		{
			_customPlayButton.DisableInteraction();
		}
	}

	private void PlayGame()
	{
		SceneManager.LoadScene("MainGame");
	}
}