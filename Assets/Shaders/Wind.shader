Shader "Unlit/Wind"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Center ("Center", Float) = 0
        _Color ("Color", Color) = (1,1,1,1)
        _Alpha ("Alpha", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
			"RenderType" = "Transparent"
        }
        LOD 100
        ColorMask RGB
		AlphaTest Greater .01
		ZWrite Off
        Lighting Off
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Center;
            float4 _MainTex_ST;
            float4 _Color;
            float _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                float xweight = abs(i.uv.x - _Center);
                float xweightmid = _Alpha * 3 * (0.5f - abs(i.uv.x - 0.5f));
                float xcomp1 = 0.2f - xweight;
                float ycomp = 0.2f - abs(i.uv.y - 0.5 + sin(i.uv.x*30)/20);
                float xweight2 = abs(i.uv.x - (_Center + 0.9));
                float xcomp2 = 0.2f - abs(0.5 - xweight2);
                col = float4(_Color.r,_Color.g,_Color.b,xweightmid*(xcomp1 + ycomp));
                //col += float4(1.0,1.0,1.0,xweightmid*(xcomp2 + ycomp));
                return col;
            }
            ENDCG
        }
    }
}
