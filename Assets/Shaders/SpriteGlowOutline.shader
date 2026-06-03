Shader "BuilderDefender/SpriteGlowOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        [HDR] _GlowColor ("Glow Color", Color) = (0.1, 1.2, 3, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 4)) = 0.5
        [HDR] _OutlineColor ("Outline Color", Color) = (1.8, 1.2, 0.25, 1)
        _OutlineWidth ("Outline Width", Range(0, 8)) = 0
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 0
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "SpriteGlowOutline"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _GlowColor;
                float _GlowIntensity;
                float4 _OutlineColor;
                float _OutlineWidth;
                float _PulseSpeed;
                float _PulseAmount;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            float SampleOutlineAlpha(float2 uv, float2 offset)
            {
                float alpha = 0;
                alpha = max(alpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(offset.x, 0)).a);
                alpha = max(alpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-offset.x, 0)).a);
                alpha = max(alpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, offset.y)).a);
                alpha = max(alpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -offset.y)).a);
                alpha = max(alpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset).a);
                alpha = max(alpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - offset).a);
                alpha = max(alpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(offset.x, -offset.y)).a);
                alpha = max(alpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-offset.x, offset.y)).a);
                return alpha;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float4 spriteColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color * input.color;
                float2 outlineOffset = float2(_OutlineWidth, _OutlineWidth) * 0.002;
                float outlineAlpha = saturate(SampleOutlineAlpha(input.uv, outlineOffset) - spriteColor.a);
                float pulse = 1 + sin(_Time.y * _PulseSpeed) * _PulseAmount;
                float glow = _GlowIntensity * pulse;

                float3 rgb = spriteColor.rgb + _GlowColor.rgb * spriteColor.a * glow;
                rgb = lerp(rgb, _OutlineColor.rgb, outlineAlpha * _OutlineColor.a);

                float alpha = saturate(spriteColor.a + outlineAlpha * _OutlineColor.a);
                return float4(rgb, alpha);
            }
            ENDHLSL
        }
    }
}
