using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Nx;

public class MainMenuModtroller : MonoBehaviour
{
	public enum Menu
	{
		MAIN_MENU,
		SETTINGS,
		CUSTOM_GAME,
		SERVER_SCREEN,
		CLIENT_SCREEN,
		HELP_SCREEN,
		ABOUT_SCREEN,
	}

						const	string			SAVED_SETTINGS_FILE_NAME	= "/TakkuSumSettings.nxs";

						private	GameObject		_currentMenuCanvas			= null;

	[SerializeField]	private	GameObject		_mainMenuCanvas;
	[SerializeField]	private	GameObject		_settingsCanvas;
	[SerializeField]	private	GameObject		_customGameCanvas;
	[SerializeField]	private	GameObject		_serverScreenCanvas;
	[SerializeField]	private	GameObject		_clientScreenCanvas;
	[SerializeField]	private	GameObject		_helpScreenCanvas;
	[SerializeField]	private	GameObject		_aboutScreenCanvas;

						private	GameSettings	_gameSettings;
						public	GameSettings	GameSettings					{ get { return _gameSettings; } }

	[SerializeField]	private	Button			_customPlayButton;

	[SerializeField]	private	Toggle			_wildCardRuleToggle;
	[SerializeField]	private	Toggle			_eliminationRuleToggle;
	[SerializeField]	private	Toggle			_optionalPlayToggle;
	[SerializeField]	private	Toggle			_refillHandRuleToggle;
	[SerializeField]	private	Toggle			_allOrNothingRuleToggle;
	[SerializeField]	private	Toggle			_maxDeviationRuleToggle;
	[SerializeField]	private	Toggle			_dSwitchLHCRuleToggle;
	[SerializeField]	private	Toggle			_seeAICardsToggle;

	[SerializeField]	private	Slider			_numberOfDecksSlider;
	[SerializeField]	private	Slider			_numberOfValuesPerSuitSlider;
	[SerializeField]	private	Slider			_numberOfCardsPerPlayerSlider;
	[SerializeField]	private	Slider			_numberOfPointsToWinSlider;
	[SerializeField]	private	Slider			_maxDeviationThresholdSlider;

	[SerializeField]	private	Text			_numberOfDecksText;
	[SerializeField]	private	Text			_numberOfValuesPerSuitText;
	[SerializeField]	private	Text			_numberOfCardsPerPlayerText;
	[SerializeField]	private	Text			_numberOfPointsToWinText;
	[SerializeField]	private	Text			_maxDeviationThresholdText;

	[SerializeField]	private	CustomPlayer[]	_customPlayers;

	private Menu _currentMenu = Menu.MAIN_MENU;
	public Menu CurrentMenu
	{
		get
		{
			return _currentMenu;
		}
		set
		{
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
			case Menu.SERVER_SCREEN:
				_currentMenuCanvas = _serverScreenCanvas;
				break;
			case Menu.CLIENT_SCREEN:
				_currentMenuCanvas = _clientScreenCanvas;
				break;
			case Menu.HELP_SCREEN:
				_currentMenuCanvas = _helpScreenCanvas;
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
		CurrentMenu = Menu.MAIN_MENU;
		ReadSettings();
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
	}
	public void On1PlayerGamePressed()
	{
		_gameSettings.SetupFor1PlayerGame();
		PlayGame();
	}

	public void On2PlayerGamePressed()
	{
		_gameSettings.SetupFor2PlayerGame();
		PlayGame();
	}

	public void OnSettingsPressed()
	{
		CurrentMenu = Menu.SETTINGS;
	}

	public void OnHelpPressed()
	{
		CurrentMenu = Menu.HELP_SCREEN;
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

	public void OnDSwitchLHCRuleChanged(bool newRule)
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

	public void OnBackToMainMenuPressed()
	{
		if (!Network.isServer)
		{
			WriteSettings();
		}
		CurrentMenu = Menu.MAIN_MENU;
	}

	public void OnPlayGamePressed()
	{
		if (!Network.isServer)
		{
			WriteSettings();
		}
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

		for (int i = 0, iMax = _customPlayers.Length; i < iMax; ++i)
		{
			_customPlayers[i].Type = _gameSettings.Players[i];
		}

		_wildCardRuleToggle.isOn = _gameSettings.WildCardRule;
		_eliminationRuleToggle.isOn = _gameSettings.EliminationRule;
		_optionalPlayToggle.isOn = _gameSettings.OptionalPlayRule;
		_refillHandRuleToggle.isOn = _gameSettings.RefillHandRule;
		_allOrNothingRuleToggle.isOn = _gameSettings.AllOrNothingRule;
		_maxDeviationRuleToggle.isOn = _gameSettings.MaxDeviationRule;
		_dSwitchLHCRuleToggle.isOn = _gameSettings.DSwitchLBCRule;
		_seeAICardsToggle.isOn = _gameSettings.SeeAICards;
		_numberOfDecksSlider.value = _gameSettings.NumberOfDecks;
		_numberOfCardsPerPlayerSlider.value = _gameSettings.NumberOfCardsPerPlayer;
		_numberOfPointsToWinSlider.value = _gameSettings.NumberOfPointsToWin;
		_maxDeviationThresholdSlider.value = _gameSettings.MaxDeviationThreshold;
		ValidateSettings();
	}

	private void WriteSettings()
	{
		if (Network.isServer)
		{
			return;
		}

		for (int i = 0, iMax = _customPlayers.Length; i < iMax; ++i)
		{
			_gameSettings.Players[i] = _customPlayers[i].Type;
		}

		string settingsFilePath = Application.persistentDataPath + SAVED_SETTINGS_FILE_NAME;
		var formatter = new BinaryFormatter();
		var stream = new FileStream(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, _gameSettings);
		stream.Close();
	}

	public void ValidateSettings()
	{
		int numValidPlayers = 0;
		for (int i = 0, iMax = _customPlayers.Length; i < iMax; ++i)
		{
			if (_customPlayers[i].Type != CustomPlayer.PlayerType.NONE)
			{
				++numValidPlayers;
			}
		}
		_customPlayButton.interactable = numValidPlayers >= 2;
	}

	private void PlayGame()
	{
		SceneManager.LoadScene("MainGame");
	}
}
