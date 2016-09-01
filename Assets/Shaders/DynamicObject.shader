Shader "TakkuSum/DynamicObject"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Sprite Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Pass
		{
			Cull Back

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed4 vc : TEXCOORD0;
				half2 texcoord : TEXCOORD1;
			};

			fixed4 _CardBackColor0;
			fixed4 _CardBackColor1;
			fixed4 _NadirColor;
			fixed4 _ZenithColor;

			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				half3 worldNormal = mul(unity_ObjectToWorld, half4(normalize(v.normal), 0));
				half l = (worldNormal.y * 0.5 + 0.5);
				o.vc = lerp(_NadirColor, _ZenithColor, l);
				o.texcoord = v.texcoord;
				return o;
			}

			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{
				//return tex2D(_CardBackPatternTex, i.texcoord) * i.vc;
				fixed s = tex2D(_MainTex, i.texcoord).a;
				return lerp(_CardBackColor1, _CardBackColor0, s) * i.vc;
			}
			ENDCG
		}
	}
}
