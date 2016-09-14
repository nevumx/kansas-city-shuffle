using System;
using System.Linq;
using UnityEngine;
using Nx;

[Serializable]
public class LocalizationData : ScriptableObject
{
	[Serializable]
	private class Localization
	{
		[Serializable]
		public struct Translation
		{
						public	string	StringKey;
			[TextArea]	public	string	TranslatedString;
		}

		public	SystemLanguage	Language		= SystemLanguage.English;
		public	Translation[]	Translations	= null;
	}

	[SerializeField]	private	Localization[]	_localizations;

	public string GetLocalizedStringForKey(string stringKey)
	{
		Localization currentLocalization = _localizations.FirstOrDefault(l => l.Language == Application.systemLanguage);
		return (currentLocalization == default(Localization) ? _localizations.First(l => l.Language == SystemLanguage.English)
															 : currentLocalization).Translations.First(t => t.StringKey == stringKey).TranslatedString;
	}
}