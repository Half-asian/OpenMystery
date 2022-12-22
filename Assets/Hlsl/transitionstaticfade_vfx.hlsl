#define HAS_DYNAMIC_LIGHT
#define MAX_DIRECTIONAL_LIGHT_NUM 4
#define MAX_SPOT_LIGHT_NUM 3
#define MAX_POINT_LIGHT_NUM 3
#define USE_AMBIENT_COLOR

float3 tex_u_diffuseMap;
float3 tex_u_reflectionCubeMap;

float3 eyeDir;
float3 normal;
float3 v_worldPosition;
float3 v_eyeSpacePos;
float3 headNorm;

#if defined(HAS_DYNAMIC_LIGHT) && !defined(USE_VERTEX_LIGHTING)
float3 computeLighting(float3 normalVector, float3 lightDirection, float3 lightColor, float attenuation)
{
    float diffuse = max(dot(normalVector, lightDirection), 0.0);
    float3 diffuseColor = lightColor * diffuse * attenuation;

    return diffuseColor;
}
#endif

float3 calculateLightColor(float3 normal_param)
{
    float shadowVal = 1.0;
#ifdef USE_SHADOWMAP
    int index = 0;
#ifdef USE_CASCADES
    for (int i = 0; i < NUM_CASCADES - 1; ++i)
    {
        index += int(v_viewDepth > u_cascadeSplits[i]);
    }
#ifdef USE_DIRLIGHT_SHADOWMAP
    vec3 shadowCoords = v_shadowTexCoords[index];
#else
    vec3 shadowCoords = v_shadowTexCoords[index].xyz / v_shadowTexCoords[index].w;
#endif
#else
#ifdef USE_DIRLIGHT_SHADOWMAP
#ifdef USE_SHADOW_SAMPLER
    vec3 shadowCoords = v_shadowTexCoords;
#else
    vec3 shadowCoords = vec3(v_shadowTexCoords, v_shadowDepth);
#endif
#else
    vec3 shadowCoords = v_shadowTexCoords.xyz / v_shadowTexCoords.w;
#endif
#endif

    //In the current implementation, only spotlights need to worry about things outside the shadowmap range. Directional shadows are set up so that all rendered receivers will be in valid shadow texture space.
#ifdef USE_SPOTLIGHT_SHADOWMAP
    if (shadowCoords.x >= 0.0 && shadowCoords.x <= 1.0 && shadowCoords.y >= 0.0 && shadowCoords.y <= 1.0)
#endif
    {
        float curDepth = shadowCoords.z;
        shadowVal = shadowValForKernelPos(shadowCoords.xy, curDepth, index);
    }

#ifdef DRAW_CASCADE_REGIONS
    if (index == 0)
        gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
    else if (index == 1)
        gl_FragColor = vec4(0.0, 1.0, 0.0, 1.0);
    else if (index == 2)
        gl_FragColor = vec4(0.0, 0.0, 1.0, 1.0);
    else if (index == 3)
        gl_FragColor = vec4(1.0, 1.0, 0.0, 1.0);
    return;
#endif
#endif



#if defined(USE_AMBIENT_COLOR)
    float3 lightColor = u_AmbientLightSourceColor;
#else
    vec3 lightColor = vec3(0.0);
#endif

#if defined(HAS_DYNAMIC_LIGHT) && defined(USE_VERTEX_LIGHTING)
    lightColor += v_lightColor.rgb;

#if defined(USE_DIRLIGHT_SHADOWMAP)
    vec3 shadowLight = u_DirLightSourceColor[0] * (v_lightColor.w * shadowVal);
    lightColor += shadowLight;
#elif defined(USE_SPOTLIGHT_SHADOWMAP)
    vec3 shadowLight = u_SpotLightSourceColor[0] * (v_lightColor.w * shadowVal);
    lightColor += shadowLight;
#endif
#endif


    float3 eyeDir = normalize(v_eyeSpacePos);
    float3 normal = normalize(normal_param);


    //Do lighting calculations. Loops are manually unrolled because old devices seem to be incapable of doing this for some damn reason...
#if defined(HAS_DYNAMIC_LIGHT) && !defined(USE_VERTEX_LIGHTING)
    float3 curLightColor;
    float3 ldir;
    float attenuation;
#if (MAX_DIRECTIONAL_LIGHT_NUM > 0)
    //Index 0 is the shadowcasting light.

    ldir = normalize(u_DirLightSourceDirection1);

#ifdef USE_DIRLIGHT_SHADOWMAP
    curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor[0], 1.0) * shadowVal;
#else
    curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor1, 1.0);
#endif

    lightColor += curLightColor;

#if MAX_DIRECTIONAL_LIGHT_NUM > 1
    ldir = normalize(u_DirLightSourceDirection2);
    curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor2, 1.0);
    lightColor += curLightColor;
#endif

#if MAX_DIRECTIONAL_LIGHT_NUM > 2
    ldir = normalize(u_DirLightSourceDirection3);
    curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor3, 1.0);
    lightColor += curLightColor;
#endif

#if MAX_DIRECTIONAL_LIGHT_NUM > 3
    ldir = normalize(u_DirLightSourceDirection4);
    curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor4, 1.0);
    lightColor += curLightColor;
#endif
#endif

#if (MAX_POINT_LIGHT_NUM > 0)
    float3 vertToLight = u_PointLightSourcePosition1 - v_worldPosition;
    ldir = vertToLight * u_PointLightSourceRangeInverse1;
    attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
    vertToLight = normalize(vertToLight);
    curLightColor = computeLighting(normal, vertToLight, u_PointLightSourceColor1, attenuation);

    lightColor += curLightColor;

