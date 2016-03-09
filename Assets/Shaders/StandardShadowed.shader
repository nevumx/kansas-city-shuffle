Shader "TakkuSum/Standard Shadowed" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_SpecColor("Specular", Color) = (0.2,0.2,0.2)
		_BumpMap("Bumpmap", 2D) = "bump" {}
		//_PatternTex("Game Pattern (a8)", 2D) = "black" {}
		[NoScaleOffset] _ShadowTex("Shadow (RGB)", 2D) = "white" {}
		_ShadowTransform("_ShadowTransform xy:offset zw:scale", Vector) = (0.0, 0.0, 1.0, 1.0)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_DetailAlbedoMap ("Detail Albedo x2", 2D) = "grey" {}
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
		sampler2D _BumpMap;
		sampler2D _ShadowTex;
		half4 _ShadowTransform;
		sampler2D _DetailAlbedoMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv2_DetailAlbedoMap;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {

			half2 shadowCoord = IN.worldPos.xz / _ShadowTransform.zw + _ShadowTransform.xy;
			fixed shadow = tex2D(_ShadowTex, shadowCoord).x; // shadow render target (argb)
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			half3 detailAlbedo = tex2D(_DetailAlbedoMap, IN.uv2_DetailAlbedoMap).rgb;
			c.rgb *= detailAlbedo * unity_ColorSpaceDouble.rgb;

			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			o.Albedo = c.rgb * shadow * _Color.rgb;
			o.Specular = _SpecColor.rgb * LerpWhiteTo(shadow, _Color.a);

			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
