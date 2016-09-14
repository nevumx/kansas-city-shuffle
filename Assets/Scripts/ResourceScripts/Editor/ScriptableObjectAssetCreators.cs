using UnityEngine;
using UnityEditor;
using System;

public class ScriptableObjectAssetCreators
{
	[MenuItem("Assets/Create/Add New Graphics Setup Asset...")]
	public static void CreateGraphicsSetupAsset()
	{
		CustomAssetUtility.CreateAsset<GraphicsSetup>();
	}

	[MenuItem("Assets/Create/Add New Card Animation Data...")]
	public static void CreateNewCardAnimationData()
	{
		CustomAssetUtility.CreateAsset<CardAnimationData>();
	}

	[MenuItem("Assets/Create/Add New Localization Data...")]
	public static void CreateNewLocalizationData()
	{
		CustomAssetUtility.CreateAsset<LocalizationData>();
	}
}