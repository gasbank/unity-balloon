// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "Custom/Gradient_3Color" {
     Properties {
         [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
         _ColorTop ("Top Color", Color) = (1,1,1,1)
         _ColorMid ("Mid Color", Color) = (1,1,1,1)
         _ColorBot ("Bot Color", Color) = (1,1,1,1)
         _Middle ("Middle", Range(0.001, 0.999)) = 1
     }
 
     SubShader {
         Tags {"Queue"="Background"  "IgnoreProjector"="True"}
         LOD 100
 
         ZWrite On
 
         Pass {
         CGPROGRAM
         #pragma vertex vert  
         #pragma fragment frag
         #include "UnityCG.cginc"
 
         float4 _ColorTop;
         float4 _ColorMid;
         float4 _ColorBot;
         float  _Middle;
 
         struct v2f {
             float4 pos : SV_POSITION;
             float4 texcoord : TEXCOORD0;
         };
 
         v2f vert (appdata_full v) {
             v2f o;
             o.pos = UnityObjectToClipPos (v.vertex);
             o.texcoord = v.texcoord;
             return o;
         }
 
         float4 frag (v2f i) : COLOR {
             float4 c = lerp(_ColorBot, _ColorMid, i.texcoord.y / _Middle) * step(i.texcoord.y, _Middle);
             c += lerp(_ColorMid, _ColorTop, (i.texcoord.y - _Middle) / (1 - _Middle)) * (1 - step(i.texcoord.y, _Middle));
             c.a = 1;
             return c;
         }
         ENDCG
         }
     }
 }