using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
	[SerializeField]	private	Text		_titleText;
	[SerializeField]	private	float		_timeUntilStart	= 3.0f;
						private float		_timeObjectStarted;

	private void Start()
	{
		_timeObjectStarted = Time.time;
	}

	private void Update()
	{
		if (Application.isShowingSplashScreen)
		{
			_timeObjectStarted = Time.time;
			return;
		}

		if (Time.time < _timeObjectStarted + 1.0f)
		{
			_titleText.color = new Color(1.0f, 1.0f, 1.0f, (Time.time - _timeObjectStarted));
		}
		else if (Time.time <= _timeObjectStarted + _timeUntilStart - 1.0f)
		{
			_titleText.color = Color.white;
		}
		else if (Time.time < _timeObjectStarted + _timeUntilStart)
		{
			_titleText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f - (Time.time - _timeObjectStarted - _timeUntilStart + 1.0f));
		}
		else
		{
			LoadMainMenu();
		}
	}

	public void LoadMainMenu()
	{
		_titleText.gameObject.SetActive(false);
		SceneManager.LoadScene("MainMenu");
	}
}
