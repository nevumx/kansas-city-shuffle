Shader "TakkuSum/ShadowObject"
{
	Properties
	{
		_Max ("Max", Range(0, 1)) = 1
		_Fade ("Fade", Range(0, 10)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Pass
		{
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half vc : TEXCOORD0;
			};

			half _Max;
			half _Fade;

			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vc = saturate(_Max + mul(unity_ObjectToWorld, v.vertex).y * _Fade);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.vc.xxxx;
			}
			ENDCG
		}
	}
}
