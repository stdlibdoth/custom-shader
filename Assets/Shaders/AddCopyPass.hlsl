#ifndef ADD_COPY_PASS_INCLUDED
#define ADD_COPY_PASS_INCLUDED


#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"


FRAMEBUFFER_INPUT_FLOAT(0);

float4 AddFragment(Varyings i):SV_Target
{
    float2 uv = i.texcoord;
    float4 color = FragBlit(i,sampler_PointClamp);
    float4 addColor = LOAD_FRAMEBUFFER_INPUT(0,i.positionCS.xy);
    float blendFactor = max(addColor.x,addColor.y);
    blendFactor = max(blendFactor,addColor.z);
    float3 final = (1-blendFactor)*color.xyz + addColor.xyz;

    return float4(final,1);
}


#endif