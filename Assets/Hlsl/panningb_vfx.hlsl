//#define USE_UNLIT_DIFFUSE
#define USE_DIFFUSE
#define HAS_EYE_SHADOW_PARAMS
void panningb_vfx_float(
    UnityTexture2D u_alphaMap,
    float2 v_texCoord0,
    float4 v_color,
    float3 v_fogWorldPos,
    float3 v_eyeSpacePos,
    float time,
    out float4 gl_FragColor
) {
    /*Vertex shader*/
    float MulOp = (time * u_speed);
    float AddOp = (MulOp + v_texCoord0.y);
    float2 VectorConstruct = float2(v_texCoord0.x, AddOp);

    /*Fragment shder*/
    float4 TextureStandin = tex2D(u_alphaMap, VectorConstruct);
    float4 ColorStandin = v_color;
    float MulOp1434 = (2.0 * ColorStandin.x);
    float SubOp = (MulOp1434 - 1.0);
    float AddOp1432 = (TextureStandin.w + SubOp);
    float3 MulOp1441 = (AddOp1432 * u_diffuseColor.xyz);
    float4 VectorConstruct917 = float4(MulOp1441.x, MulOp1441.y, MulOp1441.z, AddOp1432);
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
        finalColor.rgb = lerp(finalColor, heightMixColor*opacity, heightMixT);

        float3 depthMixColor = lerp(u_fogColor, u_heightFogColor, u_flipFogOrder);
        float depthMixT = lerp(depthFogT, heightFogT, u_flipFogOrder);
        finalColor.rgb = lerp(finalColor, depthMixColor * opacity, depthMixT);
        gl_FragColor = float4(finalColor, opacity);

    }

    gl_FragColor = pow(gl_FragColor, 2.2);
    gl_FragColor.a = AddOp1432;
}