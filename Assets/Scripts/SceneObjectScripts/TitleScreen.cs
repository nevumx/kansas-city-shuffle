using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
	[SerializeField]	private	TweenableGraphics	_whiteFadeOut;
	[SerializeField]	private	TweenableGraphics	_titleText;

	private void Update()
	{
		if (Application.isShowingSplashScreen || _titleText.TweenHolder.enabled || _whiteFadeOut.TweenHolder.enabled)
		{
			return;
		}

		_whiteFadeOut.AddAlphaTween(0.0f).TweenHolder
					 .SetDelay(0.5f)
					 .SetDuration(1.0f)
					 .AddToOnFinishedOnce(() => _titleText.AddAlphaTween(0.0f).TweenHolder
														  .SetDelay(1.5f)
														  .SetDuration(1.0f)
														  .AddToOnFinishedOnce(LoadMainMenu));
	}

	public void LoadMainMenu()
	{
		_whiteFadeOut.RootRectTransform.gameObject.SetActive(false);
		_titleText.RootRectTransform.gameObject.SetActive(false);
		SceneManager.LoadScene("MainMenu");
	}
}