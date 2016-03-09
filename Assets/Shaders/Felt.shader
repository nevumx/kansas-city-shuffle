Shader "TakkuSum/Felt" {
	Properties {
		_SpecularColor ("Specular Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		//_PatternTex("Game Pattern (a8)", 2D) = "black" {}
		[NoScaleOffset] _ShadowTex("Shadow (RGB)", 2D) = "white" {}
		_ShadowTransform("_ShadowTransform xy:offset zw:scale", Vector) = (0.0, 0.0, 1.0, 1.0)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular fullforwardshadows 
			//noambient

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		//#pragma exclude_renderers d3d11

		sampler2D _MainTex;
		sampler2D _ShadowTex;
		half4 _ShadowTransform;
		sampler2D _PatternTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		//fixed4 _Color;
		fixed4 _FeltColor;
		fixed4 _SpecularColor;
		fixed4 _PatternColor;

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			half2 shadowCoord = IN.worldPos.xz / _ShadowTransform.zw + _ShadowTransform.xy;

			fixed shadow = tex2D(_ShadowTex, shadowCoord).x; // shadow render target (argb)
			fixed pattern = tex2D(_PatternTex, IN.uv_MainTex).a; // felt game pattern (alpha8)
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _FeltColor;
			
			o.Albedo = lerp(c.rgb, _PatternColor.rgb, pattern) * shadow;
			o.Specular = _SpecularColor.rgb;

			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
