﻿Shader "Nx/FadeOut"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FadeAmt ("Fade Amount", Range(0.5, 0.0001)) = 0.02
	}

	SubShader
	{
		ZWrite Off

		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				half _FadeAmt;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					return o;
				}

				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.texcoord);
					col.a -= _FadeAmt;
					return col;
				}
			ENDCG
		}
	}
}