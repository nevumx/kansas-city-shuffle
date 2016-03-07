using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class NetworkController : MonoBehaviour
{
	private enum Phonetic : byte
	{
		Alpha = 0,
		Bravo = 1,
		Charlie = 2,
		Delta = 3,
		Echo = 4,
		Foxtrot = 5,
		Golf= 6,
		Hotel = 7,
		India = 8,
		Juliet= 9,
		Kilo = 10,
		Lima = 11,
		Mike = 12,
		November = 13,
		Oscar = 14,
		Papa = 15,
	}

	[SerializeField]	private			MainMenuModtroller	_mainMenuModtroller;

	[SerializeField]	private			GameObject			_loadingPopup;
	[SerializeField]	private			Text				_loadingPopupText;
	[SerializeField]	private			GameObject			_errorPopup;
	[SerializeField]	private			Text				_errorPopupText;

						private			List<Phonetic>		_phoneticCode				= null;
	[SerializeField]	private			Text				_phoneticCodeInputText;
	[SerializeField]	private			Text				_phoneticCodeOutputText;
	[SerializeField]	private			Text				_instructionText;
	[SerializeField]	private			Button[]			_phoneticButtons;
	[SerializeField]	private			Button				_backspaceButton;
	[SerializeField]	private			Button				_setupGameButton;

						private	const	int					DEFAULT_INCOMING_PORT		= 2523;

	public void CreateLANGame()
	{
		_phoneticCode = GetPhoneticCodeForIP(Network.player.ipAddress);
		if (Network.HavePublicAddress() || _phoneticCode == null)
		{
			_errorPopupText.text = "Sorry but you cannot play LAN multiplayer at this time.\n" +
				"Either you are not connected to a router or pivate network, or you have an invalid IP.\n" +
				"Please conect to a router or private network and try again.";
			_errorPopup.SetActive(true);
		}
		else
		{
			_loadingPopupText.text = "Initializing...";
			_loadingPopup.SetActive(true);
			Network.InitializeServer(connections: 16, listenPort: DEFAULT_INCOMING_PORT, useNat: false);
		}
	}

	public void JoinLANGame()
	{
		SetupLANScreen();
		_mainMenuModtroller.CurrentMenu = MainMenuModtroller.Menu.CLIENT_SCREEN;
	}

	private void SetupLANScreen()
	{
		_phoneticCode = new List<Phonetic>();
		_instructionText.text = "Enter the host's game code...";
		_phoneticCodeInputText.text = ConvertPhonticCodeToString(_phoneticCode);
		DechiperPhoneticCode();
	}

	public void Disconnect()
	{
		Network.Disconnect();
	}

	public void AddPhoneticCodeCharacter(int newChar)
	{
		_phoneticCode.Add((Phonetic) newChar);
		_phoneticCodeInputText.text = ConvertPhonticCodeToString(_phoneticCode);
		string ip = DechiperPhoneticCode();
		if (ip != null)
		{
			_loadingPopupText.text = "Connecting...";
			_loadingPopup.SetActive(true);
			Network.Connect(ip, DEFAULT_INCOMING_PORT);
		}
	}

	public void RemovePhoneticCodeCharacter()
	{
		_phoneticCode.RemoveAt(_phoneticCode.Count - 1);
		_phoneticCodeInputText.text = ConvertPhonticCodeToString(_phoneticCode);
		DechiperPhoneticCode();
	}

	private void OnConnectedToServer()
	{
		ActivateButtons(0);
		_loadingPopup.SetActive(false);
		_backspaceButton.interactable = false;
		_instructionText.text = "Connected! Waiting for game to start...";
	}

	private void OnFailedToConnect(NetworkConnectionError error)
	{
		_loadingPopup.SetActive(false);
		_errorPopupText.text = "Failed to connect to game.";
		_errorPopup.SetActive(true);
		SetupLANScreen();
	}

	private void OnServerInitialized()
	{
		_loadingPopup.SetActive(false);
		Debug.Log("Server initialized.");
		_phoneticCodeOutputText.text = ConvertPhonticCodeToString(_phoneticCode);
		_setupGameButton.interactable = false;
		_mainMenuModtroller.CurrentMenu = MainMenuModtroller.Menu.SERVER_SCREEN;
	}

	private void OnPlayerConnected(NetworkPlayer player)
	{
		_setupGameButton.interactable = true;
		//TODO: Update Game Setup UI & gameSettings
	}

	private void OnPlayerDisconnected(NetworkPlayer player)
	{
		if (Network.connections.Length <= 0 || (Network.connections.Length == 1 && Network.connections[0] == player))
		{
			_setupGameButton.interactable = false;
			if (_mainMenuModtroller.CurrentMenu == MainMenuModtroller.Menu.CUSTOM_GAME)
			{
				_errorPopupText.text = "All players disconnected...\nPlease try again.";
				_mainMenuModtroller.OnBackToMainMenuPressed();
			}
		}
		//TODO: Update Game Setup UI & gameSettings
	}

	private void OnDisconnectedFromServer(NetworkDisconnection info) {
		if (Network.isServer)
		{
			Debug.Log("Local server connection disconnected");
		}
		else
		{
			_mainMenuModtroller.CurrentMenu = MainMenuModtroller.Menu.MAIN_MENU;
			if (info == NetworkDisconnection.LostConnection)
			{
				Debug.Log("Lost connection to the server");
			}
			else
			{
				Debug.Log("Successfully diconnected from the server");
			}
		}
	}

	private string DechiperPhoneticCode()
	{
		if (_phoneticCode.Count <= 0)
		{
			_backspaceButton.interactable = false;
			ActivateButtons(5);
		}
		else
		{
			_backspaceButton.interactable = true;
			switch (_phoneticCode[0])
			{
			case Phonetic.Alpha: // Class C - 192.168.0.(0-23)
			case Phonetic.Bravo: // Class C - 192.168.1.(0-23)
				if (_phoneticCode.Count == 1)
				{
					ActivateButtons(11);
				}
				else if (_phoneticCode[1] == Phonetic.India // Class C - 192.168.(0,1).(8-15)
				         || _phoneticCode[1] == Phonetic.Juliet) // Class C - 192.168.(0,1).(16-23)
				{
					if (_phoneticCode.Count < 3)
					{
						ActivateButtons(8);
					}
					else
					{
						return GetIPAddressClassC((byte) (_phoneticCode[0] == Phonetic.Alpha ? 0 : 1),
												  (byte) ((_phoneticCode[1] == Phonetic.India ? 8 : 16) + (byte) _phoneticCode[2]));
					}
				}
				else if (_phoneticCode[1] == Phonetic.Kilo) // Class C - 192.168.(0-1).(0-255)
				{
					if (_phoneticCode.Count < 4)
					{
						ActivateButtons(16);
					}
					else
					{
						return GetIPAddressClassC((byte) (_phoneticCode[0] == Phonetic.Alpha ? 0 : 1),
												  (byte) (((byte) _phoneticCode[1] << 4) + (byte) _phoneticCode[2]));
					}
				}
				else
				{
					return GetIPAddressClassC((byte) (_phoneticCode[0] == Phonetic.Alpha ? 0 : 1), (byte) _phoneticCode[1]);
				}
				break;
			case Phonetic.Charlie: // Class C - Arbitrary
				if (_phoneticCode.Count < 5)
				{
					ActivateButtons(16);
				}
				else
				{
					return GetIPAddressClassC((byte) (((byte) _phoneticCode[1] << 4) + (byte) _phoneticCode[2]),
											  (byte) (((byte) _phoneticCode[3] << 4) + (byte) _phoneticCode[4]));
				}
				break;
			case Phonetic.Delta: // Class B - Arbitrary
				if (_phoneticCode.Count < 6)
				{
					ActivateButtons(16);
				}
				else
				{
					return GetIPAddressClassB((byte) ((byte) _phoneticCode[1] + (byte) 16),
											  (byte) (((byte) _phoneticCode[2] << 4) + (byte) _phoneticCode[3]),
											  (byte) (((byte) _phoneticCode[4] << 4) + (byte) _phoneticCode[5]));
				}
				break;
			case Phonetic.Echo: // Class A - Arbitrary
				if (_phoneticCode.Count < 7)
				{
					ActivateButtons(16);
				}
				else
				{
					return GetIPAddressClassA((byte) (((byte) _phoneticCode[1] << 4) + (byte) _phoneticCode[2]),
											  (byte) (((byte) _phoneticCode[3] << 4) + (byte) _phoneticCode[4]),
											  (byte) (((byte) _phoneticCode[5] << 4) + (byte) _phoneticCode[6]));
				}
				break;
			}
		}
		return null;
	}

	private string GetIPAddressClassA(byte b, byte c, byte d)
	{
		return "10." + b + "." + c + "." + d;
	}

	private string GetIPAddressClassB(byte b, byte c, byte d)
	{
		return "172." + b + "." + c + "." + d;
	}

	private string GetIPAddressClassC(byte c, byte d)
	{
		return "192.168." + c + "." + d;
	}

	private List<Phonetic> GetPhoneticCodeForIP(string ip)
	{
		Debug.Log(ip);
		string[] ipStrings = ip.Split('.');
		byte[] ipBytes = new byte[ipStrings.Length];
		var phoneticCode = new List<Phonetic>();
		for (int i = 0, iMax = ipStrings.Length; i < iMax; ++i)
		{
			ipBytes[i] = byte.Parse(ipStrings[i]);
		}
		if (ipBytes[0] == 10)
		{
			phoneticCode.Add(Phonetic.Echo);
			phoneticCode.Add((Phonetic) ((ipBytes[1] & 0xf0) >> 4));
			phoneticCode.Add((Phonetic) (ipBytes[1] & 0x0f));
			phoneticCode.Add((Phonetic) ((ipBytes[2] & 0xf0) >> 4));
			phoneticCode.Add((Phonetic) (ipBytes[2] & 0x0f));
			phoneticCode.Add((Phonetic) ((ipBytes[3] & 0xf0) >> 4));
			phoneticCode.Add((Phonetic) (ipBytes[3] & 0x0f));
			return phoneticCode;
		}
		else if (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] < 32)
		{
			phoneticCode.Add(Phonetic.Delta);
			phoneticCode.Add((Phonetic) ((ipBytes[1] - 16)));
			phoneticCode.Add((Phonetic) ((ipBytes[2] & 0xf0) >> 4));
			phoneticCode.Add((Phonetic) (ipBytes[2] & 0x0f));
			phoneticCode.Add((Phonetic) ((ipBytes[3] & 0xf0) >> 4));
			phoneticCode.Add((Phonetic) (ipBytes[3] & 0x0f));
			return phoneticCode;
		}
		else if (ipBytes[0] == 192 && ipBytes[1] == 168)
		{
			if (ipBytes[2] <= 1)
			{
				phoneticCode.Add(ipBytes[2] == 0 ? Phonetic.Alpha : Phonetic.Bravo);
				if (ipBytes[3] < 8)
				{
					phoneticCode.Add((Phonetic) (ipBytes[3] & 0x0f));
					return phoneticCode;
				}
				else if (ipBytes[3] < 16)
				{
					phoneticCode.Add(Phonetic.India);
					phoneticCode.Add((Phonetic) ((ipBytes[3] - 8) & 0x0f));
					return phoneticCode;
				}
				else if (ipBytes[3] < 24)
				{
					phoneticCode.Add(Phonetic.Juliet);
					phoneticCode.Add((Phonetic) ((ipBytes[3] - 16) & 0x0f));
					return phoneticCode;
				}
				phoneticCode.Add(Phonetic.Kilo);
				phoneticCode.Add((Phonetic) ((ipBytes[3] & 0xf0) >> 4));
				phoneticCode.Add((Phonetic) (ipBytes[3] & 0x0f));
				return phoneticCode;
			}
			phoneticCode.Add(Phonetic.Charlie);
			phoneticCode.Add((Phonetic) ((ipBytes[2] & 0xf0) >> 4));
			phoneticCode.Add((Phonetic) (ipBytes[2] & 0x0f));
			phoneticCode.Add((Phonetic) ((ipBytes[3] & 0xf0) >> 4));
			phoneticCode.Add((Phonetic) (ipBytes[3] & 0x0f));
			return phoneticCode;
		}
		return null;
	}

	private void ActivateButtons(int upToExclusive)
	{
		for (int i = 0, iMax = _phoneticButtons.Length; i < iMax; ++i)
		{
			_phoneticButtons[i].interactable = i < upToExclusive;
		}
	}

	private string ConvertPhonticCodeToString(List<Phonetic> phoneticCode)
	{
		string phoneticCodeString = phoneticCode.Count > 0 ? phoneticCode[0].ToString() : "";
		for (int i = 1, iMax = phoneticCode.Count; i < iMax; ++i)
		{
			phoneticCodeString += " - " + phoneticCode[i].ToString();
		}
		return phoneticCodeString;
	}
}