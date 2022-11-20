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

float3 tex_u_diffuseMap;
float3 tex_u_reflectionCubeMap;

float3 eyeDir;
float3 normal;
float3 v_absWorldSpacePos;
float3 viewPosition;
float3 headNorm;

#if defined(HAS_DYNAMIC_LIGHT) && !defined(USE_VERTEX_LIGHTING)
float3 computeLighting(float3 normalVector, float3 lightDirection, float3 lightColor, float attenuation)
{
    float diffuse = max(dot(normalVector, lightDirection), 0.0);
    float3 diffuseColor = lightColor * diffuse * attenuation;
    
    return diffuseColor;
}
#endif


//This is a fake specular highlight, which will follow the camera. Use normal instead of light vector when computing the halfVector.
#ifdef USE_SPECULAR
float3 computeSpecularLight(float3 normalVector, float3 eyeDirection)
{
    float3 halfVector = normalize(normalVector + eyeDirection);
    float specFactor = max(dot(halfVector, normalVector), 0.0);
    
    float cosMin = cos(u_specMinRadius);
    float cosMax = cos(u_specMaxRadius);
    
    specFactor = clamp((specFactor - cosMax) / (cosMin - cosMax), 0.0, 1.0);
    float specAmount = specFactor*specFactor*specFactor;
    
    return u_specColor * specAmount;
}
#endif

