using UnityEngine;
using System;
using System.Collections.Generic;
using Nx;


public class EasterEggListener : MonoBehaviour
{
	private	static	readonly	int[]		EASTER_EGG_CODE				= {4, 2, 3, 1};
	private	static	readonly	int[]		EASTER_EGG_CODE_ALT			= {1, 2, 3, 4};
	private	static	readonly	string		CHANGE_BLACK_CARD_VALUES	= "ChangeBlackCardValues";
	private	static	readonly	string		CHANGE_RED_CARD_VALUES		= "ChangeRedCardValues";
	private						Queue<int>	_numbersEntered				= new Queue<int>();
	private						int			_previousTouchCount			= 0;
	private						int			_maxTouchCount				= 1;

	private void Update()
	{
		if (Input.touchCount > 0)
		{
			_maxTouchCount = Mathf.Max(_maxTouchCount, Input.touchCount);
		}
		else if (_previousTouchCount > 0)
		{
			_numbersEntered.Enqueue(_maxTouchCount);
			_maxTouchCount = 1;
			if (_numbersEntered.Count > EASTER_EGG_CODE.Length + 12)
			{
				_numbersEntered.Dequeue();
			}

			int[] numbersEnteredArray = _numbersEntered.ToArray();
			string cardBackColorShaderVariableName = null;

			if (numbersEnteredArray.Length == EASTER_EGG_CODE.Length + 12)
			{
				int[] easterEggCodeReversed = new int[EASTER_EGG_CODE.Length];
				int[] easterEggCodeAltReversed = new int[EASTER_EGG_CODE.Length];
				Array.Copy(EASTER_EGG_CODE, easterEggCodeReversed, EASTER_EGG_CODE.Length);
				Array.Copy(EASTER_EGG_CODE_ALT, easterEggCodeAltReversed, EASTER_EGG_CODE_ALT.Length);
				Array.Reverse(easterEggCodeReversed);
				Array.Reverse(easterEggCodeAltReversed);

				if (numbersEnteredArray.StartsWith(EASTER_EGG_CODE))
				{
					cardBackColorShaderVariableName = "_CardBackColor1";
				}
				else if (numbersEnteredArray.StartsWith(easterEggCodeReversed))
				{
					cardBackColorShaderVariableName = "_CardBackColor0";
				}
				else if (numbersEnteredArray.StartsWith(EASTER_EGG_CODE_ALT))
				{
					cardBackColorShaderVariableName = CHANGE_BLACK_CARD_VALUES;
				}
				else if (numbersEnteredArray.StartsWith(easterEggCodeAltReversed))
				{
					cardBackColorShaderVariableName = CHANGE_RED_CARD_VALUES;
				}
				else
				{
					goto updatePrevious;
				}

				byte[] translatedColorData = new byte[3];

				for (int i = 0, iMax = translatedColorData.Length; i < iMax; ++i)
				{
					for (int j = 0, jMax = 4; j < 4; ++j)
					{
						translatedColorData[i] |= (byte)((numbersEnteredArray[EASTER_EGG_CODE.Length + j + i * jMax] - 1) << (2 * (jMax - j - 1)));
					}
				}

				Color translatedColor = new Color(translatedColorData[0] / 255.0f, translatedColorData[1] / 255.0f, translatedColorData[2] / 255.0f);

				if (cardBackColorShaderVariableName.Equals(CHANGE_BLACK_CARD_VALUES))
				{
					CardModViewtroller.BlackTextColor = translatedColor;
				}
				else if (cardBackColorShaderVariableName.Equals(CHANGE_RED_CARD_VALUES))
				{
					CardModViewtroller.RedTextColor = translatedColor;
				}
				else
				{
					Shader.SetGlobalColor(cardBackColorShaderVariableName, translatedColor);
				}

				_numbersEntered.Clear();
			}
		}

		updatePrevious:
		_previousTouchCount = Input.touchCount;
	}
}
