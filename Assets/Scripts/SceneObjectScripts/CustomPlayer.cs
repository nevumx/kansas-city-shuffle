using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class CustomPlayer : MonoBehaviour
{
	public enum PlayerType
	{
		HUMAN,
		AI_EASY,
		AI_HARD,
		NONE,
	}

	[SerializeField]	private	Toggle		HumanToggle;
	[SerializeField]	private	Toggle		AIToggle;
	[SerializeField]	private	Toggle		NoneToggle;

	[SerializeField]	private	Toggle		EasyAIToggle;
	[SerializeField]	private	Toggle		HardAIToggle;

	[SerializeField]	private	PlayerType	_type;

	public PlayerType Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
			UpdateToggles();
		}
	}

	private void UpdateToggles()
	{
		switch (_type)
		{
		case PlayerType.HUMAN:
			HumanToggle.isOn = true;
			break;
		case PlayerType.AI_EASY:
			EasyAIToggle.isOn = true;
			HardAIToggle.isOn = false;
			AIToggle.isOn = true;
			break;
		case PlayerType.AI_HARD:
			EasyAIToggle.isOn = false;
			HardAIToggle.isOn = true;
			AIToggle.isOn = true;
			break;
		case PlayerType.NONE:
		default:
			NoneToggle.isOn = true;
			break;
		}
		switch (_type)
		{
		case PlayerType.AI_EASY:
		case PlayerType.AI_HARD:
			EasyAIToggle.interactable = true;
			HardAIToggle.interactable = true;
			break;
		case PlayerType.HUMAN:
		case PlayerType.NONE:
		default:
			EasyAIToggle.interactable = false;
			HardAIToggle.interactable = false;
			break;
		}
	}

	public void HumanToggled(bool on)
	{
		if (on)
		{
			Type = PlayerType.HUMAN;
		}
	}

	public void AIToggled(bool on)
	{
		if (on)
		{
			Type = HardAIToggle.isOn ? PlayerType.AI_HARD : PlayerType.AI_EASY;
		}
	}

	public void EasyAIToggled(bool on)
	{
		if (on)
		{
			Type = PlayerType.AI_EASY;
		}
	}

	public void HardAIToggled(bool on)
	{
		if (on)
		{
			Type = PlayerType.AI_HARD;
		}
	}

	public void NoneToggled(bool on)
	{
		if (on)
		{
			Type = PlayerType.NONE;
		}
	}
}
