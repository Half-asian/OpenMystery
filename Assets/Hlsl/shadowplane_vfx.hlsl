void shadowplane_vfx_float(
    float2 v_texCoord0,
    float3 v_fogWorldPos,
    float3 v_eyeSpacePos,
    out float4 gl_FragColor
) {
    /*Vertex shader*/
    float MulOp = (v_texCoord0.y * u_opacity);
    float PowOp = pow(MulOp, u_falloff);
    float4 VectorConstruct = float4(u_shadowColor.xyz.x, u_shadowColor.xyz.y, u_shadowColor.xyz.z, PowOp);
    gl_FragColor = VectorConstruct;

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
        finalColor.rgb = lerp(finalColor, heightMixColor*opacity, heightMixT);

        float3 depthMixColor = lerp(u_fogColor, u_heightFogColor, u_flipFogOrder);
        float depthMixT = lerp(depthFogT, heightFogT, u_flipFogOrder);
        finalColor.rgb = lerp(finalColor, depthMixColor * opacity, depthMixT);
        gl_FragColor = float4(finalColor, opacity);

    }

    gl_FragColor = pow(gl_FragColor, 2.2);
}