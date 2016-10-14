using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Nx;

public class MainMenuModtroller : MonoBehaviour
{
	public enum Menu : byte
	{
		MAIN_MENU,
		RULESETS,
		CUSTOM_RULESET,
		PLAYER_SETUP,
		HELP1_SCREEN,
		HELP2_SCREEN,
		HELP3_SCREEN,
		HELP4_SCREEN,
		HELP5_SCREEN,
		HELP6_SCREEN,
		ABOUT_SCREEN,
	}

						private	GameObject				_currentMenuCanvas				= null;

	[SerializeField]	private	GameObject				_mainMenuCanvas;
	[SerializeField]	private	GameObject				_rulesetsCanvas;
	[SerializeField]	private	GameObject				_customRulesetCanvas;
	[SerializeField]	private	GameObject				_playerSetupCanvas;
	[SerializeField]	private	GameObject				_help1ScreenCanvas;
	[SerializeField]	private	GameObject				_help2ScreenCanvas;
	[SerializeField]	private	GameObject				_help3ScreenCanvas;
	[SerializeField]	private	GameObject				_help4ScreenCanvas;
	[SerializeField]	private	GameObject				_help5ScreenCanvas;
	[SerializeField]	private	GameObject				_help6ScreenCanvas;
	[SerializeField]	private	GameObject				_aboutScreenCanvas;

	[SerializeField]	private	GameObject				_classicRulesetButtonOutline;
	[SerializeField]	private	GameObject				_advancedRulesetButtonOutline;
	[SerializeField]	private	GameObject				_customRulesetButtonOutline;
						private	GameSettings			_gameSettings;
						public	GameSettings			GameSettings					{ get { return _gameSettings; } }
	[SerializeField]	private	GameSettings			_classicGameSettings;
	[SerializeField]	private	GameSettings			_advancedGameSettings;

	[SerializeField]	private	LocalizationData		_localizationData;

	[SerializeField]	private	AdaptiveTutorialSystem	_tutorialSystem;

	[SerializeField]	private	NxSimpleButton			_customPlayButton;

	[SerializeField]	private	Toggle					_wildCardRuleToggle;
	[SerializeField]	private	Toggle					_eliminationRuleToggle;
	[SerializeField]	private	Toggle					_optionalPlayToggle;
	[SerializeField]	private	Toggle					_refillHandRuleToggle;
	[SerializeField]	private	Toggle					_allOrNothingRuleToggle;
	[SerializeField]	private	Toggle					_maxDeviationRuleToggle;
	[SerializeField]	private	Toggle					_loseBestCardRuleToggle;

	[SerializeField]	private	Slider					_numberOfDecksSlider;
	[SerializeField]	private	Slider					_numberOfPointsToWinSlider;
	[SerializeField]	private	Slider					_maxDeviationThresholdSlider;

	[SerializeField]	private	Text					_numberOfDecksText;
	[SerializeField]	private	Text					_numberOfPointsToWinText;
	[SerializeField]	private	Text					_maxDeviationThresholdText;

	[SerializeField]	private	Text[]					_customPlayerToggleButtonTexts;

	[SerializeField]	private	Text					_rulesetDescriptionText;

	[SerializeField]	private	Image					_logoImage;
	[SerializeField]	private	Image					_blackFadeOutImage;
	[SerializeField]	private	float					_fadeOutTime					= 2.0f;
	[SerializeField]	private	Image					_authorPortrait;
	[SerializeField]	private	Sprite					_authorIOSSprite;

