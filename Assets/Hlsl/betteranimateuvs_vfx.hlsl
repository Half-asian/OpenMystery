void betteranimateuvs_vfx_float(
    float3 viewPosition,
    UnityTexture2D u_colorTexture,
    float2 v_texCoord0,
    float3 v_absWorldSpacePos,
    float4 vertexcolor,
    float time,
    out float4 gl_FragColor
){
	float MulOp = (time * u_speedX);

	float AddOp = (MulOp + v_texCoord0.x);
	float MulOp1427 = (time * u_speedY);
	float AddOp1426 = (MulOp1427 + v_texCoord0.y);
	v_texCoord0 = float2(AddOp, AddOp1426);

    float4 TextureStandin = tex2D(u_colorTexture, v_texCoord0);
    float4 ColorStandin = vertexcolor;
    float3 AddOp1441 = (TextureStandin.xyz + ColorStandin.xyz);
	float4 VectorConstruct917 = float4(AddOp1441.x, AddOp1441.y, AddOp1441.z, TextureStandin.w);
	gl_FragColor = VectorConstruct917;

    if (USE_FOG){
        float3 v_fogWorldPos = v_absWorldSpacePos;
        float v_eyeSpaceDepth = -viewPosition.z;
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