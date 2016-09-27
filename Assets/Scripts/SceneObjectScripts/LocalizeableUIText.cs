using UnityEngine;
using UnityEngine.UI;

public class LocalizeableUIText : MonoBehaviour
{
	[SerializeField]	private	LocalizationData				_localizationData;
	[SerializeField]	private	Text							_UITextToLocalize;
	[SerializeField]	private	LocalizationData.TranslationKey	_stringKey;

	private void Start()
	{
		_UITextToLocalize.text = _localizationData.GetLocalizedStringForKey(_stringKey);
		Destroy(this);
	}
}
