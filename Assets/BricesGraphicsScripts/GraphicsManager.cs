using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class GraphicsManager : MonoBehaviour {

	public			GraphicsSetup	GraphicsSetup;

	public			Material		CardBackMaterial;

	private			Texture2D		rampTexture;
	private	const	int				rampWidth = 128;

	void Start()
	{
		createRamp();
		updateShaderParams();
	}

	void Update()
	{
		if (Application.isEditor && !Application.isPlaying)
		{
			createRamp();
			updateShaderParams();
		}
	}

	void createRamp()
	{
		rampTexture = new Texture2D(rampWidth, 2, TextureFormat.ARGB32, false);
		rampTexture.wrapMode = TextureWrapMode.Clamp;

		Shader.SetGlobalTexture("_SkyRamp", rampTexture);

		for (int y = 0; y < rampTexture.height; y++)
		{
			for (int x = 0; x < rampTexture.width; x++)
			{
				rampTexture.SetPixel(x, y, GraphicsSetup.NadirZenithGradient.Evaluate((float)x / (float)rampWidth));
			}
		}

		rampTexture.Apply();
	}

	void updateShaderParams ()
	{
		Shader.SetGlobalColor("_ZenithColor",		GraphicsSetup.ZenithColor);
		Shader.SetGlobalColor("_NadirColor",		GraphicsSetup.BounceFeltColor);
		Shader.SetGlobalColor("_FeltColor",			GraphicsSetup.FeltColor);
		Shader.SetGlobalColor("_PatternColor",		GraphicsSetup.FeltPatternColor);
		Shader.SetGlobalTexture("_PatternTex",		GraphicsSetup.FeltPatternTexture);
		Shader.SetGlobalColor("_CardBackColor0",	GraphicsSetup.CardBackColorBackground);
		Shader.SetGlobalColor("_CardBackColor1",	GraphicsSetup.CardBackColorPattern);

		CardBackMaterial.SetTexture("_MainTex",		GraphicsSetup.CardBackPatternTexture);
	}
}
