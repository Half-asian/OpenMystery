#define vec4 float4
#define vec3 float3
#define vec2 float2
#define mix lerp

#define USE_DIFFUSE
//#define USE_AMBIENT_COLOR
#define HAS_DYNAMIC_LIGHT
#define MAX_DIRECTIONAL_LIGHT_NUM 0
#define USE_SPECULAR

#define USE_UNLIT_DIFFUSE

#if ((MAX_DIRECTIONAL_LIGHT_NUM > 0) || (MAX_POINT_LIGHT_NUM > 0) || (MAX_SPOT_LIGHT_NUM > 0))
    #define HAS_DYNAMIC_LIGHT
    #if defined(USE_DIRLIGHT_SHADOWMAP) || defined(USE_SPOTLIGHT_SHADOWMAP)
        #define USE_SHADOWMAP
    #endif
#endif


#if defined(HAS_DYNAMIC_LIGHT) && !defined(USE_VERTEX_LIGHTING)
vec3 computeLighting(vec3 normalVector, vec3 lightDirection, vec3 lightColor, float attenuation)
{
    float diffuse = max(dot(normalVector, lightDirection), 0.0);
    vec3 diffuseColor = lightColor * diffuse * attenuation;
    
    return diffuseColor;
}
#endif

//This is a fake specular highlight, which will follow the camera. Use normal instead of light vector when computing the halfVector.
#ifdef USE_SPECULAR
vec3 computeSpecularLight(vec3 normalVector, vec3 eyeDirection)
{
    vec3 halfVector = normalize(normalVector + eyeDirection);
    float specFactor = max(dot(halfVector, normalVector), 0.0);
    
    float cosMin = cos(u_specMinRadius);
    float cosMax = cos(u_specMaxRadius);
    
    specFactor = clamp((specFactor - cosMax) / (cosMin - cosMax), 0.0, 1.0);
    float specAmount = specFactor*specFactor*specFactor;
    
    return u_specColor * specAmount;
}
#endif