	[NonSerialized]		public	bool					ShouldDestroyShadowsOfNewCards	= false;
	[NonSerialized]		public	bool					ShouldReduceQualityOfNewCards	= false;

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
			case Menu.RULESETS:
				_currentMenuCanvas = _rulesetsCanvas;
				break;
			case Menu.CUSTOM_RULESET:
				_currentMenuCanvas = _customRulesetCanvas;
				break;
			case Menu.PLAYER_SETUP:
				_currentMenuCanvas = _playerSetupCanvas;
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
			case Menu.HELP4_SCREEN:
				_currentMenuCanvas = _help4ScreenCanvas;
				break;
			case Menu.HELP5_SCREEN:
				_currentMenuCanvas = _help5ScreenCanvas;
				break;
			case Menu.HELP6_SCREEN:
				_currentMenuCanvas = _help6ScreenCanvas;
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
#if UNITY_IOS
		_authorPortrait.sprite = _authorIOSSprite;
#endif
	}

	private void Update()
	{
		if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape) && CurrentMenu != Menu.MAIN_MENU)
		{
			if (CurrentMenu == Menu.CUSTOM_RULESET)
			{
				WriteSettingsToDisk();
			}
			CurrentMenu = Menu.MAIN_MENU;
		}

		if (_logoImage.color.a < 1.0f)
		{
			_logoImage.SetAlpha(Mathf.Min(_logoImage.color.a + Time.deltaTime, 1.0f));
		}
		else if (_blackFadeOutImage.color.a > 0.0f)
		{
			_blackFadeOutImage.SetAlpha(Mathf.Max(_blackFadeOutImage.color.a - Time.deltaTime / _fadeOutTime, 0.0f));
		}
	}

	public void OnBackToMainMenuPressed()
	{
		CurrentMenu = Menu.MAIN_MENU;
	}

	public void OnRulesetsPressed()
	{
		CurrentMenu = Menu.RULESETS;
	}

	public void OnCustomRulesetPressed()
	{
		CurrentMenu = Menu.CUSTOM_RULESET;
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

	public void OnHelp4Pressed()
	{
		CurrentMenu = Menu.HELP4_SCREEN;
	}

	public void OnHelp5Pressed()
	{
		CurrentMenu = Menu.HELP5_SCREEN;
	}

	public void OnHelp6Pressed()
	{
		CurrentMenu = Menu.HELP6_SCREEN;
	}

	public void OnAboutPressed()
	{
		CurrentMenu = Menu.ABOUT_SCREEN;
	}

	public void OnPlayerSetupPressed()
	{
		CurrentMenu = Menu.PLAYER_SETUP;
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

	public void OnLoseBestCardRuleChanged(bool newRule)
	{
		_gameSettings.LoseBestCardRule = newRule;
	}

	public void OnNumberOfDecksSliderChanged(float newValue)
	{
		_gameSettings.NumberOfDecks = Mathf.RoundToInt(newValue);
		_numberOfDecksText.text = newValue.ToString();
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
			do
			{
				_gameSettings.Players[playerIndex] = (GameSettings.PlayerType)(((byte)_gameSettings.Players[playerIndex] + 1)
																				% Enum.GetValues(typeof(GameSettings.PlayerType)).Length);
			} while (_gameSettings.NumValidPlayers <= 1);
		}
		DetermineCustomPlayerButtonTexts();
		WriteSettingsToDisk();
	}

	public void OnResetCustomPlayersPressed()
	{
		_gameSettings.Players[0] = GameSettings.PlayerType.HUMAN;
		_gameSettings.Players[1] = GameSettings.PlayerType.NONE;
		_gameSettings.Players[2] = GameSettings.PlayerType.AI_EASY;
		_gameSettings.Players[3] = GameSettings.PlayerType.NONE;
		DetermineCustomPlayerButtonTexts();
		WriteSettingsToDisk();
	}

	public void OnClassicRulesetPressed()
	{
		_gameSettings.SetRulesFromOtherGameSettings(_classicGameSettings);
		WriteSettingsToDisk();
		ReadSettings();
	}

	public void OnAdvancedRulesetPressed()
	{
		_gameSettings.SetRulesFromOtherGameSettings(_advancedGameSettings);
		WriteSettingsToDisk();
		ReadSettings();
	}

	private void DetermineCustomPlayerButtonTexts()
	{
		for (int i = 0, iMax = _customPlayerToggleButtonTexts.Length; i < iMax; ++i)
		{
			switch (_gameSettings.Players[i])
			{
			case GameSettings.PlayerType.HUMAN:
				_customPlayerToggleButtonTexts[i].text = _localizationData.GetLocalizedStringForKey(LocalizationData.TranslationKey.HUMAN);
				break;
			case GameSettings.PlayerType.AI_EASY:
				_customPlayerToggleButtonTexts[i].text = _localizationData.GetLocalizedStringForKey(LocalizationData.TranslationKey.AI_EASY);
				break;
			case GameSettings.PlayerType.AI_HARD:
				_customPlayerToggleButtonTexts[i].text = _localizationData.GetLocalizedStringForKey(LocalizationData.TranslationKey.AI_HARD);
				break;
			case GameSettings.PlayerType.NONE:
			default:
				_customPlayerToggleButtonTexts[i].text = _localizationData.GetLocalizedStringForKey(LocalizationData.TranslationKey.NONE);
				break;
			}
		}
	}

	public void WriteSettingsToDisk()
	{
		_classicRulesetButtonOutline.SetActive(false);
		_advancedRulesetButtonOutline.SetActive(false);
		_customRulesetButtonOutline.SetActive(false);
		if (_gameSettings.OtherGameSettingsRulesAreEqual(_classicGameSettings))
		{
			_classicRulesetButtonOutline.SetActive(true);
			_rulesetDescriptionText.text = _localizationData.GetLocalizedStringForKey(LocalizationData.TranslationKey.CLASSIC_BUTTON_CONTENT);
		}
		else if (_gameSettings.OtherGameSettingsRulesAreEqual(_advancedGameSettings))
		{
			_advancedRulesetButtonOutline.SetActive(true);
			_rulesetDescriptionText.text = _localizationData.GetLocalizedStringForKey(LocalizationData.TranslationKey.ADVANCED_BUTTON_CONTENT);
		}
		else
		{
			_customRulesetButtonOutline.SetActive(true);
			_rulesetDescriptionText.text = _localizationData.GetLocalizedStringForKey(LocalizationData.TranslationKey.CUSTOM_BUTTON_CONTENT);
		}
		_gameSettings.WriteToDisk();
	}

	private void ReadSettings()
	{
		_gameSettings = GameSettings.ReadFromDisk();
		ValidateSettings();
		WriteSettingsToDisk();

		_wildCardRuleToggle.isOn			= _gameSettings.WildCardRule;
		_eliminationRuleToggle.isOn			= _gameSettings.EliminationRule;
		_optionalPlayToggle.isOn			= _gameSettings.OptionalPlayRule;
		_refillHandRuleToggle.isOn			= _gameSettings.RefillHandRule;
		_allOrNothingRuleToggle.isOn		= _gameSettings.AllOrNothingRule;
		_maxDeviationRuleToggle.isOn		= _gameSettings.MaxDeviationRule;
		_loseBestCardRuleToggle.isOn		= _gameSettings.LoseBestCardRule;
		_numberOfDecksSlider.value			= _gameSettings.NumberOfDecks;
		_numberOfPointsToWinSlider.value	= _gameSettings.NumberOfPointsToWin;
		_maxDeviationThresholdSlider.value	= _gameSettings.MaxDeviationThreshold;
		DetermineCustomPlayerButtonTexts();
	}

	public void ValidateSettings()
	{
		if (_gameSettings.NumValidPlayers <= 1)
		{
			_gameSettings = new GameSettings();
		}
	}

	public void PlayGame()
	{
		SceneManager.LoadScene("MainGame");
	}

	public void EmailAuthor()
	{
		Application.OpenURL("mailto:nevumx@gmail.com?subject=KansasCityShuffle");
	}

	public void ResetAllTutorials()
	{
		_tutorialSystem.ResetAllTutorials();
	}

	public void RemoveAllTutorials()
	{
		_tutorialSystem.FinishAllTutorials();
	}
}