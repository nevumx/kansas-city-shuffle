Shader "TakkuSum/SkyboxMesh"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Cull Front

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;
			};

			sampler2D _SkyRamp;
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = half2(v.texcoord.y, 0.5);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return tex2D(_SkyRamp, i.uv);
			}
			ENDCG
		}
	}
}
