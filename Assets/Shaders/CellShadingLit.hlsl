#ifndef CELL_SHANDING_INCLUDED
#define CELL_SHANDING_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"

struct VertData
{
    float4 positionOS   : POSITION;
    float3 normal : NORMAL;
    float2 uv: TEXCOORD0;
};

struct FragData
{
    float4 positionHCS  : SV_POSITION;
    float2 uv: TEXCOORD0;
    float3 normal:TEXCOORD1;
    float3 positionWS  : TEXCOORD2;
};


TEXTURE2D(_baseMap);
SAMPLER(sampler_baseMap);


CBUFFER_START(UnityPerMaterial)
    float4 _baseMap_ST;

    float4 _diffuseColor;
    float4 _rimColor;
    float4 _specularColor;
    float4 _diffuseShadowColor;
    float4 _externalShadowColor;


    float _shadowIntensityShift;
    float _rampCutoff;
    float _rampRolloff;
    float _smoothness;
    float _rimIntensity;
CBUFFER_END


float FresnelIntensity(float3 normal, float3 viewDir, float Power)
{
    return pow((1.0 - saturate(dot(normalize(normal), normalize(viewDir)))), Power);
}


float DiffuseIntensity(Light l, float3 normal)
{
    //return l.shadowAttenuation*saturate(dot(l.direction,normal));
    return saturate(dot(l.direction,normal));
}

float SpecularIntensity(Light l,float3 viewDir, float3 normal ,float smoothness)
{
    float specular = dot(normalize(l.direction + viewDir),normal);
    return pow(saturate(specular),smoothness);
}

FragData vert(VertData IN)
{
    FragData OUT;
    float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);

    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
    OUT.positionWS = positionWS;
    OUT.normal = TransformObjectToWorldNormal(IN.normal);
    OUT.uv = TRANSFORM_TEX(IN.uv, _baseMap);
    return OUT;
}

// The fragment shader definition.
half4 frag(FragData IN) : SV_Target
{
    float3 normalWS = normalize(IN.normal);
    float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
    Light mainLight = GetMainLight(shadowCoord);

    //Diffuse
    float diffuseIntensity = DiffuseIntensity(mainLight,normalWS);

    float3 diffuseShadowMask = step(_rampCutoff+ _rampRolloff,diffuseIntensity);
    float3 diffuseShadow = (1-diffuseShadowMask)*_diffuseShadowColor.xyz*_shadowIntensityShift; 
    float3 externalShadow = diffuseShadowMask*(1-mainLight.shadowAttenuation)*_externalShadowColor.xyz;

    float3 diffuseHighlight = smoothstep(_rampCutoff, _rampCutoff+ _rampRolloff,diffuseIntensity)* mainLight.shadowAttenuation * mainLight.color*_diffuseColor.xyz;

    float3 diffuse = max(diffuseHighlight,diffuseShadow) + externalShadow;
    //Specular
    float3 viewDir = GetWorldSpaceNormalizeViewDir(IN.positionWS);
    float specularIntensity = diffuseIntensity * SpecularIntensity(mainLight,viewDir,normalWS,_smoothness);
    specularIntensity = smoothstep(_rampCutoff, _rampCutoff+ _rampRolloff,specularIntensity);
    float3 specular = _specularColor.xyz* specularIntensity;



    //rim
    float fresnel = FresnelIntensity(normalWS, viewDir,_rimIntensity) * diffuseIntensity;
    float3 rim = smoothstep(_rampCutoff, _rampCutoff+ _rampRolloff,fresnel)*_rimColor.xyz;
    
    //texture
    float4 texColor = SAMPLE_TEXTURE2D(_baseMap,sampler_baseMap,IN.uv);
    
    float3 final = texColor.xyz * (diffuse + specular + rim);
    return half4(final,1);
    
}


#endif
