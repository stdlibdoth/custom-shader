void MainLight_float(float3 WorldPos, out half3 Direction, out half3 Color
    ,out float DistanceAtten,out half ShadowAtten)
    {
#ifdef SHADERGRAPH_PREVIEW
        Direction = normalize(float3(0.5f,0.5f,0.25f));
        Color = float3(1.0f,1.0f,1.0f);
        DistanceAtten = 1.0f;
        ShadowAtten = 1.0f;

#else      
        float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
        Light mainLight = GetMainLight(shadowCoord);

        Direction = mainLight.direction;
        Color = mainLight.color;
        DistanceAtten = mainLight.distanceAttenuation;
        DistanceAtten = 1;
        ShadowAtten = mainLight.shadowAttenuation;
        //ShadowAtten = 1;
#endif
    }