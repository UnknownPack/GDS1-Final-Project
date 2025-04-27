Shader "Hidden/Custom/Scanlines"
{
    Properties
    {
        [Toggle] _EnableEffect("Enable Effect", Float) = 1
        _Intensity ("Intensity", Range(0, 1)) = 0.5
        _LineCount ("Line Count", Range(1, 100)) = 10
        _LineColor ("Line Color", Color) = (0, 0, 0, 1)
        _LineWidth ("Line Width", Range(0, 1)) = 0.5
        _ScrollSpeed ("Scroll Speed", Float) = 0
        [Toggle] _EnableFlicker("Enable Flicker", Float) = 0
        _FlickerSpeed("Flicker Speed", Float) = 60
        _FlickerAmount("Flicker Amount", Range(0, 0.5)) = 0.1
        [Toggle] _EnableJitter("Enable Jitter", Float) = 0
        _JitterSpeed("Jitter Speed", Float) = 5
        _JitterAmount("Jitter Amount", Range(0, 0.01)) = 0.002
        [Toggle] _EnablePulsing("Enable Pulsing", Float) = 0
        _PulseSpeed("Pulse Speed", Float) = 3
        _PulseAmount("Pulse Amount", Range(0, 0.5)) = 0.2
        [Toggle] _EnableGhosting("Enable Ghosting", Float) = 0
        _GhostingAmount("Ghosting Amount", Range(0, 0.2)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Scanlines"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_CameraColorTexture);
            SAMPLER(sampler_CameraColorTexture);
            TEXTURE2D(_LastFrameTex);
            SAMPLER(sampler_LastFrameTex);
            
            CBUFFER_START(UnityPerMaterial)
                float _EnableEffect;
                float _Intensity;
                float _LineCount;
                float4 _LineColor;
                float _LineWidth;
                float _ScrollSpeed;
                float _EnableFlicker;
                float _FlickerSpeed;
                float _FlickerAmount;
                float _EnableJitter;
                float _JitterSpeed;
                float _JitterAmount;
                float _EnablePulsing;
                float _PulseSpeed;
                float _PulseAmount;
                float _EnableGhosting;
                float _GhostingAmount;
            CBUFFER_END
            
            float4 frag(Varyings input) : SV_Target
            {
                float4 originalColor = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, input.texcoord);
                
                if (_EnableEffect < 0.5)
                    return originalColor;
                
                float2 uv = input.texcoord;
                
                // Apply vertical jitter
                if (_EnableJitter > 0)
                {
                    float vj = floor(sin(_Time.y * _JitterSpeed) * 2) * _JitterAmount;
                    uv.y += vj;
                }
                
                // Sample current frame
                originalColor = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, uv);
                
                // Apply brightness flicker
                if (_EnableFlicker > 0)
                {
                    float flick = 0.9 + sin(_Time.y * _FlickerSpeed) * _FlickerAmount;
                    originalColor.rgb *= flick;
                }
                
                // Apply ghosting effect
                if (_EnableGhosting > 0)
                {
                    float4 prevFrame = SAMPLE_TEXTURE2D(_LastFrameTex, sampler_LastFrameTex, uv);
                    originalColor = lerp(originalColor, prevFrame, _GhostingAmount);
                }
                
                // Calculate scanline effect with variable thickness
                float dynamicWidth = _LineWidth;
                if (_EnablePulsing > 0)
                {
                    dynamicWidth *= (0.8 + _PulseAmount * sin(_Time.y * _PulseSpeed));
                }
                
                float scanline = frac((uv.y + (_Time.y * _ScrollSpeed)) * _LineCount);
                scanline = smoothstep(dynamicWidth, dynamicWidth + 0.5, scanline);
                
                // Blend the scanline effect with original color
                float4 scanlineColor = lerp(originalColor, originalColor * _LineColor, _Intensity);
                float4 finalColor = lerp(originalColor, scanlineColor, scanline * _Intensity);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}