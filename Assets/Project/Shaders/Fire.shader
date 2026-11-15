// A burning cartoon flame drawn procedurally on a quad that turns to the camera.
// The quad stands on the object's origin: its local X scale is the flame width, local Y scale the height.
// Three nested teardrops (outer, middle, core) are wobbled by a scrolling noise texture; every flame flickers
// out of step with its neighbours because the phase comes from its world position.
// One material serves every obstacle (SRP Batcher and GPU instancing compatible); _Douse and _BaseColor are
// meant for a MaterialPropertyBlock on the few flames that are being put out.
Shader "SelfBulletting/Fire"
{
    Properties
    {
        [NoScaleOffset] _NoiseMap ("Noise (RGB)", 2D) = "gray" {}
        _BaseColor ("Tint", Color) = (1, 1, 1, 1)

        [Header(Flame)]
        _OuterColor ("Outer", Color) = (0.98, 0.25, 0.06, 1)
        _MiddleColor ("Middle", Color) = (1, 0.56, 0.1, 1)
        _CoreColor ("Core", Color) = (1, 0.93, 0.58, 1)
        _GlowColor ("Glow (A = strength)", Color) = (1, 0.42, 0.1, 0.4)
        _GlowWidth ("Glow Width", Range(0.01, 0.3)) = 0.1

        [Header(Motion)]
        _FlickerSpeed ("Flicker Speed", Float) = 1.4
        _Wobble ("Wobble", Range(0, 1)) = 0.5
        _Sway ("Sway", Range(0, 0.2)) = 0.05
        _Lean ("Lean To Camera", Range(0, 1)) = 0.35

        [Header(Put Out)]
        _Douse ("Douse", Range(0, 1)) = 0
        _DousedOuterColor ("Doused Outer (A = opacity)", Color) = (0.58, 0.77, 0.95, 0.85)
        _DousedMiddleColor ("Doused Middle", Color) = (0.82, 0.92, 1, 1)
        _DousedCoreColor ("Doused Core", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "DisableBatching" = "True"
        }

        Pass
        {
            Name "Fire"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Width of the flame drawing over its height; the quad keeps this aspect at rest.
            #define FLAME_ASPECT 0.7

            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _OuterColor;
                half4 _MiddleColor;
                half4 _CoreColor;
                half4 _GlowColor;
                half _GlowWidth;
                float _FlickerSpeed;
                half _Wobble;
                half _Sway;
                half _Lean;
                half _Douse;
                half4 _DousedOuterColor;
                half4 _DousedMiddleColor;
                half4 _DousedCoreColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float seed : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float4x4 objectToWorld = GetObjectToWorldMatrix();
                float3 pivot = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
                float width = length(objectToWorld._m00_m10_m20);
                float height = length(objectToWorld._m01_m11_m21);

                // Upright like a real flame, tilted a little towards the camera so the top-down view sees more of it.
                float3 cameraRight = UNITY_MATRIX_V[0].xyz;
                float3 cameraUp = UNITY_MATRIX_V[1].xyz;
                float3 up = normalize(lerp(float3(0.0, 1.0, 0.0), cameraUp, _Lean));
                float3 right = normalize(cameraRight - up * dot(cameraRight, up));

                float3 positionWS = pivot
                    + right * ((input.uv.x - 0.5) * width)
                    + up * (input.uv.y * height);

                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                output.seed = frac(dot(pivot.xz, float2(0.3183, 0.7071)));
                return output;
            }

            // Signed distance-like value of a teardrop: a round bottom around centerY and a pointed top at tipY.
            float Teardrop(float2 q, float centerY, float radius, float tipY)
            {
                float bottom = length(q - float2(0.0, centerY)) - radius;

                float t = saturate((q.y - centerY) / (tipY - centerY));
                float halfWidth = radius * pow(max(cos(t * HALF_PI), 0.0), 1.6);
                float top = max(abs(q.x) - halfWidth, q.y - tipY);

                return q.y < centerY ? bottom : top;
            }

            half Fill(float distance)
            {
                return saturate(0.5 - distance / max(fwidth(distance), 1e-4));
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float time = _Time.y * _FlickerSpeed + input.seed * 17.0;
                float2 q = float2((input.uv.x - 0.5) * FLAME_ASPECT, input.uv.y);
                half douse = _Douse;

                // Two noise layers scroll upwards at different rates; the sway bends the whole flame.
                float2 noiseUv = float2(input.uv.x * 0.7 + input.seed, input.uv.y * 0.6 - time * 0.55);
                half3 slow = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, noiseUv).rgb;
                half3 fast = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, noiseUv * 1.7 + float2(0.37, -time * 0.4)).rgb;

                half wobble = _Wobble * (1.0 + douse * 1.5);
                float rise = smoothstep(0.08, 0.9, q.y);
                q.x += (slow.r - 0.5) * wobble * 0.16 * rise;
                q.x += sin(time * 2.3 + input.seed * TWO_PI) * _Sway * q.y * q.y;
                float tongues = (fast.g - 0.5) * wobble * 0.16 * rise;

                float tipScale = lerp(1.0, 0.7, douse);
                float outer = Teardrop(q, 0.24, 0.24, 0.97 * tipScale) + tongues;
                float middle = Teardrop(q, 0.21, 0.165, 0.72 * tipScale) + tongues * 0.8;
                float core = Teardrop(q, 0.17, 0.095, (0.45 + (slow.b - 0.5) * 0.08) * tipScale);

                // The water climbs the flame from its base: below the front the fire has already turned to steam.
                float waterLine = douse * 1.3 - 0.15 + (fast.b - 0.5) * 0.1;
                half doused = saturate((waterLine - q.y) / 0.12 + 0.5);
                half3 outerColor = lerp(_OuterColor.rgb, _DousedOuterColor.rgb, doused);
                half3 middleColor = lerp(_MiddleColor.rgb, _DousedMiddleColor.rgb, doused);
                half3 coreColor = lerp(_CoreColor.rgb, _DousedCoreColor.rgb, doused);

                half outerFill = Fill(outer);
                half3 color = lerp(outerColor, middleColor, Fill(middle));
                color = lerp(color, coreColor, Fill(core));

                // Soft warm halo just outside the flame; it fades away while the fire is put out.
                half glow = saturate(1.0 - outer / _GlowWidth) * _GlowColor.a * (1.0 - douse);
                color = lerp(_GlowColor.rgb, color, outerFill);
                half alpha = max(outerFill * lerp(1.0, _DousedOuterColor.a, doused), glow);

                // The base sinks into the ground and the tip never ends on the quad edge.
                alpha *= smoothstep(0.0, 0.07, q.y) * (1.0 - smoothstep(0.93, 1.0, input.uv.y));

                return half4(color * _BaseColor.rgb, alpha * _BaseColor.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
