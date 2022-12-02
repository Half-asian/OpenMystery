//#define USE_UNLIT_DIFFUSE
#define USE_DIFFUSE
#define HAS_EYE_SHADOW_PARAMS
void simpletexture_float(
    UnityTexture2D tex1,
    float2 v_texCoord,
    float3 v_fogWorldPos,
    float v_eyeDepth,

    out float4 gl_FragColor
) {

    float4 col = tex2D(tex1, v_texCoord);
    gl_FragColor = float4(col.rgb, col.a * alpha);

    if (USE_FOG) {
        float depthFogT = (v_eyeDepth - u_minFogDistance) / u_maxFogDistance;
        depthFogT = clamp(depthFogT, 0.0, u_maxFog);

        float heightFogRange = u_heightFogEnd - u_heightFogStart;
        float heightFogT = ((v_fogWorldPos.y - u_heightFogStart) / heightFogRange);
        heightFogT = clamp(heightFogT, 0.0, u_heightFogDensity);

        float3 heightMixColor = lerp(u_heightFogColor, u_fogColor, u_flipFogOrder);
        float heightMixT = lerp(heightFogT, depthFogT, u_flipFogOrder);
        gl_FragColor.rgb = lerp(gl_FragColor, heightMixColor, heightMixT);

        float3 depthMixColor = lerp(u_fogColor, u_heightFogColor, u_flipFogOrder);
        float depthMixT = lerp(depthFogT, heightFogT, u_flipFogOrder);
        gl_FragColor.rgb = lerp(gl_FragColor, depthMixColor, depthMixT);
    }

    gl_FragColor = pow(gl_FragColor, 2.2);
}