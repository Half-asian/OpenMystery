void twocolorglow_vfx_float(
    float3 u_frontColor,
    float3 u_sideColor,
    float3 v_eyeSpacePos,
    float3 v_normal,
    float3 u_cameraPosition,
    float3 v_absWorldSpacePos,
    float3 v_worldPosition,
    out float4 gl_FragColor
){
    float3 NormalStandin = normalize(v_normal);
    float3 CamVectorStandin = u_cameraPosition - v_worldPosition;
    float3 NormOp = normalize(CamVectorStandin.xyz);
	float DotOp = dot(NormalStandin.xyz, NormOp);
	float MulOp = ((DotOp * DotOp) * u_rimAngle);
	float ClampOp = clamp(MulOp, 0.0, 1.0);
	float PowOp = pow(ClampOp, u_colorGradient);
	float3 MulOp1459 = ((1.0 - PowOp) * u_sideColor.xyz);
	float3 AddOp = (MulOp1459 + (PowOp * u_frontColor.xyz));
	float PowOp1461 = pow(ClampOp, u_opacityGradient);
	float MulOp1464 = (PowOp1461 * u_opacity);
	float4 VectorConstruct = float4(AddOp.x, AddOp.y, AddOp.z, MulOp1464);
	gl_FragColor = VectorConstruct;

    if (USE_FOG){
        float3 v_fogWorldPos = v_absWorldSpacePos;
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
        finalColor = lerp(finalColor, heightMixColor*opacity, heightMixT);
        
        float3 depthMixColor = lerp(u_fogColor, u_heightFogColor, u_flipFogOrder);
        float depthMixT = lerp(depthFogT, heightFogT, u_flipFogOrder);
        finalColor = lerp(finalColor, depthMixColor*opacity, depthMixT);
        gl_FragColor = float4(finalColor, opacity);
    }

    gl_FragColor = pow(gl_FragColor, 2.2);
}