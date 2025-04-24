Shader "Custom/URP/CellShader"
{
    
    Properties
    {
        _baseMap("Base Map", 2D) = "white"{}
        
        _diffuseColor("Diffuse Color", Color) = (1,1,1,1)
        _rimColor("Rim Color", Color) = (1,1,1,1)
        _specularColor("Specular Color", Color) = (1,1,1,1)
        _shadowColor("Shadow Color", Color) = (0.1,0.1,0.1,1)
        
        _shadowIntensityShift("Shadow shift",Float) = 0.1
        _rampCutoff("Shadow Cut Off", Float) = 0
        _rampRolloff("Shadow Roll Off Distance", Float) = 0
        _smoothness("Smoothness",Float) = 200
        _rimIntensity("Rim Intensity", Float) = 10
    }

    SubShader
    {
        
        Pass
        {
            Name "CellLit"
            Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }


            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            // This line defines the name of the vertex shader.

            #include "Assets/Shaders/CellShadingLit.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster"}

            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // This line defines the name of the vertex shader.

            #include "Assets/Shaders/CellShadowCasterPass.hlsl"

            ENDHLSL
        }

    }
}