#if MAX_POINT_LIGHT_NUM > 1
    vertToLight = u_PointLightSourcePosition2 - v_worldPosition;
    ldir = vertToLight * u_PointLightSourceRangeInverse2;
    attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
    vertToLight = normalize(vertToLight);
    curLightColor = computeLighting(normal, vertToLight, u_PointLightSourceColor2, attenuation);

    lightColor += curLightColor;

#if MAX_POINT_LIGHT_NUM > 2
    vertToLight = u_PointLightSourcePosition3 - v_worldPosition;
    ldir = vertToLight * u_PointLightSourceRangeInverse3;
    attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
    vertToLight = normalize(vertToLight);
    curLightColor = computeLighting(normal, vertToLight, u_PointLightSourceColor3, attenuation);

    lightColor += curLightColor;

#endif
#endif
#endif

#if (MAX_SPOT_LIGHT_NUM > 0)

    vertToLight = u_SpotLightSourcePosition1 - v_worldPosition;
    ldir = vertToLight * u_SpotLightSourceRangeInverse1;
    attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
    vertToLight = normalize(vertToLight);

    float3 spotLightDirection = normalize(u_SpotLightSourceDirection1);

    float spotCurrentAngleCos = dot(spotLightDirection, -vertToLight);

    attenuation *= smoothstep(u_SpotLightSourceOuterAngleCos1, u_SpotLightSourceInnerAngleCos1, spotCurrentAngleCos);

#ifdef USE_SPOTLIGHT_SHADOWMAP
    curLightColor = computeLighting(normal, vertToLight, u_SpotLightSourceColor1, attenuation) * shadowVal;
#else
    curLightColor = computeLighting(normal, vertToLight, u_SpotLightSourceColor1, attenuation);
#endif

    lightColor += curLightColor;


#if MAX_SPOT_LIGHT_NUM > 1
    vertToLight = u_SpotLightSourcePosition2 - v_worldPosition;
    ldir = vertToLight * u_SpotLightSourceRangeInverse2;
    attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
    vertToLight = normalize(vertToLight);

    spotLightDirection = normalize(u_SpotLightSourceDirection2);

    spotCurrentAngleCos = dot(spotLightDirection, -vertToLight);

    // Apply spot attenuation
    attenuation *= smoothstep(u_SpotLightSourceOuterAngleCos2, u_SpotLightSourceInnerAngleCos2, spotCurrentAngleCos);

    curLightColor = computeLighting(normal, vertToLight, u_SpotLightSourceColor2, attenuation);
    lightColor += curLightColor;

#endif

#if MAX_SPOT_LIGHT_NUM > 2
    vertToLight = u_SpotLightSourcePosition3 - v_worldPosition;
    ldir = vertToLight * u_SpotLightSourceRangeInverse3;
    attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
    vertToLight = normalize(vertToLight);

    spotLightDirection = normalize(u_SpotLightSourceDirection3);

    spotCurrentAngleCos = dot(spotLightDirection, -vertToLight);

    // Apply spot attenuation
    attenuation *= smoothstep(u_SpotLightSourceOuterAngleCos3, u_SpotLightSourceInnerAngleCos3, spotCurrentAngleCos);

    curLightColor = computeLighting(normal, vertToLight, u_SpotLightSourceColor3, attenuation);
    lightColor += curLightColor;
#endif
#endif
#endif
    return lightColor;
}

void transitionstaticfade_vfx_float(
    UnityTexture2D u_diffuse,
    UnityTexture2D u_mask,
    float2 v_texCoord0,
    float3 v_eyeDir,
    float3 v_normal,
    float3 _v_fogWorldPos,
    float3 _v_eyeSpacePos,

    out float4 gl_FragColor
){
    v_eyeSpacePos = _v_eyeSpacePos;
    v_worldPosition = _v_fogWorldPos;

    float4 TextureStandin = tex2D(u_diffuse, v_texCoord0);
    float3 LightColorStandin = calculateLightColor(v_normal);
    float3 MulOp = (LightColorStandin.xyz * TextureStandin.xyz);
    float Val = 0.0;
    float OneMinusOp = (1.0 - u_transition);
    float AddOp = (OneMinusOp + u_gradient);
    float4 TextureStandin1375 = tex2D(u_mask, v_texCoord0);
    float SmoothStepOp = smoothstep(OneMinusOp, AddOp, clamp(TextureStandin1375.xyz.x, OneMinusOp, AddOp));
    float LerpOp = lerp(lerp(TextureStandin.w, Val, float(SmoothStepOp)), lerp(Val, TextureStandin.w, float(SmoothStepOp)), float(u_direction));
    float4 VectorConstruct = float4(MulOp.x, MulOp.y, MulOp.z, LerpOp);
    gl_FragColor = VectorConstruct;

    if (USE_FOG) {
        float v_eyeSpaceDepth = -v_eyeSpacePos.z;
        float opacity = clamp(gl_FragColor.a, 0.0, 1.0);

        float depthFogT = (v_eyeSpaceDepth - u_minFogDistance) / u_maxFogDistance;
        depthFogT = clamp(depthFogT, 0.0, u_maxFog);

        float heightFogRange = u_heightFogEnd - u_heightFogStart;
        float heightFogT = ((_v_fogWorldPos.y - u_heightFogStart) / heightFogRange);
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