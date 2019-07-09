using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Nx;

#pragma warning disable IDE0044 // Add readonly modifier

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

						private			GameObject				_currentMenuCanvas;

	[SerializeField]	private			GameObject				_mainMenuCanvas;
	[SerializeField]	private			GameObject				_rulesetsCanvas;
	[SerializeField]	private			GameObject				_customRulesetCanvas;
	[SerializeField]	private			GameObject				_playerSetupCanvas;
	[SerializeField]	private			GameObject				_help1ScreenCanvas;
	[SerializeField]	private			GameObject				_help2ScreenCanvas;
	[SerializeField]	private			GameObject				_help3ScreenCanvas;
	[SerializeField]	private			GameObject				_help4ScreenCanvas;
	[SerializeField]	private			GameObject				_help5ScreenCanvas;
	[SerializeField]	private			GameObject				_help6ScreenCanvas;
	[SerializeField]	private			GameObject				_aboutScreenCanvas;

	[SerializeField]	private			GameObject				_classicRulesetButtonOutline;
	[SerializeField]	private			GameObject				_advancedRulesetButtonOutline;
	[SerializeField]	private			GameObject				_customRulesetButtonOutline;

						public	static	GameSettings			GameSettings					{ get; private set; }
	[SerializeField]	private			GameSettings			_classicGameSettings;
	[SerializeField]	private			GameSettings			_advancedGameSettings;

	[SerializeField]	private			LocalizationData		_localizationData;

	[SerializeField]	private			AdaptiveTutorialSystem	_tutorialSystem;

	[SerializeField]	private			NxSimpleButton			_customPlayButton;

	[SerializeField]	private			Toggle					_wildCardRuleToggle;
	[SerializeField]	private			Toggle					_eliminationRuleToggle;
	[SerializeField]	private			Toggle					_optionalPlayToggle;
	[SerializeField]	private			Toggle					_refillHandRuleToggle;
	[SerializeField]	private			Toggle					_allOrNothingRuleToggle;
	[SerializeField]	private			Toggle					_maxDeviationRuleToggle;
	[SerializeField]	private			Toggle					_loseBestCardRuleToggle;

	[SerializeField]	private			Slider					_numberOfDecksSlider;
	[SerializeField]	private			Slider					_numberOfPointsToWinSlider;
	[SerializeField]	private			Slider					_maxDeviationThresholdSlider;

	[SerializeField]	private			Text					_numberOfDecksText;
	[SerializeField]	private			Text					_numberOfPointsToWinText;
	[SerializeField]	private			Text					_maxDeviationThresholdText;

	[SerializeField]	private			Text[]					_customPlayerToggleButtonTexts;

	[SerializeField]	private			Text					_rulesetDescriptionText;

	[SerializeField]	private			Image					_logoImage;
	[SerializeField]	private			Image					_blackFadeOutImage;
	[SerializeField]	private			float					_fadeOutTime					= 2.0f;
	[SerializeField]	private			Image					_authorPortrait;
	[SerializeField]	private			Sprite					_authorIOSSprite;

						public	static	bool					ShouldDestroyShadowsOfNewCards;
						public	static	bool					ShouldReduceQualityOfNewCards;

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
			}
			_currentMenuCanvas.IfIsNotNullThen(c => c.SetActive(true));
			_currentMenu = value;
		}
	}

	private void Start()
	{
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
		GameSettings.WildCardRule = newRule;
	}

	public void OnEliminationRuleChanged(bool newRule)
	{
		GameSettings.EliminationRule = newRule;
	}

	public void OnOptionalPlayRuleChanged(bool newRule)
	{
		GameSettings.OptionalPlayRule = newRule;
	}

	public void OnRefillHandRuleChanged(bool newRule)
	{
		GameSettings.RefillHandRule = newRule;
	}

	public void OnAllOrNothingRuleChanged(bool newRule)
	{
		GameSettings.AllOrNothingRule = newRule;
	}

	public void OnMaxDeviationRuleChanged(bool newRule)
	{
		GameSettings.MaxDeviationRule = newRule;
	}

	public void OnLoseBestCardRuleChanged(bool newRule)
	{
		GameSettings.LoseBestCardRule = newRule;
	}

	public void OnNumberOfDecksSliderChanged(float newValue)
	{
		GameSettings.NumberOfDecks = Mathf.RoundToInt(newValue);
		_numberOfDecksText.text = newValue.ToString();
	}

	public void OnNumberOfPointsToWinSliderChanged(float newValue)
	{
		GameSettings.NumberOfPointsToWin = Mathf.RoundToInt(newValue);
		_numberOfPointsToWinText.text = newValue.ToString();
	}

	public void OnMaxDeviationThresholdSliderChanged(float newValue)
	{
		GameSettings.MaxDeviationThreshold = Mathf.RoundToInt(newValue);
		_maxDeviationThresholdText.text = newValue.ToString();
	}

	public void OnCustomPlayerCycled(int playerIndex)
	{
		if (GameSettings.Players.IndexIsValid(playerIndex))
		{
			do
			{
				GameSettings.Players[playerIndex] = (GameSettings.PlayerType)(((byte)GameSettings.Players[playerIndex] + 1)
																				% Enum.GetValues(typeof(GameSettings.PlayerType)).Length);
			} while (GameSettings.NumValidPlayers <= 1);
		}
		DetermineCustomPlayerButtonTexts();
		WriteSettingsToDisk();
	}

	public void OnResetCustomPlayersPressed()
	{
		GameSettings.Players[0] = GameSettings.PlayerType.HUMAN;
		GameSettings.Players[1] = GameSettings.PlayerType.NONE;
		GameSettings.Players[2] = GameSettings.PlayerType.AI_EASY;
		GameSettings.Players[3] = GameSettings.PlayerType.NONE;
		DetermineCustomPlayerButtonTexts();
		WriteSettingsToDisk();
	}

	public void OnClassicRulesetPressed()
	{
		GameSettings.SetRulesFromOtherGameSettings(_classicGameSettings);
		WriteSettingsToDisk();
		ReadSettings();
	}

	public void OnAdvancedRulesetPressed()
	{
		GameSettings.SetRulesFromOtherGameSettings(_advancedGameSettings);
		WriteSettingsToDisk();
		ReadSettings();
	}

	private void DetermineCustomPlayerButtonTexts()
	{
		for (int i = 0, iMax = _customPlayerToggleButtonTexts.Length; i < iMax; ++i)
		{
			switch (GameSettings.Players[i])
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
			default: // case GameSettings.PlayerType.NONE:
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
		if (GameSettings.OtherGameSettingsRulesAreEqual(_classicGameSettings))
		{
			_classicRulesetButtonOutline.SetActive(true);
			_rulesetDescriptionText.text = _localizationData.GetLocalizedStringForKey(LocalizationData.TranslationKey.CLASSIC_BUTTON_CONTENT);
		}
		else if (GameSettings.OtherGameSettingsRulesAreEqual(_advancedGameSettings))
		{
			_advancedRulesetButtonOutline.SetActive(true);
			_rulesetDescriptionText.text = _localizationData.GetLocalizedStringForKey(LocalizationData.TranslationKey.ADVANCED_BUTTON_CONTENT);
		}
		else
		{
			_customRulesetButtonOutline.SetActive(true);
			_rulesetDescriptionText.text = _localizationData.GetLocalizedStringForKey(LocalizationData.TranslationKey.CUSTOM_BUTTON_CONTENT);
		}
		GameSettings.WriteToDisk();
	}

	private void ReadSettings()
	{
		GameSettings = GameSettings.ReadFromDisk();
		if (GameSettings.NumValidPlayers <= 1)
		{
			GameSettings = new GameSettings();
		}
		WriteSettingsToDisk();

		_wildCardRuleToggle.isOn			= GameSettings.WildCardRule;
		_eliminationRuleToggle.isOn			= GameSettings.EliminationRule;
		_optionalPlayToggle.isOn			= GameSettings.OptionalPlayRule;
		_refillHandRuleToggle.isOn			= GameSettings.RefillHandRule;
		_allOrNothingRuleToggle.isOn		= GameSettings.AllOrNothingRule;
		_maxDeviationRuleToggle.isOn		= GameSettings.MaxDeviationRule;
		_loseBestCardRuleToggle.isOn		= GameSettings.LoseBestCardRule;
		_numberOfDecksSlider.value			= GameSettings.NumberOfDecks;
		_numberOfPointsToWinSlider.value	= GameSettings.NumberOfPointsToWin;
		_maxDeviationThresholdSlider.value	= GameSettings.MaxDeviationThreshold;
		DetermineCustomPlayerButtonTexts();
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

#pragma warning restore IDE0044 // Add readonly modifier