float4 processHMShader(FragInputs input, LayerTexCoord layerTexCoord, PositionInputs posInput){

    float shadowVal = 1.0;

    vec3 normalwsa = layerTexCoord.vertexNormalWS;
    vec3 triplanarWeightsa = layerTexCoord.triplanarWeights;
    vec3 headNorm = vec3(-normalwsa.x * triplanarWeightsa.x, normalwsa.y * triplanarWeightsa.y, normalwsa.z * triplanarWeightsa.z);

    #ifdef USE_DIFFUSE
    vec3 gradient = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(u_diffuse), sampler_u_diffuse, layerTexCoord.base).rgb;
    vec3 maskColor = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(u_mask), sampler_u_mask, layerTexCoord.base).rgb;
    vec3 diffuseColor = mix(vec3(1.0, 1.0, 1.0), u_diffuseColor, maskColor.g) * gradient;
    //Darken based on head-space normal.
    //vec3 headNorm = vec3(1.0, 1.0, 1.0); //normalize(v_headNormal);
    #ifdef HAS_EYE_SHADOW_PARAMS
    float topShadowStart = u_topShadowStart;
    float topShadowEnd = u_topShadowEnd;
    float sideShadowStart = u_sideShadowStart;
    float sideShadowEnd = u_sideShadowEnd;
    #else
    const float topShadowStart = 0.4;
    const float topShadowEnd = 1.2;
    const float sideShadowStart = 0.1;
    const float sideShadowEnd = 1.0;
    #endif

    float yFactor = smoothstep(topShadowStart, topShadowEnd, 1.0 - headNorm.x);
    float zFactor = smoothstep(sideShadowStart, sideShadowEnd, headNorm.z);
    //diffuseColor *= (yFactor*zFactor);
    #else
    vec3 diffuseColor = vec3(0.0);
    #endif

    #ifdef USE_AMBIENT_COLOR
    vec3 lightColor = u_AmbientLightSourceColor;
    #else
    vec3 lightColor = vec3(0.0, 0.0, 0.0);
    #endif

    #if defined(HAS_DYNAMIC_LIGHT) && defined(USE_VERTEX_LIGHTING)
    lightColor += v_lightColor;
    #endif

    vec3 eyeDir = normalize(GetWorldSpaceNormalizeViewDir(posInput.positionWS));
    vec3 normalws = layerTexCoord.vertexNormalWS;
    vec3 triplanarWeights = layerTexCoord.triplanarWeights;
    vec3 normal = vec3(-normalws.x * triplanarWeights.x, normalws.y * triplanarWeights.y, normalws.z * triplanarWeights.z);

    //For the eyes, specular isn't dependent on light sources.
    #ifdef USE_SPECULAR
    vec3 specularColor = computeSpecularLight(normal, eyeDir);
    specularColor = eyeDir;
    #endif

        //Do lighting calculations. Loops are manually unrolled because old devices seem to be incapable of doing this for some damn reason...
    #if defined(HAS_DYNAMIC_LIGHT) && !defined(USE_VERTEX_LIGHTING)
        vec3 curLightColor;
        vec3 ldir;
        float attenuation;
        #if (MAX_DIRECTIONAL_LIGHT_NUM > 0)
            //Index 0 is the shadowcasting light.
    
            #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(u_DirLightSourceDirection1);
            #else
                ldir = normalize(u_DirLightSourceDirection1);
            #endif
    
            #ifdef USE_DIRLIGHT_SHADOWMAP
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor1, 1.0) * shadowVal;
            #else
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor1, 1.0);
            #endif
    
            lightColor += curLightColor;
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 1
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(u_DirLightSourceDirection2);
                #else
                ldir = normalize(u_DirLightSourceDirection2);
                #endif
                
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor2, 1.0);
                lightColor += curLightColor;
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 2
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(u_DirLightSourceDirection3);
                #else
                ldir = normalize(u_DirLightSourceDirection3);
                #endif
                
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor3, 1.0);
                lightColor += curLightColor;
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 3
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(u_DirLightSourceDirection4);
                #else
                ldir = normalize(u_DirLightSourceDirection4);
                #endif
                
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor4, 1.0);
                lightColor += curLightColor;
            #endif
        #endif

        #if (MAX_POINT_LIGHT_NUM > 0)
        for (int i = 0; i < MAX_POINT_LIGHT_NUM; ++i)
        {
            ldir = v_vertexToPointLightDirection[i] * u_PointLightSourceRangeInverse[i];
            attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
            lightColor += computeLighting(normal, normalize(v_vertexToPointLightDirection[i]), u_PointLightSourceColor[i], attenuation);
        }
        #endif

        #if (MAX_SPOT_LIGHT_NUM > 0)
            ldir = v_vertexToSpotLightDirection[0] * u_SpotLightSourceRangeInverse[0];
            attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
            vec3 vertexToSpotLightDirection = normalize(v_vertexToSpotLightDirection[0]);
            
            #ifdef HAS_NORMAL_TEXTURE
            vec3 spotLightDirection = normalize(v_spotLightDirection[0]);
            #else
            vec3 spotLightDirection = normalize(u_SpotLightSourceDirection[0]);
            #endif
    
            float spotCurrentAngleCos = dot(spotLightDirection, -vertexToSpotLightDirection);
    
            attenuation *= smoothstep(u_SpotLightSourceOuterAngleCos[0], u_SpotLightSourceInnerAngleCos[0], spotCurrentAngleCos);
    
            #ifdef USE_SPOTLIGHT_SHADOWMAP
                curLightColor = computeLighting(normal, vertexToSpotLightDirection, u_SpotLightSourceColor[0], attenuation) * shadowVal;
            #else
                curLightColor = computeLighting(normal, vertexToSpotLightDirection, u_SpotLightSourceColor[0], attenuation);
            #endif
    
            lightColor += curLightColor;
    
            #if MAX_SPOT_LIGHT_NUM > 1
                // Compute range attenuation
                ldir = v_vertexToSpotLightDirection[1] * u_SpotLightSourceRangeInverse[1];
                attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
                vertexToSpotLightDirection = normalize(v_vertexToSpotLightDirection[1]);
                
                #ifdef HAS_NORMAL_TEXTURE
                spotLightDirection = normalize(v_spotLightDirection[1]);
                #else
                spotLightDirection = normalize(u_SpotLightSourceDirection[1]);
                #endif

                // "-lightDirection" is used because light direction points in opposite direction to spot direction.
                spotCurrentAngleCos = dot(spotLightDirection, -vertexToSpotLightDirection);

                // Apply spot attenuation
                attenuation *= smoothstep(u_SpotLightSourceOuterAngleCos[1], u_SpotLightSourceInnerAngleCos[1], spotCurrentAngleCos);
                
                curLightColor = computeLighting(normal, vertexToSpotLightDirection, u_SpotLightSourceColor[1], attenuation);
                lightColor += curLightColor;
            #endif
    
            #if MAX_SPOT_LIGHT_NUM > 2
                // Compute range attenuation
                ldir = v_vertexToSpotLightDirection[2] * u_SpotLightSourceRangeInverse[2];
                attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
                vertexToSpotLightDirection = normalize(v_vertexToSpotLightDirection[2]);
                
                #ifdef HAS_NORMAL_TEXTURE
                spotLightDirection = normalize(v_spotLightDirection[2]);
                #else
                spotLightDirection = normalize(u_SpotLightSourceDirection[2]);
                #endif
                
                // "-lightDirection" is used because light direction points in opposite direction to spot direction.
                spotCurrentAngleCos = dot(spotLightDirection, -vertexToSpotLightDirection);
                
                // Apply spot attenuation
                attenuation *= smoothstep(u_SpotLightSourceOuterAngleCos[2], u_SpotLightSourceInnerAngleCos[2], spotCurrentAngleCos);
                
                curLightColor = computeLighting(normal, vertexToSpotLightDirection, u_SpotLightSourceColor[2], attenuation);
                lightColor += curLightColor;
            #endif
        #endif
    #endif


    //Determine final color, based on diffuse lighting, specular, and reflection.
    vec3 finalColor = vec3(0.0, 0.0, 0.0);

    #ifdef USE_SPECULAR
    finalColor += specularColor;
    #endif

    #ifdef HAS_REFLECTION_TEXTURE
        vec3 reflectTexel = textureCube(u_reflectionCubeMap, v_reflectVector).rgb;
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


    #ifdef USE_FOG
        float eyeSpaceDepth = -v_eyeSpacePos.z;
        float depthFogT = (eyeSpaceDepth - u_minFogDistance) / u_maxFogDistance;
        depthFogT = clamp(depthFogT, 0.0, u_maxFog);
        
        float heightFogRange = u_heightFogEnd - u_heightFogStart;
        float heightFogT = ((v_fogWorldPos.y - u_heightFogStart) / heightFogRange);
        heightFogT = clamp(heightFogT, 0.0, u_heightFogDensity);
    
        vec3 heightMixColor = mix(u_heightFogColor, u_fogColor, u_flipFogOrder);
        float heightMixT = mix(heightFogT, depthFogT, u_flipFogOrder);
        finalColor = mix(finalColor, heightMixColor, heightMixT);
        
        vec3 depthMixColor = mix(u_fogColor, u_heightFogColor, u_flipFogOrder);
        float depthMixT = mix(depthFogT, heightFogT, u_flipFogOrder);
        finalColor = mix(finalColor, depthMixColor, depthMixT);
    #endif

    float alpha = 1.0;

    float4 gl_FragColor;
    #ifdef USE_SCREENDOOR_TRANSPARENCY
        gl_FragColor = vec4(finalColor, 1.0);
    #else
        gl_FragColor = vec4(finalColor, alpha);
    #endif
	return gl_FragColor;
}