void AdditionalLight_float(float3 WorldPos, int Index, out half3 Direction, out half3 Color
    ,out half DistanceAtten,out half ShadowAtten)
    {
        Direction = normalize(float3(0.5f,0.5,0.25f));
        Color = float3(1.0f,1.0f,1.0f);
        DistanceAtten = 1.0f;
        ShadowAtten = 1.0f;

#ifndef SHADERGRAPH_PREVIEW
        if(Index<GetAdditionalLightsCount())
        {
                Light addLight = GetAdditionalLight(Index, WorldPos);
                Direction = addLight.direction;
                Color = addLight.color;
                DistanceAtten = addLight.distanceAttenuation;
                ShadowAtten = addLight.shadowAttenuation;                
        }
#endif
    }