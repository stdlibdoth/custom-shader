Shader "Unlit/OutlineShader"
{
    Properties
    {
        _Thickness("Thickness",Float) = 0.1
        _Color("Color",Color) = (1,1,1,1)
        _MinDepth("Min Depth",Float) = 0.1
        _MaxDepth("Max Depth",Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Blend One Zero

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment OutlineFragment


            #include "Assets/Shaders/OutlinePass.hlsl"
            ENDHLSL
        }
    }
}
