﻿Shader "TakkuSum/3D Text Culled" {
	Properties {
		_MainTex ("Font Texture", 2D) = "white" {}
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Lighting Off Cull Back ZWrite Off Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			SetTexture [_MainTex] {
				combine primary, texture * primary
			}
		}
	}
}