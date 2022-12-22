void pan2directions_vfx_float(
    UnityTexture2D u_tex1,
    float2 v_texCoord0,
    float4 v_color,
    float3 v_fogWorldPos,
    float3 v_eyeSpacePos,
    float time,
    out float4 gl_FragColor
) {
    /*Vertex shader*/
    float MulOp = (u_xSpeed * time);

    float AddOp = (MulOp + v_texCoord0.x);
    float MulOp1314 = (u_ySpeed * time);
    float AddOp1313 = (v_texCoord0.y + MulOp1314);
    float2 VectorConstruct = float2(AddOp, AddOp1313);

    float4 TextureStandin = tex2D(u_tex1, VectorConstruct);
    float4 ColorStandin = v_color;
    float MulOp1441 = (ColorStandin.w * TextureStandin.w);
    float4 VectorConstruct917 = float4(TextureStandin.xyz.x, TextureStandin.xyz.y, TextureStandin.xyz.z, MulOp1441);
    gl_FragColor = VectorConstruct917;

    if (USE_FOG) {
        float eyeSpaceDepth = -v_eyeSpacePos.z;
        float fogT = (eyeSpaceDepth - u_minFogDistance) / u_maxFogDistance;
        fogT = clamp(fogT, 0.0, u_maxFog);
        gl_FragColor.rgb = lerp(gl_FragColor.rgb, u_fogColor * gl_FragColor.a, fogT);

    }
    gl_FragColor = pow(gl_FragColor, 2.2);
}