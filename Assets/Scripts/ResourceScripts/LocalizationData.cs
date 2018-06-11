using System;
using System.Linq;
using UnityEngine;
using Nx;

[Serializable]
public class LocalizationData : ScriptableObject
{
	public enum TranslationKey : byte
	{
		QUIT,
		READY,
		HOLD,
		PLAYER_WINS,
		AI_WINS,
		PLAY_AGAIN,
		WILD_CARD_RULE_TOGGLE_TITLE,
		ELIMINATION_RULE_TOGGLE_TITLE,
		OPTIONAL_PLAY_RULE_TOGGLE_TITLE,
		REFILL_HAND_RULE_TOGGLE_TITLE,
		ALL_OR_NOTHING_RULE_TOGGLE_TITLE,
		MAX_DEVIATION_RULE_TOGGLE_TITLE,
		LOSE_BEST_CARD_RULE_TOGGLE_TITLE,
		NUMBER_OF_DECKS_SLIDER_TITLE,
		NUMBER_OF_POINTS_TO_WIN_SLIDER_TITLE,
		MAX_DEVIATION_THRESHOLD_SLIDER_TITLE,
		PLAYER_SETUP_CANVAS_TITLE,
		RULESETS_CANVAS_TITLE,
		ABOUT_CANVAS_TITLE,
		CREDITS_TEXT,
		PHOTO_CREDIT_TEXT,
		HOW_TO_PLAY_PAGE_1_TITLE,
		HOW_TO_PLAY_PAGE_1_CONTENT,
		HOW_TO_PLAY_PAGE_2_TITLE,
		HOW_TO_PLAY_PAGE_2_CONTENT,
		HOW_TO_PLAY_PAGE_3_TITLE,
		HOW_TO_PLAY_PAGE_3_CONTENT,
		HOW_TO_PLAY_PAGE_4_TITLE,
		HOW_TO_PLAY_PAGE_4_CONTENT,
		ACE_ABBREVIATION_CHARACTER,
		JACK_ABBREVIATION_CHARACTER,
		QUEEN_ABBREVIATION_CHARACTER,
		KING_ABBREVIATION_CHARACTER,
		NONE,
		AI_EASY,
		AI_HARD,
		HUMAN,
		RESET_TUTORIALS,
		REMOVE_TUTORIALS,
		MAIN_MENU_BUTTON_LABEL,
		PLAY_BUTTON_LABEL,
		RULESETS_BUTTON_LABEL,
		PLAYER_SETUP_BUTTON_LABEL,
		HOW_TO_PLAY_BUTTON_LABEL,
		ABOUT_BUTTON_LABEL,
		RESET_BUTTON_LABEL,
		NEXT_BUTTON_LABEL,
		PREV_BUTTON_LABEL,
		EMAIL_BUTTON_LABEL,
		CLASSIC_BUTTON_TITLE,
		CLASSIC_BUTTON_CONTENT,
		ADVANCED_BUTTON_TITLE,
		ADVANCED_BUTTON_CONTENT,
		CUSTOM_BUTTON_TITLE,
		CUSTOM_BUTTON_CONTENT,
		ANY_CARD_TUTORIAL,
		UP_CARD_TUTORIAL,
		DOWN_CARD_TUTORIAL,
		NO_CARD_TUTORIAL,
		MULTIPLE_CARD_TUTORIAL,
		OBJECTIVE_TUTORIAL,
		WILD_CARD_TUTORIAL,
		OPTIONAL_PLAY_TUTORIAL,
		MAX_DEVIATION_TUTORIAL,
		HOW_TO_PLAY_PAGE_5_TITLE,
		HOW_TO_PLAY_PAGE_5_CONTENT,
		WIN_ROUND_TUTORIAL,
		HOW_TO_PLAY_PAGE_6_TITLE,
		HOW_TO_PLAY_PAGE_6_CONTENT,
		PULL,
	}

	[Serializable]
	private class Localization
	{
		[Serializable]
		public struct Translation
		{
						public	TranslationKey	StringKey;
			[TextArea]	public	string			TranslatedString;
		}

		public	SystemLanguage	Language		= SystemLanguage.English;
		public	Translation[]	Translations	= null;
	}

	[SerializeField]	private	Localization[]	_localizations;

	public string GetLocalizedStringForKey(TranslationKey stringKey)
	{
		Localization currentLocalization = _localizations.FirstOrDefault(l => l.Language == Application.systemLanguage);
		return (currentLocalization == default(Localization) ? _localizations.First(l => l.Language == SystemLanguage.English)
															 : currentLocalization).Translations.First(t => t.StringKey == stringKey).TranslatedString;
	}
}