Shader "TakkuSum/ZWrite"
{
	SubShader
	{ 
		Tags{"Queue" = "Geometry+1"  "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		Pass
		{
			ZWrite On
			Cull Off
			ColorMask 0

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(0,0,0,0);
			}
			ENDCG
		}
	}
}
