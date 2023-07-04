void vertexdiffusergb_vfx_float(
    float3 v_eyeSpacePos,
    float4 v_color,
    out float4 gl_FragColor
){
    float4 ColorStandin = v_color;
    float4 VectorConstruct = float4(ColorStandin.xyz.x, ColorStandin.xyz.y, ColorStandin.xyz.z, 1.0);
	gl_FragColor = VectorConstruct;

    if(USE_FOG) {
        float eyeSpaceDepth = -v_eyeSpacePos.z;
        float fogT = (eyeSpaceDepth - u_minFogDistance) / u_maxFogDistance;
        fogT = clamp(fogT, 0.0, u_maxFog);
        gl_FragColor.rgb = lerp(gl_FragColor.rgb, u_fogColor*gl_FragColor.a, fogT);
    }

    gl_FragColor = pow(gl_FragColor, 2.2);
}