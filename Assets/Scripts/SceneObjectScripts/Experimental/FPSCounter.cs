using UnityEngine;

public class FPSCounter : MonoBehaviour
{
#if NX_DEBUG
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
#endif

	private void OnGUI()
	{
		GUI.skin.label.fontSize = 25;
		if (1.0f / Time.deltaTime < 15.0f)
		{
			GUI.color = Color.red;
		}
		else if (1.0f / Time.deltaTime < 30.0f)
		{
			GUI.color = Color.yellow;
		}
		else
		{
			GUI.color = Color.green;
		}
		GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "FPS: " + Mathf.RoundToInt(1.0f / Time.deltaTime) + " = " + Mathf.RoundToInt(Time.deltaTime * 1000.0f) + "ms");
	}
}
