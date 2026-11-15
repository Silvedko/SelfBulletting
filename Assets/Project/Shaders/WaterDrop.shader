// Translucent cartoon water for the player ball and the shot, made for Unity's sphere mesh (radius 0.5).
// One half of the sphere stays exactly as it is, so the drop is as wide as the gameplay sphere and touches the
// ground at the same point. The other half is pulled into a pointed tip. The tip leans from straight up towards
// the camera, so the top-down view sees the drop's outline instead of looking down its point.
// The whole drop wobbles like jelly around its bottom. _BaseColor can be overridden by a MaterialPropertyBlock.
Shader "SelfBulletting/Water Drop"
{
    Properties
    {
        _BaseColor ("Water (A = opacity in the middle)", Color) = (0.1, 0.48, 0.97, 0.72)
        _RimColor ("Rim", Color) = (0.7, 0.92, 1, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.5
        _GlowColor ("Light Through The Drop", Color) = (0.55, 0.9, 1, 1)

        [Header(Highlights)]
        _SpecularColor ("Specular", Color) = (1, 1, 1, 1)
        _Shininess ("Shininess", Range(8, 512)) = 160
        _HighlightPosition ("Highlight (view XY)", Vector) = (-0.34, 0.3, 0, 0)
        _HighlightSize ("Highlight Size", Range(0, 0.5)) = 0.2

        [Header(Shape)]
        _DropShape ("Drop Shape", Range(0, 1)) = 1
        _TipLength ("Tip Length", Range(0, 1.5)) = 0.75
        _TipLean ("Tip Lean To Camera", Range(0, 1)) = 0.8
        _WobbleAmplitude ("Wobble", Range(0, 0.2)) = 0.04
        _WobbleSpeed ("Wobble Speed", Float) = 5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _RimColor;
            half _RimPower;
            half4 _GlowColor;
            half4 _SpecularColor;
            half _Shininess;
            float4 _HighlightPosition;
            half _HighlightSize;
            half _DropShape;
            half _TipLength;
            half _TipLean;
            half _WobbleAmplitude;
            float _WobbleSpeed;
        CBUFFER_END

        // Moves a point of the unit-diameter sphere onto the drop and gives the drop's normal there.
        // tipAxisOS is the unit direction of the tip in object space; tipWeight is 0 on the round half, 1 at the tip.
        void DeformToDrop(float3 positionOS, float3 tipAxisOS, out float3 dropPositionOS, out float3 dropNormalOS, out float tipWeight)
        {
            float3 direction = normalize(positionOS);
            float s = dot(direction, tipAxisOS);
            float3 across = direction - tipAxisOS * s;
            float c = length(across);

            dropPositionOS = positionOS;
            dropNormalOS = direction;
            tipWeight = saturate(s);

            if (s > 0.0)
            {
                // Profile of the tip half by latitude: radius c * k and height s * stretch.
                // k bends the dome into a tip whose sides meet at an angle instead of rounding off.
                float m = _DropShape;
                float stretch = 1.0 + _TipLength * m;
                float g = c * (c + s * s);
                float k = 1.0 - m + m * g;
                dropPositionOS = 0.5 * (across * k + tipAxisOS * (s * stretch));

                float slopeOfG = s * (2.0 * c * c - 2.0 * c - s * s);
                float slopeOfRadius = -s * k + c * m * slopeOfG;
                float slopeOfHeight = c * stretch;
                float2 profileNormal = normalize(float2(slopeOfHeight, -slopeOfRadius) + float2(0.0, 1e-5));
                float3 around = c > 1e-4 ? across / c : float3(0.0, 0.0, 0.0);
                dropNormalOS = around * profileNormal.x + tipAxisOS * profileNormal.y;
            }

            // Jelly wobble: squash and stretch around the bottom point, plus a small ripple over the surface.
            const float bottom = -0.5;
            float time = _Time.y * _WobbleSpeed;
            float squash = sin(time) * _WobbleAmplitude;
            dropPositionOS.y = bottom + (dropPositionOS.y - bottom) * (1.0 + squash);
            dropPositionOS.xz *= 1.0 - squash * 0.5;
            float ripple = sin(time * 1.7 + positionOS.y * 9.0 + positionOS.x * 6.0 + positionOS.z * 4.0);
            dropPositionOS += dropNormalOS * (ripple * _WobbleAmplitude * 0.2);
        }

        // Straight up, leaned towards the camera by _TipLean: at 1 the tip lies across the line of sight.
        float3 TipAxisTowardsCamera()
        {
            float3 up = float3(0.0, 1.0, 0.0);
            float3 toCamera = SafeNormalize(_WorldSpaceCameraPos - TransformObjectToWorld(float3(0.0, 0.0, 0.0)));
            float3 acrossSight = SafeNormalize(up - toCamera * dot(up, toCamera));
            float3 tipWS = SafeNormalize(lerp(up, acrossSight, _TipLean));
            return SafeNormalize(TransformWorldToObjectDir(tipWS));
        }
        ENDHLSL

        Pass
        {
            Name "Water"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float tipWeight : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionOS;
                float3 normalOS;
                DeformToDrop(input.positionOS.xyz, TipAxisTowardsCamera(), positionOS, normalOS, output.tipWeight);

                output.positionWS = TransformObjectToWorld(positionOS);
                output.normalWS = TransformObjectToWorldNormal(normalOS);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 normal = normalize(input.normalWS);
                float3 toCamera = GetWorldSpaceNormalizeViewDir(input.positionWS);
                Light light = GetMainLight();

                // Deep in the middle, pale and almost opaque at the rim, like a lens of water.
                half facing = saturate(dot(normal, toCamera));
                half rim = pow(1.0 - facing, _RimPower);
                half3 color = lerp(_BaseColor.rgb, _RimColor.rgb, rim);

                // Light passing through gathers on the side away from the light.
                half through = pow(saturate(dot(-normal, light.direction) * 0.5 + 0.5), 3.0);
                color += _GlowColor.rgb * through * 0.35;
                color *= lerp(0.8, 1.0, saturate(dot(normal, light.direction) * 0.5 + 0.5));

                // A sharp sun glint on the round part plus the cartoon window highlight on the upper left.
                float3 halfway = normalize(light.direction + toCamera);
                half specular = pow(saturate(dot(normal, halfway)), _Shininess) * (1.0 - smoothstep(0.2, 0.7, input.tipWeight));
                float3 normalVS = TransformWorldToViewDir(normal);
                half window = 1.0 - smoothstep(_HighlightSize * 0.7, _HighlightSize, length(normalVS.xy - _HighlightPosition.xy));
                half shine = saturate(specular + window);
                color = lerp(color, _SpecularColor.rgb, shine);

                half alpha = saturate(lerp(_BaseColor.a, 1.0, rim) + shine);
                return half4(color, alpha);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);

                // The view here is the light's, so the shadow keeps an upright tip.
                float3 positionOS;
                float3 normalOS;
                float tipWeight;
                DeformToDrop(input.positionOS.xyz, SafeNormalize(TransformWorldToObjectDir(float3(0.0, 1.0, 0.0))), positionOS, normalOS, tipWeight);

                float3 positionWS = TransformObjectToWorld(positionOS);
                float3 normalWS = TransformObjectToWorldNormal(normalOS);

            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif

                output.positionCS = ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS)));
                return output;
            }

            half4 ShadowFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
