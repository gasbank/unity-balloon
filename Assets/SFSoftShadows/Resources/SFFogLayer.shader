Shader "Hidden/SFSoftShadows/FogLayer" {
	Properties {
		_FogColor ("Fog color and alpha.", Color) = (1.0, 1.0, 1.0, 0.0)
		_Scatter ("Light scattering color (RGB), Hard/soft mix (A)", Color) = (1.0, 1.0, 1.0, 0.15)
	}
	
	SubShader {
		Pass {
			Blend One OneMinusSrcAlpha
			Cull Off
			Lighting Off
			ZTest Always
			ZWrite Off
			
			CGPROGRAM
				#include "UnityCG.cginc"
				#pragma vertex VShader
				#pragma fragment FShader

				sampler2D _SFLightMap;
				sampler2D _SFLightMapWithShadows;
				float _SFExposure;
				
				half4 _FogColor;
				half4 _Scatter;

				struct VertexInput {
					float4 position : POSITION;
				};
				
				struct VertexOutput {
					float4 position : SV_POSITION;
					float2 lightCoord : TEXCOORD1;
				};
				
				VertexOutput VShader(VertexInput v){
					// The renderer passes an identity matirx, but Unity may apply "magic" to it.
					// AFAIK, this is the only way to detect the magic projection fixes.
					float4 lightCoord = mul(UNITY_MATRIX_P, v.position);

					VertexOutput o = {v.position, 0.5*lightCoord.xy + 0.5};
					return o;
				}
				
				half4 FShader(VertexOutput v) : SV_Target {
					half3 l0 = tex2D(_SFLightMap, v.lightCoord).rgb;
					half3 l1 = tex2D(_SFLightMapWithShadows, v.lightCoord).rgb;
					half3 scatter = _Scatter.rgb*lerp(l0, l1, _Scatter.a);

					half3 colorRGB = _FogColor.rgb*_FogColor.a + scatter;
					return half4(colorRGB*_SFExposure, _FogColor.a);
				}
			ENDCG
		}
	}
}