float4 main_float(){ 

    eyeDir = -eyeDir;
    
    float shadowVal = 1.0;
    #ifdef USE_SHADOWMAP
        int index = 0;
        #ifdef USE_CASCADES
            for (int i = 0; i < NUM_CASCADES-1; ++i)
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
    
    
    #ifdef USE_DIFFUSE
    float3 diffuseColor = tex_u_diffuseMap.rgb;
    
    //Darken based on head-space normal.           
    float yFactor = smoothstep(0.45, 1.2, 1.0 - headNorm.x);
    float zFactor = smoothstep(0.2, 1.0, headNorm.z); //Code says headNorm.y This must be wrong
    diffuseColor *= (yFactor*zFactor);
    #else
    vec3 diffuseColor = vec3(0.0);
    #endif
    
    #ifdef USE_AMBIENT_COLOR
    float3 lightColor = u_AmbientLightSourceColor;
    #else
    float3 lightColor = float3(0.0, 0.0, 0.0);
    #endif
    
    #if defined(HAS_DYNAMIC_LIGHT) && defined(USE_VERTEX_LIGHTING)
    lightColor += v_lightColor;
    #endif
        
    //For the eyes, specular isn't dependent on light sources.
    #ifdef USE_SPECULAR
    float3 specularColor = computeSpecularLight(normal, -eyeDir);
    #endif

    //Do lighting calculations. Loops are manually unrolled because old devices seem to be incapable of doing this for some damn reason...
    #if defined(HAS_DYNAMIC_LIGHT) && !defined(USE_VERTEX_LIGHTING)
        float3 curLightColor;
        float3 ldir;
        float attenuation;
        float3 vertToLight;
        #if (MAX_DIRECTIONAL_LIGHT_NUM > 0)
            //Index 0 is the shadowcasting light.
    
            #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[0]);
            #else
                ldir = normalize(u_DirLightSourceDirection1);
            #endif
    
            #ifdef USE_DIRLIGHT_SHADOWMAP
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor[0], 1.0) * shadowVal;
            #else
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor1, 1.0);
            #endif
    
            lightColor += curLightColor;
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 1
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[1]);
                #else
                ldir = normalize(u_DirLightSourceDirection2);
                #endif
                
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor2, 1.0);
                lightColor += curLightColor;
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 2
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[2]);
                #else
                ldir = normalize(u_DirLightSourceDirection3);
                #endif
                
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor3, 1.0);
                lightColor += curLightColor;
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 3
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[3]);
                #else
                ldir = normalize(u_DirLightSourceDirection4);
                #endif
                
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor4, 1.0);
                lightColor += curLightColor;
            #endif
        #endif

        #if (MAX_POINT_LIGHT_NUM > 0)
            vertToLight = u_PointLightSourcePosition1 - v_absWorldSpacePos;
            ldir = vertToLight * u_PointLightSourceRangeInverse1;
            attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
            lightColor += computeLighting(normal, normalize(vertToLight), u_PointLightSourceColor1, attenuation);
    
            #if MAX_POINT_LIGHT_NUM > 1
                vertToLight = u_PointLightSourcePosition2 - v_absWorldSpacePos;
                ldir = vertToLight * u_PointLightSourceRangeInverse2;
                attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
                lightColor += computeLighting(normal, normalize(vertToLight), u_PointLightSourceColor2, attenuation);
    
                #if MAX_POINT_LIGHT_NUM > 2
                    vertToLight = u_PointLightSourcePosition3 - v_absWorldSpacePos;
                    ldir = vertToLight * u_PointLightSourceRangeInverse3;
                    attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
                    lightColor += computeLighting(normal, normalize(vertToLight), u_PointLightSourceColor3, attenuation);
                #endif
            #endif
        #endif

        #if (MAX_SPOT_LIGHT_NUM > 0)
            vertToLight = u_SpotLightSourcePosition1 - v_absWorldSpacePos;
            ldir = vertToLight * u_SpotLightSourceRangeInverse1;
            attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
            vertToLight = normalize(vertToLight);

            #ifdef HAS_NORMAL_TEXTURE
                float3 spotLightDirection = normalize(v_spotLightDirection1);
            #else
                float3 spotLightDirection = normalize(u_SpotLightSourceDirection1);
            #endif


            float spotCurrentAngleCos = dot(spotLightDirection, -vertToLight);
            attenuation *= smoothstep(u_SpotLightSourceOuterAngleCos1, u_SpotLightSourceInnerAngleCos1, spotCurrentAngleCos);


            curLightColor = computeLighting(normal, vertToLight, u_SpotLightSourceColor1, attenuation);
            lightColor += curLightColor;
            #if MAX_SPOT_LIGHT_NUM > 1
                // Compute range attenuation
                vertToLight = u_SpotLightSourcePosition2 - v_absWorldSpacePos;
                ldir = vertToLight * u_SpotLightSourceRangeInverse2;
                attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
                vertToLight = normalize(vertToLight);
                
                #ifdef HAS_NORMAL_TEXTURE
                spotLightDirection = normalize(v_spotLightDirection2);
                #else
                spotLightDirection = normalize(u_SpotLightSourceDirection2);
                #endif

                // "-lightDirection" is used because light direction points in opposite direction to spot direction.
                spotCurrentAngleCos = dot(spotLightDirection, -vertToLight);

                // Apply spot attenuation
                attenuation *= smoothstep(u_SpotLightSourceOuterAngleCos2, u_SpotLightSourceInnerAngleCos2, spotCurrentAngleCos);
                
                curLightColor = computeLighting(normal, vertToLight, u_SpotLightSourceColor2, attenuation);
                lightColor += curLightColor;
            #endif
    
            #if MAX_SPOT_LIGHT_NUM > 2
                // Compute range attenuation
                vertToLight = u_SpotLightSourcePosition3 - v_absWorldSpacePos;
                ldir = vertToLight * u_SpotLightSourceRangeInverse3;
                attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
                vertToLight = normalize(vertToLight);
                
                #ifdef HAS_NORMAL_TEXTURE
                spotLightDirection = normalize(v_spotLightDirection3);
                #else
                spotLightDirection = normalize(u_SpotLightSourceDirection3);
                #endif
                
                // "-lightDirection" is used because light direction points in opposite direction to spot direction.
                spotCurrentAngleCos = dot(spotLightDirection, -vertToLight);
                
                // Apply spot attenuation
                attenuation *= smoothstep(u_SpotLightSourceOuterAngleCos3, u_SpotLightSourceInnerAngleCos3, spotCurrentAngleCos);
                
                curLightColor = computeLighting(normal, vertToLight, u_SpotLightSourceColor3, attenuation);
                lightColor += curLightColor;
            #endif
        #endif
    #endif

    //Determine final color, based on diffuse lighting, specular, and reflection.
    float3 finalColor = float3(0.0, 0.0, 0.0);

    #ifdef USE_SPECULAR
    finalColor += specularColor;
    #endif
    
    #ifdef HAS_REFLECTION_TEXTURE
        float3 reflectTexel = tex_u_reflectionCubeMap;
        float reflectAmount = u_reflectionAmount;
        #ifdef USE_REFLECTION_ANGLE
            float fresnel = dot(eyeDir, normal);
            float reflectFresnel = 1.0 - clamp((fresnel * fresnel * u_reflectionAngle), 0.0, 1.0);
            #ifdef USE_FLATNESS
                reflectFresnel *= u_flatness;
            #endif
            reflectAmount *= reflectFresnel;
        #endif
        #ifdef USE_REFLECTION_TINT
            reflectTexel *= u_reflectionTintColor;
        #endif
        finalColor += (reflectTexel * reflectAmount);
        diffuseColor = diffuseColor * (1.0 - reflectAmount);
    #endif

    #ifdef USE_UNLIT_DIFFUSE
    finalColor += diffuseColor;
    #else
    finalColor += diffuseColor * (lightColor);
    #endif

    if (USE_FOG) {
        float3 v_fogWorldPos = v_absWorldSpacePos;
        //#ifdef NEEDS_NORMALS
        float v_eyeSpaceDepth = -viewPosition.z;
        //#endif
        float depthFogT = (v_eyeSpaceDepth - u_minFogDistance) / u_maxFogDistance;
        depthFogT = clamp(depthFogT, 0.0, u_maxFog);

        float heightFogRange = u_heightFogEnd - u_heightFogStart;
        float heightFogT = ((v_fogWorldPos.y - u_heightFogStart) / heightFogRange);
        heightFogT = clamp(heightFogT, 0.0, u_heightFogDensity);

        float3 heightMixColor = lerp(u_heightFogColor, u_fogColor, u_flipFogOrder);
        float heightMixT = lerp(heightFogT, depthFogT, u_flipFogOrder);
        finalColor = lerp(finalColor, heightMixColor, heightMixT);

        float3 depthMixColor = lerp(u_fogColor, u_heightFogColor, u_flipFogOrder);
        float depthMixT = lerp(depthFogT, heightFogT, u_flipFogOrder);
        finalColor = lerp(finalColor, depthMixColor, depthMixT);
    }

    //finalColor = pow(finalColor, 2.2);
    return float4(finalColor, alpha);

}

void eyes_float(
    float3 _u_diffuseMap, 
    float3 _u_reflectionCubeMap, 
    float3 _eyeDir, 
    float3 _normal, 
    float3 _headNorm,
    float3 _absWorldSpacePos,
    float3 _viewPosition,

    out float4 gl_FragColor
){
    tex_u_diffuseMap = _u_diffuseMap;
    tex_u_reflectionCubeMap = _u_reflectionCubeMap;

    eyeDir = _eyeDir;
    normal = _normal;
    headNorm = _headNorm;
    v_absWorldSpacePos = _absWorldSpacePos;
    viewPosition = _viewPosition;

    gl_FragColor = main_float();
}