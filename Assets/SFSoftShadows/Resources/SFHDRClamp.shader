Shader "Hidden/SFSoftShadows/HDRClamp" {
	SubShader {
		Pass {
			BlendOp Max
			ColorMask A
			Blend One One
			Cull Off
			Lighting Off
			ZTest Always
			ZWrite Off
			
			CGPROGRAM
				#pragma vertex VShader
				#pragma fragment FShader

				float4 VShader(float4 vertex : POSITION) : SV_POSITION {return vertex;}
				half4 FShader(void) : SV_Target {return 0;}
			ENDCG
		}
	}
}
