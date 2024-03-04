using UnityEngine;

#pragma warning disable IDE1006 // Naming Styles

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Blur/Blur")]
public class Blur : MonoBehaviour
{
	[Range(0, 10)]
	[SerializeField]
	private			int			iterations	= 3;

	[SerializeField]
	[Range(0.0f, 1.0f)]
	private			float		blurSpread	= 0.6f;

	[SerializeField]
	private			Shader		blurShader;

	private	static	Material	_staticMaterial;
	private Material _material {
		get
		{
			if (_staticMaterial == null)
			{
				_staticMaterial = new Material(blurShader)
				{
					hideFlags = HideFlags.DontSave
				};
			}
			return _staticMaterial;
		}
	}

	private void OnDisable()
	{
		if (_staticMaterial)
		{
			DestroyImmediate(_staticMaterial);
		}
	}

	private void Start()
	{
		if (!blurShader || !_material.shader.isSupported)
		{
			enabled = false;
		}
	}

	private void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		float off = 0.5f + iteration * blurSpread;
		Graphics.BlitMultiTap (source, dest, _material,
			new Vector2(-off, -off), new Vector2(-off, off),
			new Vector2( off, off), new Vector2( off, -off));
	}

	private void DownSample4x(RenderTexture source, RenderTexture dest)
	{
		Graphics.BlitMultiTap(source, dest, _material,
			-Vector2.one, new Vector2(-1.0f, 1.0f),
			Vector2.one, new Vector2( 1.0f, -1.0f));
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		int rtW = source.width / 4;
		int rtH = source.height / 4;
		RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);

		DownSample4x(source, buffer);

		for (int i = 0; i < iterations; ++i)
		{
			RenderTexture buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
			FourTapCone (buffer, buffer2, i);
			RenderTexture.ReleaseTemporary(buffer);
			buffer = buffer2;
		}
		Graphics.Blit(buffer, destination);

		RenderTexture.ReleaseTemporary(buffer);
	}
}

#pragma warning restore IDE1006 // Naming Styles