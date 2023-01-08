void worldpanalpha_vfx_float(
    UnityTexture2D u_mainTexture,
    float2 v_texCoord0,
    float3 v_eyeDir,
    float3 v_normal,
    float3 v_fogWorldPos,
    float3 v_eyeSpacePos,
    float4 v_color,
    float time,
    out float4 gl_FragColor
){
    time *= 0.01f;
    float MulOp = (u_speed1 * time);
    float AddOp = (v_fogWorldPos.xyz.y + MulOp);
    float2 VectorConstruct = float2(v_fogWorldPos.xyz.z, AddOp);
    float2 MulOp1505 = (VectorConstruct.xy * u_scale1 * 100.0f);

    float MulOp1539 = (u_speed2 * time);
    float AddOp1538 = (v_fogWorldPos.xyz.y + MulOp1539);
    float2 VectorConstruct1537 = float2(v_fogWorldPos.xyz.x, AddOp1538);
    float2 MulOp1535 = (VectorConstruct1537.xy * u_scale2 * 100.0f);

    gl_FragColor = float4(1.0f, 0.0f, 0.0f, 1.0f);

    float4 ColorStandin = v_color;
    float3 LerpOp = lerp(u_topColor.xyz, u_bottomColor.xyz, float(ColorStandin.y));
    float4 TextureStandin = tex2D(u_mainTexture, MulOp1505);
    float4 TextureStandin1411 = tex2D(u_mainTexture, MulOp1535);
    float3 NormalStandin = normalize(v_normal);
    float AddOp1324 = (1.0 + NormalStandin.xyz.z);
    float SatOp = clamp((1.0 - AddOp1324), 0.0, 1.0);
    float AddOp1323 = (clamp(NormalStandin.xyz.z, 0.0, 1.0) + SatOp);
    float DivOp = (u_blendXZ / 2.0);
    float SubOp = ((u_blendXZ * AddOp1323) - DivOp);
    float ClampOp = clamp(SubOp, 0.0, 1.0);
    float OneMinusOp = (1.0 - clamp(NormalStandin.xyz.y, 0.0, 1.0));
    float AddOp1566 = (NormalStandin.xyz.y + 1.0);
    float MulOp1570 = (clamp(AddOp1566, 0.0, 1.0) * OneMinusOp);
    float PowOp = pow(MulOp1570, u_fadeTop);
    float3 MulOp1316 = (PowOp * lerp(TextureStandin.xyz, TextureStandin1411.xyz, float(ClampOp)));
    float3 MulOp1337 = (MulOp1316 * u_brightness);
    float MulOp1312 = ((MulOp1337.x * ColorStandin.x) * 1.5);
    float4 VectorConstruct917 = float4(LerpOp.x, LerpOp.y, LerpOp.z, MulOp1312);
    gl_FragColor = VectorConstruct917;


    if (USE_FOG) {
        float v_eyeSpaceDepth = -v_eyeSpacePos.z;
        float opacity = clamp(gl_FragColor.a, 0.0, 1.0);

        float depthFogT = (v_eyeSpaceDepth - u_minFogDistance) / u_maxFogDistance;
        depthFogT = clamp(depthFogT, 0.0, u_maxFog);

        float heightFogRange = u_heightFogEnd - u_heightFogStart;
        float heightFogT = ((v_fogWorldPos.y - u_heightFogStart) / heightFogRange);
        heightFogT = clamp(heightFogT, 0.0, u_heightFogDensity);

        float3 heightMixColor = lerp(u_heightFogColor, u_fogColor, u_flipFogOrder);
        float heightMixT = lerp(heightFogT, depthFogT, u_flipFogOrder);
        float3 finalColor = gl_FragColor.rgb;
        finalColor.rgb = lerp(finalColor, heightMixColor * opacity, heightMixT);

        float3 depthMixColor = lerp(u_fogColor, u_heightFogColor, u_flipFogOrder);
        float depthMixT = lerp(depthFogT, heightFogT, u_flipFogOrder);
        finalColor.rgb = lerp(finalColor, depthMixColor * opacity, depthMixT);
        gl_FragColor = float4(finalColor, opacity);
    }

    gl_FragColor = pow(gl_FragColor, 2.2);
}