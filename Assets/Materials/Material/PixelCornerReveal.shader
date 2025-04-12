Shader "UI/PixelCornerReveal"
{
    Properties
    {
        _Color("Color", Color) = (0,0,0,1)
        _Progress("Progress", Range(0, 1)) = 0
        _PixelSize("Pixel Size", Range(8, 256)) = 48
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Progress;
            float _PixelSize;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float cornerDistance(float2 uv)
            {
                float2 distTL = uv;
                float2 distTR = float2(1 - uv.x, uv.y);
                float2 distBL = float2(uv.x, 1 - uv.y);
                float2 distBR = float2(1 - uv.x, 1 - uv.y);
                return min(min(length(distTL), length(distTR)), min(length(distBL), length(distBR)));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pixelUV = floor(i.uv * _PixelSize) / _PixelSize;
                float d = cornerDistance(pixelUV);
                float mask = step(d, _Progress);
                return lerp(fixed4(0,0,0,0), _Color, mask);
            }
            ENDCG
        }
    }
}
