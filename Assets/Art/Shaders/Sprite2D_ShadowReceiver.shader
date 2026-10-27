Shader "HitMe/2.5D/Sprite_ShadowReceiver"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color ("Tint Color", Color) = (1, 1, 1, 1)
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _ShadowColor ("Shadow Tint Color", Color) = (0.28, 0.28, 0.38, 1.0)
        _ShadowStrength ("Shadow Strength", Range(0.0, 1.0)) = 0.65
        [Toggle(_BILLBOARD_Y)] _BillboardY ("Cylindrical Y-Billboard", Float) = 0
        [Toggle(_BILLBOARD_FULL)] _BillboardFull ("Full Camera Billboard", Float) = 0
    }

    // =========================================================================
    // SUB-SHADER 1: UNIVERSAL RENDER PIPELINE (URP)
    // =========================================================================
    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout" 
            "Queue" = "AlphaTest" 
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector" = "True"
        }

        LOD 200
        Cull Off
        ZWrite On

        // ---------------------------------------------------------------------
        // Pass 1: Forward Lit (Nhận bóng đổ thời gian thực Directional Light)
        // ---------------------------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            // URP Shadow Keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _BILLBOARD_Y
            #pragma shader_feature_local _BILLBOARD_FULL

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
                float3 positionWS   : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                half   _Cutoff;
                half4  _ShadowColor;
                half   _ShadowStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);

                #if defined(_BILLBOARD_Y)
                    // Quay theo trục thẳng đứng hướng về camera (Cylindrical Billboard)
                    float3 rootWS = TransformObjectToWorld(float3(0, 0, 0));
                    float3 camDir = _WorldSpaceCameraPos - rootWS;
                    camDir.y = 0;
                    camDir = normalize(camDir);
                    float3 camRight = normalize(cross(float3(0, 1, 0), camDir));
                    worldPos = rootWS + camRight * input.positionOS.x + float3(0, 1, 0) * input.positionOS.y;
                #elif defined(_BILLBOARD_FULL)
                    // Quay trực diện camera trên cả 2 trục
                    float3 rootWS = TransformObjectToWorld(float3(0, 0, 0));
                    float3 camRight = UNITY_MATRIX_V[0].xyz;
                    float3 camUp    = UNITY_MATRIX_V[1].xyz;
                    worldPos = rootWS + camRight * input.positionOS.x + camUp * input.positionOS.y;
                #endif

                output.positionWS = worldPos;
                output.positionCS = TransformWorldToHClip(worldPos);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                
                // Alpha Cutout sắc nét loại bỏ hoàn toàn viền matte bẩn
                clip(texColor.a - _Cutoff);

                // Tính toán bóng đổ thời gian thực từ Directional Light trong URP
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    Light mainLight = GetMainLight(shadowCoord);
                    half shadowAtten = mainLight.shadowAttenuation;
                #else
                    half shadowAtten = 1.0;
                #endif

                // Hòa trộn vùng sáng và vùng tối bóng đổ với Shadow Tint
                half shadowFactor = lerp(1.0, shadowAtten, _ShadowStrength);
                half3 shadedTint = lerp(_ShadowColor.rgb, half3(1.0, 1.0, 1.0), shadowFactor);
                
                half3 finalRGB = texColor.rgb * shadedTint;
                return half4(finalRGB, texColor.a);
            }
            ENDHLSL
        }

        // ---------------------------------------------------------------------
        // Pass 2: ShadowCaster (Sprite 2.5D tự đổ bóng xuống mặt sân 3D)
        // ---------------------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vertShadow
            #pragma fragment fragShadow

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                half   _Cutoff;
                half4  _ShadowColor;
                half   _ShadowStrength;
            CBUFFER_END

            Varyings vertShadow(Attributes input)
            {
                Varyings output = (Varyings)0;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 fragShadow(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                clip(texColor.a - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }

    // =========================================================================
    // SUB-SHADER 2: BUILT-IN RENDER PIPELINE & UNIVERSAL FALLBACK
    // =========================================================================
    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout" 
            "Queue" = "AlphaTest" 
            "IgnoreProjector" = "True"
        }

        LOD 100
        Cull Off
        ZWrite On

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                SHADOW_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed _Cutoff;
            fixed4 _ShadowColor;
            fixed _ShadowStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                clip(col.a - _Cutoff);

                fixed atten = SHADOW_ATTENUATION(i);
                fixed shadowFactor = lerp(1.0, atten, _ShadowStrength);
                fixed3 shadowTint = lerp(_ShadowColor.rgb, fixed3(1.0, 1.0, 1.0), shadowFactor);

                return fixed4(col.rgb * shadowTint, col.a);
            }
            ENDCG
        }

        Pass
        {
            Name "Caster"
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _Cutoff;

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                fixed4 texcol = tex2D(_MainTex, i.uv);
                clip(texcol.a - _Cutoff);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }

    FallBack "Transparent/Cutout/Diffuse"
}
