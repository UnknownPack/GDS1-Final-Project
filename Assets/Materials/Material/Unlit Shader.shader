Shader "Custom/GlitchFlickerSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlitchAmount ("Glitch Amount", Range(0, 1)) = 0.1
        _GlitchSpeed ("Glitch Speed", Range(0, 10)) = 2.0
        _ColorOffset ("Color Offset", Range(0, 1)) = 0.02
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _GlitchAmount;
            float _GlitchSpeed;
            float _ColorOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float rand(float2 co)
            {
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = _Time.y * _GlitchSpeed;
                float2 uv = i.uv;

                float noiseVal = rand(floor(float2(t * 10, uv.y * 100)));
                if (noiseVal < _GlitchAmount)
                {
                    float glitchOffset = (rand(float2(uv.y, t)) - 0.5) * 0.2;
                    uv.x += glitchOffset;
                }

                fixed4 col = tex2D(_MainTex, uv);

                float rOffset = rand(float2(t, uv.y * 23.45)) * _ColorOffset;
                float gOffset = rand(float2(uv.x * 12.98, t)) * _ColorOffset * 2;
                float bOffset = rand(float2(t+1.0, uv.y * 45.67)) * _ColorOffset * 0.5;

                float3 color;
                color.r = tex2D(_MainTex, uv + float2(rOffset, 0)).r;
                color.g = tex2D(_MainTex, uv - float2(gOffset, 0)).g;
                color.b = tex2D(_MainTex, uv + float2(0, bOffset)).b;

                fixed4 finalColor = fixed4(color, col.a);
                return finalColor;
            }
            ENDCG
        }
    }
}
