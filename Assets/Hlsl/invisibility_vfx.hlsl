//#define USE_UNLIT_DIFFUSE
#define USE_DIFFUSE
#define HAS_EYE_SHADOW_PARAMS
#define USE_SPECULAR
#define HAS_DYNAMIC_LIGHT
#define MAX_DIRECTIONAL_LIGHT_NUM 4
#define MAX_SPOT_LIGHT_NUM 3
#define MAX_POINT_LIGHT_NUM 3
//#define HAS_REFLECTION_TEXTURE
#define USE_REFLECTION_ANGLE
#define USE_REFLECTION_TINT
#define USE_AMBIENT_COLOR
#define USE_SCREENDOOR_TRANSPARENCY

UnityTexture2D tex_u_baseTexture;
UnityTexture2D tex_u_projectionMap;

float3 u_cameraPosition;
float3 v_worldPosition;
float3 v_normal;

float2 v_texCoord0;

float2 gl_FragCoord;
float2 u_viewport;

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


    float3 eyeDir = normalize(u_cameraPosition);//v_eyeSpacePos);
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

float4 main_float() {

    float3 Color = float3(0.0, 0.0, 0.0);
    float3 NormalStandin = normalize(v_normal);
    float3 CamVectorStandin = u_cameraPosition - v_worldPosition;
    float3 NormOp = normalize(CamVectorStandin.xyz);
    float DotOp = dot(NormalStandin.xyz, NormOp);
    float MulOp = ((DotOp * DotOp) * u_glowRimAngle);
    float ClampOp = clamp(MulOp, 0.0, 1.0);
    float3 MulOp1734 = ((1.0 - ClampOp) * u_glowRimColor.xyz);
    float3 NormOp1360 = normalize(CamVectorStandin.xyz);
    float DotOp1377 = dot(NormalStandin.xyz, NormOp1360);
    float MulOp1379 = ((DotOp1377 * DotOp1377) * u_rimAngle);
    float ClampOp1382 = clamp(MulOp1379, 0.0, 1.0);
    float PowOp = pow(ClampOp1382, u_warp);

    float2 VectorConstruct = gl_FragCoord *u_viewport;

    float MulOp1425 = (VectorConstruct.xy.x * u_scaleX);
    float AddOp = (u_offsetX + MulOp1425);
    float MulOp1419 = (VectorConstruct.xy.y * u_scaleY);
    float AddOp1421 = (u_offsetY + MulOp1419);
    float2 VectorConstruct1414 = float2(AddOp, AddOp1421);
    float2 AddOp1385 = (((1.0 - PowOp)) + VectorConstruct1414.xy);
    //AddOp1385.y = 1.0 - AddOp1385.y;
    float4 TextureStandin = tex2D(tex_u_projectionMap, AddOp1385); //VectorConstruct);
    float3 LerpOp = lerp(Color.xyz, (MulOp1734 + TextureStandin.xyz), float(u_blend));
    float4 VectorConstruct917 = float4(LerpOp.x, LerpOp.y, LerpOp.z, 1.0);
    float4 TextureStandin1610 = tex2D(tex_u_baseTexture, v_texCoord0);
    float3 Color1726 = float3(0.0, 0.0, 0.0);
    float3 LerpOp1727 = lerp(TextureStandin1610.xyz, Color1726.xyz, float(u_blend));
    float3 LightColorStandin = calculateLightColor(v_normal);
    float3 MulOp949 = (LightColorStandin.xyz * LerpOp1727);
    float4 VectorConstruct946 = float4(MulOp949.x, MulOp949.y, MulOp949.z, 1.0);
    float4 AddOp947 = (VectorConstruct917 + VectorConstruct946);
    float4 gl_FragColor = AddOp947;

    gl_FragColor = pow(gl_FragColor, 2.2);
    return float4(gl_FragColor);

}

void invisibility_vfx_float(
    //Textures
    UnityTexture2D _u_baseTexture,
    UnityTexture2D _u_projectionMap,

    //Data
    float3 _normal,
    float3 _absWorldSpacePos,
    float3 _viewPosition,
    float2 _screenPosition,
    //UVS
    float2 _uv0,
    out float4 gl_FragColor
) {
    u_viewport = float2(1.1f, 1.1f);

    tex_u_baseTexture = _u_baseTexture;
    tex_u_projectionMap = _u_projectionMap;

    v_normal = _normal;
    v_worldPosition = _absWorldSpacePos;
    u_cameraPosition = _viewPosition;
    gl_FragCoord = _screenPosition;

    v_texCoord0 = _uv0;

    gl_FragColor = main_float();
}