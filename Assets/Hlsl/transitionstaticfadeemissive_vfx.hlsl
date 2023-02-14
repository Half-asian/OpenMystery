void transitionstaticfadeemissive_vfx_float(
    UnityTexture2D u_diffuse,
    UnityTexture2D u_mask,
    float2 v_texCoord0,
    float3 v_eyeSpacePos,
    out float4 gl_FragColor
) {
    float4 TextureStandin = tex2D(u_diffuse, v_texCoord0);
    float Val = 0.0;
    float OneMinusOp = (1.0 - u_transition);
    float AddOp = (OneMinusOp + u_gradient);
    float4 TextureStandin1375 = tex2D(u_mask, v_texCoord0);
    float SmoothStepOp = smoothstep(OneMinusOp, AddOp, clamp(TextureStandin1375.xyz.x, OneMinusOp, AddOp));
    float LerpOp = lerp(lerp(TextureStandin.w, Val, float(SmoothStepOp)), lerp(Val, TextureStandin.w, float(SmoothStepOp)), float(u_direction));
    float4 VectorConstruct = float4(TextureStandin.xyz.x, TextureStandin.xyz.y, TextureStandin.xyz.z, LerpOp);
    gl_FragColor = VectorConstruct;

    if (USE_FOG) {
        float v_eyeSpaceDepth = -v_eyeSpacePos.z;

        float fogT = (v_eyeSpaceDepth - u_minFogDistance) / u_maxFogDistance;
        fogT = clamp(fogT, 0.0, u_maxFog);

        gl_FragColor.rgb = lerp(gl_FragColor, u_fogColor * gl_FragColor.a, fogT);
    }

    gl_FragColor = pow(gl_FragColor, 2.2);
    gl_FragColor.a = LerpOp;
}