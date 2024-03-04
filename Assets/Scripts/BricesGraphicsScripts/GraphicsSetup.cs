using UnityEngine;

public class GraphicsSetup : ScriptableObject
{
	[SerializeField]	private	Gradient	_nadirZenithGradient;
						public	Gradient	NadirZenithGradient		{ get { return _nadirZenithGradient; } }
	[SerializeField]	private	Color		_zenithColor;
						public	Color		ZenithColor				{ get { return _zenithColor; } }
	[SerializeField]	private	Color		_feltPatternColor;
						public	Color		FeltPatternColor		{ get { return _feltPatternColor; } }
	[SerializeField]	private	Color		_feltColor;
						public	Color		FeltColor				{ get { return _feltColor; } }
	[SerializeField]	private	Color		_bounceFeltColor;
						public	Color		BounceFeltColor			{ get { return _bounceFeltColor; } }
	[SerializeField]	private	Texture2D	_feltPatternTexture;
						public	Texture2D	FeltPatternTexture		{ get { return _feltPatternTexture; } }
	[SerializeField]	private	Color		_cardBackColorBackground;
						public	Color		CardBackColorBackground	{ get { return _cardBackColorBackground; } }
	[SerializeField]	private	Color		_cardBackColorPattern;
						public	Color		CardBackColorPattern	{ get { return _cardBackColorPattern; } }
	[SerializeField]	private	Texture2D	_cardBackPatternTexture;
						public	Texture2D	CardBackPatternTexture	{ get { return _cardBackPatternTexture; } }
}