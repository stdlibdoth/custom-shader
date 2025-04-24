Shader "Unlit/SimpleAddBlitShader"
{
    Properties
    {

    }
    SubShader
    {
        Pass
        {
            Name "AddBlit"
            Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
            
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment AddFragment

            #include "Assets/Shaders/AddCopyPass.hlsl"
            ENDHLSL
        }
    }
}
