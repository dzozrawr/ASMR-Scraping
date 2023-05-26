//teselejsn
// This shader adds tessellation in URP
Shader "Example/URPUnlitShaderTessallated"
{
    
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
        _Tess ("Tessellation", Range(1, 32)) = 20
        _MaxTessDistance ("Max Tess Distance", Range(1, 32)) = 20
        _Noise ("Noise", 2D) = "gray" { }
        
        _Weight ("Displacement Amount", Range(0, 1)) = 0
        
        //_Color ("Color", Color) = (1, 1, 1, 1)
        //_MainTex ("Albedo (RGB)", 2D) = "white" { }
        //_Glossiness ("Smoothness", Range(0, 1)) = 0.5
        
        //  _BumpMap ("Normal Map", 2D) = "bump" { }
        
        _UpDir ("Up Dir", Vector) = (1, 0, 0, 0)
        _MeshTop ("Mesh Top", float) = 0.5
        _RollDir ("Roll Dir", Vector) = (0, 1, 0, 0)
        _Radius ("Radius", float) = 0
        _Deviation ("Deviation", float) = 0.5
        _PointX ("PointX", float) = 0
        _PointY ("PointY", float) = 0
        
        // Specular vs Metallic workflow
        [HideInInspector] _WorkflowMode ("WorkflowMode", Float) = 1.0
        
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" { }
        [MainColor] _BaseColor ("Color", Color) = (1, 1, 1, 1)
        
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale ("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        
        // _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap ("Metallic", 2D) = "white" { }
        
        _SpecColor ("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap ("Specular", 2D) = "white" { }
        
        [ToggleOff] _SpecularHighlights ("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections ("Environment Reflections", Float) = 1.0
        
        _BumpScale ("Scale", Float) = 1.0
        _BumpMap ("Normal Map", 2D) = "bump" { }
        
        _Parallax ("Scale", Range(0.005, 0.08)) = 0.005
        _ParallaxMap ("Height Map", 2D) = "black" { }
        
        _OcclusionStrength ("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap ("Occlusion", 2D) = "white" { }
        
        [HDR] _EmissionColor ("Color", Color) = (0, 0, 0)
        _EmissionMap ("Emission", 2D) = "white" { }
        
        _DetailMask ("Detail Mask", 2D) = "white" { }
        _DetailAlbedoMapScale ("Scale", Range(0.0, 2.0)) = 1.0
        _DetailAlbedoMap ("Detail Albedo x2", 2D) = "linearGrey" { }
        _DetailNormalMapScale ("Scale", Range(0.0, 2.0)) = 1.0
        [Normal] _DetailNormalMap ("Normal Map", 2D) = "bump" { }
        
        // SRP batching compatibility for Clear Coat (Not used in Lit)
        [HideInInspector] _ClearCoatMask ("_ClearCoatMask", Float) = 0.0
        [HideInInspector] _ClearCoatSmoothness ("_ClearCoatSmoothness", Float) = 0.0
        
        // Blending state
        [HideInInspector] _Surface ("__surface", Float) = 0.0
        [HideInInspector] _Blend ("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip ("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
        [HideInInspector] _Cull ("__cull", Float) = 2.0
        
        _ReceiveShadows ("Receive Shadows", Float) = 1.0
        // Editmode props
        [HideInInspector] _QueueOffset ("Queue offset", Float) = 0.0
        
        // ObsoleteProperties
        [HideInInspector] _MainTex ("BaseMap", 2D) = "white" { }
        [HideInInspector] _Color ("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _GlossMapScale ("Smoothness", Float) = 0.0
        [HideInInspector] _Glossiness ("Smoothness", Float) = 0.0
        [HideInInspector] _GlossyReflections ("EnvironmentReflections", Float) = 0.0
        
        [HideInInspector][NoScaleOffset]unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" { }
        [HideInInspector][NoScaleOffset]unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" { }
        [HideInInspector][NoScaleOffset]unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" { }
    }
    
    // The SubShader block containing the Shader code.
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" "ShaderModel" = "4.5" }
        LOD 300
        
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]
            
            
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            
            //#pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #pragma vertex TessellationVertexProgram
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            
            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "CustomTessellation.hlsl"
            
            #pragma require tessellation
            // This line defines the name of the vertex shader.
            
            // This line defines the name of the fragment shader.
            //   #pragma fragment frag
            // This line defines the name of the hull shader.
            #pragma hull hull
            // This line defines the name of the domain shader.
            #pragma domain domain
            
            sampler2D _MainTex;
            // sampler2D _BumpMap;
            half _Glossiness;
            //half _Metallic;
            float4 _Color;
            
            float4 _UpDir;
            float _MeshTop;
            float4 _RollDir;
            half _Radius;
            half _Deviation;
            half _PointX;
            half _PointY;
            
            sampler2D _Noise;
            float _Weight;
            
            #ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
                #define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
                
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                
                // GLES2 has limited amount of interpolators
                #if defined(_PARALLAXMAP) && !defined(SHADER_API_GLES)
                    #define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
                #endif
                
                #if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
                    #define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
                #endif
                
                // keep this file in sync with LitGBufferPass.hlsl
                
                struct Attributes
                {
                    float4 positionOS: POSITION;
                    float3 normalOS: NORMAL;
                    float4 tangentOS: TANGENT;
                    float2 texcoord: TEXCOORD0;
                    float2 lightmapUV: TEXCOORD1;
                    //my variables start
                    float4 color: COLOR;
                    //my variables end
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };
                
                struct Varyings
                {
                    float2 uv: TEXCOORD0;
                    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
                    
                    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                        float3 positionWS: TEXCOORD2;
                    #endif
                    
                    float3 normalWS: TEXCOORD3;
                    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
                        float4 tangentWS: TEXCOORD4;    // xyz: tangent, w: sign
                    #endif
                    float3 viewDirWS: TEXCOORD5;
                    
                    half4 fogFactorAndVertexLight: TEXCOORD6; // x: fogFactor, yzw: vertex light
                    
                    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                        float4 shadowCoord: TEXCOORD7;
                    #endif
                    
                    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
                        float3 viewDirTS: TEXCOORD8;
                    #endif
                    
                    float4 positionCS: SV_POSITION;
                    //my added variables start
                    float4 color: COLOR;
                    float3 normal: NORMAL;
                    // float4 noise: TEXCOORD1;
                    //my added variables end
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                };
                
                void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
                {
                    inputData = (InputData)0;
                    
                    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                        inputData.positionWS = input.positionWS;
                    #endif
                    
                    half3 viewDirWS = SafeNormalize(input.viewDirWS);
                    #if defined(_NORMALMAP) || defined(_DETAIL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
                    #else
                        inputData.normalWS = input.normalWS;
                    #endif
                    
                    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                    inputData.viewDirectionWS = viewDirWS;
                    
                    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                        inputData.shadowCoord = input.shadowCoord;
                    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
                    #else
                        inputData.shadowCoord = float4(0, 0, 0, 0);
                    #endif
                    
                    inputData.fogCoord = input.fogFactorAndVertexLight.x;
                    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
                    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
                    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
                }
                
                ///////////////////////////////////////////////////////////////////////////////
                //                  Vertex and Fragment functions                            //
                ///////////////////////////////////////////////////////////////////////////////
                
                
                // Used in Standard (Physically Based) shader
                half4 LitPassFragment(Varyings input): SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                    
                    #if defined(_PARALLAXMAP)
                        #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
                            half3 viewDirTS = input.viewDirTS;
                        #else
                            half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, input.viewDirWS);
                        #endif
                        ApplyPerPixelDisplacement(viewDirTS, input.uv);
                    #endif
                    
                    SurfaceData surfaceData;
                    InitializeStandardLitSurfaceData(input.uv, surfaceData);
                    
                    InputData inputData;
                    InitializeInputData(input, surfaceData.normalTS, inputData);
                    
                    half4 color = UniversalFragmentPBR(inputData, surfaceData);
                    
                    color.rgb = MixFog(color.rgb, inputData.fogCoord);
                    color.a = OutputAlpha(color.a, _Surface);
                    
                    return color;
                }
                
            #endif
            
            // pre tesselation vertex program
            ControlPoint TessellationVertexProgram(Attributes v)
            {
                ControlPoint p;
                
                p.vertex = v.positionOS;
                p.uv = v.texcoord;
                p.normal = v.normalOS;
                p.color.xy = v.lightmapUV;
                
                return p;
            }
            
            // after tesselation
            Varyings vert(Attributes input)
            {
                //LIGHTING PART START///    // this part of the code is copied from Varyings LitPassVertex(Attributes input), it was maybe modified with different variable names
                Varyings output = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                
                // normalWS and tangentWS already normalize.
                // this is required to avoid skewing the direction during interpolation
                // also required for per-vertex lighting and SH evaluation
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
                half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                
                
                // already normalized from normal transform to WS.
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = viewDirWS;
                #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
                    real sign = input.tangentOS.w * GetOddNegativeScale();
                    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
                #endif
                #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
                    output.tangentWS = tangentWS;
                #endif
                
                #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
                    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
                    output.viewDirTS = viewDirTS;
                #endif
                
                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
                
                output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
                
                #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                    output.positionWS = vertexInput.positionWS;
                #endif
                
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    output.shadowCoord = GetShadowCoord(vertexInput);
                #endif
                
                output.positionCS = vertexInput.positionCS;
                
                //LIGHTING PART END///
                
                //ROLLING PART START//
                // Varyings output;
                float Noise = tex2Dlod(_Noise, float4(input.texcoord, 0, 0)).r;
                
                input.positionOS.xyz += (input.normalOS) * Noise * _Weight;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.normal = input.normalOS;
                // output.uv = input.texcoord;
                
                float3 v0 = input.positionOS.xyz;
                
                float3 upDir = normalize(_UpDir);
                float3 rollDir = normalize(_RollDir);
                
                //float y = UNITY_ACCESS_INSTANCED_PROP(Props, _PointY);
                float y = _PointY;
                float dP = dot(v0 - upDir * y, upDir);
                dP = max(0, dP);
                float3 fromInitialPos = upDir * dP;
                v0 -= fromInitialPos;
                
                float radius = _Radius + _Deviation * max(0, - (y - _MeshTop));
                float length = 2 * 3.14 * (radius - _Deviation * max(0, - (y - _MeshTop)) / 2);
                float r = dP / max(0, length);
                float a = 2 * r * 3.14;
                
                float s = sin(a);
                float c = cos(a);
                float one_minus_c = 1.0 - c;
                
                float3 axis = normalize(cross(upDir, rollDir));
                float3x3 rot_mat = {
                    one_minus_c * axis.x * axis.x + c, one_minus_c * axis.x * axis.y - axis.z * s, one_minus_c * axis.z * axis.x + axis.y * s,
                    one_minus_c * axis.x * axis.y + axis.z * s, one_minus_c * axis.y * axis.y + c, one_minus_c * axis.y * axis.z - axis.x * s,
                    one_minus_c * axis.z * axis.x - axis.y * s, one_minus_c * axis.y * axis.z + axis.x * s, one_minus_c * axis.z * axis.z + c
                };
                float3 cycleCenter = rollDir * _PointX + rollDir * radius + upDir * y;
                
                float3 fromCenter = v0.xyz - cycleCenter;
                float3 shiftFromCenterAxis = cross(axis, fromCenter);
                shiftFromCenterAxis = cross(shiftFromCenterAxis, axis);
                shiftFromCenterAxis = normalize(shiftFromCenterAxis);
                fromCenter -= shiftFromCenterAxis * _Deviation * dP;// * ;
                
                v0.xyz = mul(rot_mat, fromCenter) + cycleCenter;
                
                input.positionOS.xyz = v0;
                
                
                output.positionCS = mul(UNITY_MATRIX_MVP, float4(input.positionOS.xyz, 1.0));
                // output.uv = input.texcoord;
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                // v.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                // v.texcoord=float4(1,1,0,1);
                // v.texcoord=TRANSFORM_TEX(v.texcoord, _MainTex);
                
                //  UNITY_TRANSFER_FOG(o,o.vertex);
                
                //ROLLING PART END//
                
                return output;
            }
            
            [UNITY_domain("tri")]
            Varyings domain(TessellationFactors factors, OutputPatch < ControlPoint, 3 > patch, float3 barycentricCoordinates: SV_DomainLocation)
            {
                Attributes v;
                v.positionOS = \
                patch[0].vertex * barycentricCoordinates.x + \

                

                
                
                
                
                
                
                patch[1].vertex * barycentricCoordinates.y + \
                                                                

                
                
                
                patch[2].vertex * barycentricCoordinates.z;
                
                
                // DomainPos(texcoord)
                v.texcoord = \
                patch[0].uv * barycentricCoordinates.x + \

                

                
                
                
                
                
                
                
                
                patch[1].uv * barycentricCoordinates.y + \
                                                                                                                                                                

                
                
                
                
                
                
                
                
                
                patch[2].uv * barycentricCoordinates.z;
                //DomainPos(color)
                v.color = \
                patch[0].color * barycentricCoordinates.x + \

                

                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                patch[1].color * barycentricCoordinates.y + \
                                                                                                                                                                

                
                
                
                
                
                
                
                
                
                patch[2].color * barycentricCoordinates.z;
                //DomainPos(normalOS)
                v.normalOS = \
                patch[0].normal * barycentricCoordinates.x + \

                

                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                patch[1].normal * barycentricCoordinates.y + \
                                                                                                                                                                

                
                
                
                
                
                
                
                
                
                patch[2].normal * barycentricCoordinates.z;
                
                
                /*                 DomainPos(vertex)
                DomainPos(uv)
                DomainPos(color)
                DomainPos(normal) */
                
                
                return vert(v);
            }
            
            // The fragment shader definition.
            half4 frag(Varyings IN): SV_Target
            {
                half4 tex = tex2D(_Noise, IN.uv);
                
                return tex;
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
            Cull[_Cull]
            
            HLSLPROGRAM
            
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            
            #pragma vertex vert
            #pragma fragment ShadowPassFragment
            
            sampler2D _MainTex;
            // sampler2D _BumpMap;
            half _Glossiness;
            //half _Metallic;
            float4 _Color;
            
            float4 _UpDir;
            float _MeshTop;
            float4 _RollDir;
            half _Radius;
            half _Deviation;
            half _PointX;
            half _PointY;
            
            sampler2D _Noise;
            float _Weight;
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #ifndef UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED
                #define UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED
                
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
                
                #include "CustomTessellation.hlsl"
                #pragma require tessellation
                
                float3 _LightDirection;
                
                struct Attributes
                {
                    float4 positionOS: POSITION;
                    float3 normalOS: NORMAL;
                    float2 texcoord: TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };
                
                struct Varyings
                {
                    float2 uv: TEXCOORD0;
                    float4 positionCS: SV_POSITION;
                };
                
                float4 GetShadowPositionHClip(Attributes input)
                {
                    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                    
                    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                    
                    #if UNITY_REVERSED_Z
                        positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                    #else
                        positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                    #endif
                    
                    return positionCS;
                }
                
/*                 // pre tesselation vertex program
                ControlPoint TessellationVertexProgram(Attributes v)
                {
                    ControlPoint p;
                    
                    p.vertex = v.positionOS;
                    p.uv = v.texcoord;
                    p.normal = v.normalOS;
                    // p.color.xy = v.lightmapUV;
                    
                    return p;
                } */
                
                Varyings vert(Attributes input)
                {
                    //SHADOW PART START///    // this part of the code is copied from ShadowPassVertex(Attributes input), it was maybe modified with different variable names
                    Varyings output;
                    UNITY_SETUP_INSTANCE_ID(input);
                    
                    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                    output.positionCS = GetShadowPositionHClip(input);
                    
                    //SHADOW PART END///
                    
                    //ROLLING PART START//
                    // Varyings output;
                     float Noise = tex2Dlod(_Noise, float4(input.texcoord, 0, 0)).r;
                    
                    input.positionOS.xyz += (input.normalOS) * Noise * _Weight;
                    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                    //output.color = input.color;
                    //output.normal = input.normalOS;
                    
                    
                    float3 v0 = input.positionOS.xyz;
                    
                    float3 upDir = normalize(_UpDir);
                    float3 rollDir = normalize(_RollDir);
                    
                    //float y = UNITY_ACCESS_INSTANCED_PROP(Props, _PointY);
                    float y = _PointY;
                    float dP = dot(v0 - upDir * y, upDir);
                    dP = max(0, dP);
                    float3 fromInitialPos = upDir * dP;
                    v0 -= fromInitialPos;
                    
                    float radius = _Radius + _Deviation * max(0, - (y - _MeshTop));
                    float length = 2 * 3.14 * (radius - _Deviation * max(0, - (y - _MeshTop)) / 2);
                    float r = dP / max(0, length);
                    float a = 2 * r * 3.14;
                    
                    float s = sin(a);
                    float c = cos(a);
                    float one_minus_c = 1.0 - c;
                    
                    float3 axis = normalize(cross(upDir, rollDir));
                    float3x3 rot_mat = {
                        one_minus_c * axis.x * axis.x + c, one_minus_c * axis.x * axis.y - axis.z * s, one_minus_c * axis.z * axis.x + axis.y * s,
                        one_minus_c * axis.x * axis.y + axis.z * s, one_minus_c * axis.y * axis.y + c, one_minus_c * axis.y * axis.z - axis.x * s,
                        one_minus_c * axis.z * axis.x - axis.y * s, one_minus_c * axis.y * axis.z + axis.x * s, one_minus_c * axis.z * axis.z + c
                    };
                    float3 cycleCenter = rollDir * _PointX + rollDir * radius + upDir * y;
                    
                    float3 fromCenter = v0.xyz - cycleCenter;
                    float3 shiftFromCenterAxis = cross(axis, fromCenter);
                    shiftFromCenterAxis = cross(shiftFromCenterAxis, axis);
                    shiftFromCenterAxis = normalize(shiftFromCenterAxis);
                    fromCenter -= shiftFromCenterAxis * _Deviation * dP;// * ;
                    
                    v0.xyz = mul(rot_mat, fromCenter) + cycleCenter;
                    
                    input.positionOS.xyz = v0;
                    
                    
                    output.positionCS = mul(UNITY_MATRIX_MVP, float4(input.positionOS.xyz, 1.0));
                    // output.uv = input.texcoord;
                    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                    // v.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    // v.texcoord=float4(1,1,0,1);
                    // v.texcoord=TRANSFORM_TEX(v.texcoord, _MainTex);
                    
                    //  UNITY_TRANSFER_FOG(o,o.vertex);
                    
                    //ROLLING PART END//
                    
                    return output;
                }
                
     /*            [UNITY_domain("tri")]
                Varyings domain(TessellationFactors factors, OutputPatch < ControlPoint, 3 > patch, float3 barycentricCoordinates: SV_DomainLocation)
                {
                    Attributes v;
                    v.positionOS = \
                    patch[0].vertex * barycentricCoordinates.x + \

                    

                    
                    
                    
                    
                    patch[1].vertex * barycentricCoordinates.y + \
                                                                            

                    
                    
                    
                    patch[2].vertex * barycentricCoordinates.z;
                    
                    
                    v.texcoord = \
                    patch[0].uv * barycentricCoordinates.x + \

                    

                    
                    
                    
                    
                    
                    patch[1].uv * barycentricCoordinates.y + \
                                                                                                                                                                            

                    
                    
                    
                    
                    patch[2].uv * barycentricCoordinates.z;


                    v.normalOS = \
                    patch[0].normal * barycentricCoordinates.x + \

                    

                    
                    
                    
                    
                    
                    patch[1].normal * barycentricCoordinates.y + \
                                                            

                    
                    
                    patch[2].normal * barycentricCoordinates.z;
                    

                    
                    
                    return vert(v);
                } */
                
                /*                 Varyings ShadowPassVertex(Attributes input)
                {
                    Varyings output;
                    UNITY_SETUP_INSTANCE_ID(input);
                    
                    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                    output.positionCS = GetShadowPositionHClip(input);
                    return output;
                } */
                
                half4 ShadowPassFragment(Varyings input): SV_TARGET
                {
                    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
                    return 0;
                }
                
            #endif
            
            ENDHLSL
            
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ZWrite On
            ColorMask 0
            Cull[_Cull]
            
            HLSLPROGRAM
            
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
            
        }
    }
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.LitShader"
}