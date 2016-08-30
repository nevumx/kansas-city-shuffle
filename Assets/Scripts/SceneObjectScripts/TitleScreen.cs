using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
	[SerializeField]	private	TweenableGraphics	_titleText;

	private void Update()
	{
		if (Application.isShowingSplashScreen || _titleText.TweenHolder.enabled)
		{
			return;
		}

		_titleText.AddIncrementalAlphaTween(1.0f).TweenHolder
				  .SetDelay(0.5f)
				  .SetDuration(1.0f)
				  .AddToOnFinishedOnce(() => _titleText.AddIncrementalAlphaTween(0.0f).TweenHolder
													   .SetDelay(1.5f)
													   .SetDuration(1.0f)
													   .AddToOnFinishedOnce(LoadMainMenu));
	}

	public void LoadMainMenu()
	{
		_titleText.RootRectTransform.gameObject.SetActive(false);
		SceneManager.LoadScene("MainMenu");
	}
}