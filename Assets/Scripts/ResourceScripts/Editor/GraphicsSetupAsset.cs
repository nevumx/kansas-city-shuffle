using UnityEngine;
using UnityEditor;
using System;

public class GraphicsSettingsAsset
{
	[MenuItem("Assets/Create/GraphicsSetupAsset")]
	public static void CreateGraphicsSetupAsset()
	{
		CustomAssetUtility.CreateAsset<GraphicsSetup>();
	}

	[MenuItem("Assets/Create/Add New Card Animation Data...")]
	public static void CreateNewCardAnimationData()
	{
		CustomAssetUtility.CreateAsset<CardAnimationData>();
	}
}