#ifndef OUTLINE_PASS_INCLUDED
#define OUTLINE_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"


SAMPLER(sampler_BlitTexture);


CBUFFER_START(UnityPerMaterial)
    float _Thickness;
    float _MinDepth;
    float _MaxDepth;
    float4 _Color;
CBUFFER_END

float4 OutlineFragment(Varyings i):SV_Target
{
    float2 uv = i.texcoord;
    float4 original = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);
    // For testing;
    //float4 depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
    // Four sample UV points.
    float offset_positive = +ceil(_Thickness * 0.5); 
    float offset_negative = -floor(_Thickness * 0.5); 
    float left = _BlitTexture_TexelSize.x * offset_negative; 
    float right = _BlitTexture_TexelSize.x * offset_positive; 
    float top = _BlitTexture_TexelSize.y * offset_negative; 
    float bottom = _BlitTexture_TexelSize.y * offset_positive; 
    float2 uv0 = uv + float2(left, top);
    float2 uv1 = uv + float2(right, bottom);
    float2 uv2 = uv + float2(right, top); 
    float2 uv3 = uv + float2(left, bottom);

    // Sample depth.
    float d0 = Linear01Depth(SampleSceneDepth(uv0),_ZBufferParams); 
    float d1 = Linear01Depth(SampleSceneDepth(uv1),_ZBufferParams);
    float d2 = Linear01Depth(SampleSceneDepth(uv2),_ZBufferParams);
    float d3 = Linear01Depth(SampleSceneDepth(uv3),_ZBufferParams);
    
    float d =length(float2(d1 - d0, d3 - d2));
    d = smoothstep(_MinDepth, _MaxDepth, d);
    float4 output = d*_Color;
    return output;
}


#endif