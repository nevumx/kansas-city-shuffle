#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class MakeCardAnimationData
{
	[MenuItem("Assets/Create/Add New Card Animation Data...")]
	public static void CreateNewCardAnimationData()
	{
		CardAnimationData asset = ScriptableObject.CreateInstance<CardAnimationData>();

		AssetDatabase.CreateAsset(asset, "Assets/NewCardAnimationData.asset");
		AssetDatabase.SaveAssets();

		EditorUtility.FocusProjectWindow();

		Selection.activeObject = asset;
	}
}
#endif