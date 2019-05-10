Shader "Hidden/SFSoftShadows/ShadowMask" {
	SubShader {
		Pass {
			BlendOp RevSub
			Blend One One
			Cull Back
			Lighting Off
			ZTest Always
			ZWrite Off
			
			CGPROGRAM
				#pragma target 3.0
				#pragma vertex VShader
				#pragma fragment FShader

				struct VertexInput {
					float3 properties : POSITION;
					float4 segmentData : TANGENT;
					float2 occluderCoord : TEXCOORD0;
				};
				
				struct FragmentInput {
					float4 position : SV_POSITION;
					float opacity : COLOR;
					float4 penumbras : TEXCOORD0;
					float3 edges : TEXCOORD1;
					float4 lightPosition : TEXCOORD2;
					float4 segmentData : TEXCOORD3;
				};
				
				float2x2 invert2x2(float2 basisX, float2 basisY){
					float2x2 m = float2x2(basisX, basisY);
					return float2x2(m._m11, -m._m10, -m._m01, m._m00)/determinant(m);
				}
				
				void VShader(VertexInput IN, out FragmentInput OUT){
					// Unpack input.
					float radius = IN.properties[0];
					float lightPenetration = IN.properties[1];

					// Woraround for a Unity shader compiler bug. Weee?
					// Direct assignemnt leads to garbage in the frag shader.
					OUT.opacity = 1.00001*IN.properties[2];

					float2 segmentA = IN.segmentData.xy;
					float2 segmentB = IN.segmentData.zw;

					// Determinant of the light matrix to check if it's flipped at all.
					float flip = sign(UNITY_MATRIX_M._m00*UNITY_MATRIX_M._m11 - UNITY_MATRIX_M._m01*UNITY_MATRIX_M._m10);

					// Vertex projection.
					float2 lightOffsetA = flip*float2(-radius,  radius)*normalize(segmentA).yx;
					float2 lightOffsetB = flip*float2( radius, -radius)*normalize(segmentB).yx;
					
					float2 segmentPosition = lerp(segmentA, segmentB, IN.occluderCoord.x);
					float2 projectionOffset = lerp(lightOffsetA, lightOffsetB, IN.occluderCoord.x);
					float4 projected = float4(segmentPosition - projectionOffset*IN.occluderCoord.y, 0.0, 1.0 - IN.occluderCoord.y);
					OUT.position = mul(UNITY_MATRIX_VP, mul(UNITY_MATRIX_M, projected));

					// Penumbras.
					float2 penumbraA = mul(invert2x2(lightOffsetA, segmentA), projected.xy - segmentA*projected.w);
					float2 penumbraB = mul(invert2x2(lightOffsetB, segmentB), projected.xy - segmentB*projected.w);
					OUT.penumbras = (radius > 0.0 ? float4(penumbraA, penumbraB) : float4(0.0, 0.0, 1.0, 1.0));

					// Clipping values.
					float2 segmentDelta = segmentB - segmentA;
					float2 segmentSum = segmentA + segmentB;
					float2 segmentNormal = segmentDelta.yx*float2(-1.0, 1.0);

					OUT.edges.xy = mul(invert2x2(segmentDelta, segmentSum), projected.xy);
					OUT.edges.y *= 2.0;
					OUT.edges.z = flip*dot(segmentNormal, projected.xy - segmentPosition*projected.w);

					// Offsets for light penetration.
					float2x2 M2 = unity_ObjectToWorld;
					OUT.lightPosition = float4(mul(M2, projected.xy), 0.0, projected.w*lightPenetration);
					OUT.segmentData = float4(mul(M2, segmentA), mul(M2, segmentB))/lightPenetration;
				}

				float _SFShadowCompensation;
				
				half4 FShader(FragmentInput IN) : SV_Target {
					// Light penetration.
					float closestT = clamp(IN.edges.x/abs(IN.edges.y), -0.5, 0.5) + 0.5;
					float2 closestP = lerp(IN.segmentData.xy, IN.segmentData.zw, closestT);
					float2 penetration = closestP - IN.lightPosition.xy/IN.lightPosition.w;
					float attenuation = min(pow(dot(penetration, penetration), 0.25), 1.0);

					// Penumbra mixing.
					float2 p = clamp(IN.penumbras.xz/IN.penumbras.yw, -1.0, 1.0);
					float2 values = lerp(p*(3.0 - p*p)*0.25 + 0.5, 1.0, step(IN.penumbras.yw, 0.0));
					float occlusion = _SFShadowCompensation*(values[0] + values[1] - 1.0);
					
					float value = IN.opacity*attenuation*occlusion*step(IN.edges.z, 0.0);
					return half4(0.0, 0.0, 0.0, value);
				}
			ENDCG
		}
	}
}
