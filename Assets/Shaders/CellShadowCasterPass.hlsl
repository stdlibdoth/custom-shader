#ifndef CELL_SHADOW_CASTER_PASS_INCLUDED
#define CELL_SHADOW_CASTER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"


// struct VertData
// {
//     float4 positionOS   : POSITION;
// };

// struct FragData
// {
//     float4 positionHCS  : SV_POSITION;
// };

// Varyings shadowPassVertex(Attributes input)
// {
// 	Varyings output = (Varyings)0;
// 	UNITY_SETUP_INSTANCE_ID(input);

// 	// Example Displacement
// 	//input.positionOS += float4(0, _SinTime.y, 0, 0);

// 	//output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
// 	output.positionCS = GetShadowPositionHClip(input);
// 	return output;
// }

// // The fragment shader definition.
// half4 shadowPassFragment(Varyings IN) : SV_Target
// {
//     return half4(0.1,0.5,0,1);
// }


#endif
