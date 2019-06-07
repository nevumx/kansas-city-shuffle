using UnityEngine;
using UnityEditor;
using System.IO;

public class ScriptableObjectAssetCreators
{
	[MenuItem("Assets/Create/Add New Graphics Setup Asset...")]
	public static void CreateGraphicsSetupAsset()
	{
		CreateAsset<GraphicsSetup>();
	}

	[MenuItem("Assets/Create/Add New Card Animation Data...")]
	public static void CreateNewCardAnimationData()
	{
		CreateAsset<CardAnimationData>();
	}

	[MenuItem("Assets/Create/Add New Localization Data...")]
	public static void CreateNewLocalizationData()
	{
		CreateAsset<LocalizationData>();
	}

	private static void CreateAsset<T>() where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T>();

		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path == "")
		{
			path = "Assets";
		}
		else if (Path.GetExtension(path) != "")
		{
			path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
		}

		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T) + ".asset");

		AssetDatabase.CreateAsset(asset, assetPathAndName);
		AssetDatabase.SaveAssets();

		EditorUtility.FocusProjectWindow();

		Selection.activeObject = asset;
	}
}