#define USE_DIFFUSE
#define USE_AMBIENT_COLOR
#define USE_SPECULAR
#define USE_RIM
#define USE_EMISSIVE
#define IS_SKIN_SHADER
#define HAS_DYNAMIC_LIGHT
//#define USE_UNLIT_DIFFUSE
#define MAX_DIRECTIONAL_LIGHT_NUM 4
#define vec4 float4
#define vec3 float3
#define vec2 float2

#if ((MAX_DIRECTIONAL_LIGHT_NUM > 0) || (MAX_POINT_LIGHT_NUM > 0) || (MAX_SPOT_LIGHT_NUM > 0))
    #define HAS_DYNAMIC_LIGHT
    #if defined(USE_DIRLIGHT_SHADOWMAP) || defined(USE_SPOTLIGHT_SHADOWMAP)
        #define USE_SHADOWMAP
    #endif
#endif

/*float fract(float x)
{
    return x - floor(x)
}*/

#ifdef USE_SHADOWMAP
float randomAngle(vec3 pos, float freq)
{
    float dt = dot((pos * freq), vec3(53.1215, 21.1352, 9.1322));
    return dt;
    //return fract(sin(dt) * 2105.2354) * 6.283285;
}

const float ShadowBias = 0.01;
float shadowValForKernelPos(vec2 pos, float depth, int index)
{
    float shadowAmount = 0.0;
    #ifdef USE_POISSON_DISK
        #ifdef USE_ROTATED_DISK
        float rotAngle = randomAngle(vec3(pos, depth), 15.0);
        float sinAngle = sin(rotAngle);
        float cosAngle = cos(rotAngle);
        #endif
    
        float radius = u_diskRadius / float(index+1);
        for (int i = 0; i < NUM_DISK_SAMPLES; ++i)
        {
            vec2 coordOffset = u_diskKernel[i] * (radius * u_shadowTexelSize);
            #ifdef USE_ROTATED_DISK
            coordOffset = vec2(coordOffset.x*cosAngle - coordOffset.y*sinAngle, coordOffset.x*sinAngle + coordOffset.y*cosAngle);
            #endif
            
            float curD = texture2D(u_shadowMap, pos + coordOffset).r;
            float depthDelta = (depth-ShadowBias) - curD;
            shadowAmount += float(depthDelta <= 0.0);
        }
    
        shadowAmount /= float(NUM_DISK_SAMPLES);
    
    #else
        //We can avoid dependent texture read on directional light
        #ifdef USE_DIRLIGHT_SHADOWMAP
            #ifdef USE_CASCADES
                #ifdef USE_SHADOW_SAMPLER
                shadowAmount = shadow2DEXT(u_shadowMap, v_shadowTexCoords[index]);
                #else
                float curD = texture2D(u_shadowMap, v_shadowTexCoords[index].xy).r;
                shadowAmount = float(depth - ShadowBias <= curD);
                #endif
            #else
                #ifdef USE_SHADOW_SAMPLER
                shadowAmount = shadow2DEXT(u_shadowMap, v_shadowTexCoords);
                #else
                float curD = 1.0;
                shadowAmount = float(depth - ShadowBias <= curD);
                #endif
            #endif
        #else
            //Gotta use values passed in for spotlight, due to w component.
            #ifdef USE_SHADOW_SAMPLER
            shadowAmount = shadow2DEXT(u_shadowMap, vec3(pos, depth));
            #else
            float curD = texture2D(u_shadowMap, pos).r;
            shadowAmount = float(depth - ShadowBias <= curD);
            #endif
        #endif
    #endif
    const float minLight = 0.3;
    return clamp((shadowAmount - minLight) / (1.0 - minLight), minLight, 1.0);
}
#endif

#if defined(HAS_DYNAMIC_LIGHT) && !defined(USE_VERTEX_LIGHTING)
vec3 computeLighting(vec3 normalVector, vec3 lightDirection, vec3 lightColor, float attenuation)
{
    #if defined(IS_SKIN_SHADER) || defined(IS_HAIR_SHADER)
    float diffuse = dot(normalVector, lightDirection);
    diffuse = max((diffuse + u_lightAngleBoost) / (1.0 + u_lightAngleBoost), 0.0);
    #else
    float diffuse = max(dot(normalVector, lightDirection), 0.0);
    #endif
    vec3 diffuseColor = lightColor * diffuse * attenuation;
    
    return diffuseColor;
}

#ifdef USE_SPECULAR
//Specular power and intensity are the r and g channels of secondary texture. Optinally, specular power can be a passed in uniform.
float computeSpecularAmount(vec3 normalVector, vec3 lightDirection, vec3 eyeDirection, float specPower)
{
    vec3 halfVector = normalize(lightDirection + eyeDirection);
    float specFactor = max(dot(halfVector, normalVector), 0.0);
    
#if defined(IS_HAIR_SHADER) || defined(IS_CLOTH_SHADER)
    specFactor = pow(specFactor, u_specularity);
#else
    specFactor = pow(specFactor, specPower);
#endif
    
    return specFactor;
}
#endif
#endif

float4 processHMShader(FragInputs input, LayerTexCoord layerTexCoord){
	float4 gl_FragColor;
    vec4 v_lightColor = vec4(1.0, 0.0, 0.0, 1.0);
    vec4 v_specColor = vec4(0.0, 0.0, 0.0, 1.0);


    #ifdef USE_SCREENDOOR_TRANSPARENCY
        #ifdef RANDOM_DITHER_FALLBACK
            if (alpha < rand(v_texCoords)) {
                discard;
            }
        #else
            if (alpha < thresholdMatrix[int(mod(gl_FragCoord.x, 4.0))][int(mod(gl_FragCoord.y, 4.0))]) {
                discard;
            }
        #endif
    #endif
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

    vec3 eyeDir = normalize(input.positionRWS);


    vec3 normalws = layerTexCoord.vertexNormalWS;
    vec3 triplanarWeights = layerTexCoord.triplanarWeights;

    vec3 normal = vec3(-normalws.x * triplanarWeights.x, normalws.y * triplanarWeights.y, normalws.z * triplanarWeights.z);

	
        //DIFFUSE CALCULATIONS. This is the main differentiation between the various flavors of this shader.
    #if defined(IS_HOUSEROBE_SHADER)
        vec3 maskColor = texture2D(u_mask, v_texCoords).rgb;
        vec3 houseColor = clamp((vec3(1.0) - maskColor) + u_robeColor, 0.0, 1.0);
        vec3 robeColor = texture2D(u_colorMap, v_texCoords).rgb * houseColor;
        vec4 emblemColor = texture2D(u_emblemTexture, v_emblemTexCoords);
        vec3 diffuseColor = mix(robeColor, emblemColor.rgb, emblemColor.a * u_houseSet);
    #elif defined(IS_HOUSECLOTH_SHADER)
        vec3 maskColor = texture2D(u_mask, v_texCoords).rgb;
        vec3 primaryColor = clamp((vec3(1.0 - maskColor.r)) + u_primaryColor, 0.0, 1.0);
        vec3 secondaryColor = clamp((vec3(1.0 - maskColor.g)) + u_secondaryColor, 0.0, 1.0);
        vec3 combinedColor = primaryColor * secondaryColor;
        vec3 diffuseColor = texture2D(u_colorMap, v_texCoords).rgb * combinedColor;
    #elif defined(IS_OUTFIT_SHADER)
        vec3 maskColor = texture2D(u_mask, v_texCoords).rgb;
        vec3 maskCombine = vec3(clamp(1.0 - (maskColor.r + maskColor.g + maskColor.b), 0.0, 1.0));
        vec3 c1 = u_color1 * maskColor.r;
        vec3 c2 = u_color2 * maskColor.g;
        vec3 c3 = u_color3 * maskColor.b;
        vec3 combinedColor = c1 + c2 + c3 + maskCombine;
        vec3 diffuseColor = texture2D(u_colorMap, v_texCoords).rgb * combinedColor;
    #elif defined(IS_AVATAR_HAIR_SHADER)
        vec3 diffuseColor = texture2D(u_colorMap, v_texCoords).rgb * u_hairColor;
    #elif defined(IS_AVATAR_SKIN_SHADER)
        vec3 diffuseColor = texture2D(u_colorMap, v_texCoords).rgb * u_skinColor;
    #elif defined(IS_AVATAR_FACE_SHADER)
        vec3 maskColor = texture2D(u_mask, v_texCoords).rgb;
        vec3 gradientColor = texture2D(u_colorMap, v_texCoords).rgb;
        vec3 skinColor = u_skinColor;

        #ifdef USE_FRECKLES
            vec3 frecklesTexColor = texture2D(u_frecklesTexture, v_texCoords1).rgb;
            skinColor = mix(u_freckleColor, skinColor, frecklesTexColor.r);
        #endif

        #ifdef USE_BLUSH
            skinColor = mix(skinColor, u_blushColor, texture2D(u_blushTexture, v_texCoords).r);
        #endif

        #ifdef USE_EYESHADOW
            skinColor = mix(skinColor, u_eyeShadowColor, texture2D(u_eyeShadowTexture, v_texCoords1).g);
        #endif

        #ifdef USE_BEAUTY_MARK
            vec2 moleTexCoords = ((v_texCoords1 + u_moleOffset) * u_moleScale) - ((u_moleScale / 2.0) - 0.5);
            vec4 moleTexColor = texture2D(u_moleTexture, moleTexCoords);
            skinColor = mix(skinColor, moleTexColor.rgb, moleTexColor.a);
        #endif
    
        vec3 maskedColor = mix(skinColor, vec3(1.0), maskColor.b);
        vec3 diffuseColor = mix(maskedColor, u_browColor, maskColor.g);

        #ifdef USE_LIPSTICK
            diffuseColor = mix(diffuseColor, u_lipstickColor, maskColor.r) * gradientColor;
        #else
            diffuseColor = mix(diffuseColor, u_lipColor, maskColor.r) * gradientColor;
        #endif

       #ifdef USE_FACIAL_HAIR
            vec4 beardTexColor = texture2D(u_beardTexture, v_texCoords1);
            diffuseColor = mix(diffuseColor, u_beardColor * beardTexColor.r, beardTexColor.a);
        #endif
    #else
        vec3 diffuseColor = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(u_colorMap), sampler_u_colorMap, layerTexCoord.base).rgb;
    #endif

    #if defined(USE_SPECULAR) || defined(USE_RIM) || defined(USE_EMISSIVE)
    vec3 specTexel = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(u_secondaryMaps), sampler_u_secondaryMaps, layerTexCoord.base).rgb;
    float specPower = (u_specularity) * (1.0 - specTexel.g);

        #ifdef USE_LIPSTICK
            specPower += (maskColor.r * u_lipSpecularity);
        #endif

        #ifdef USE_FACIAL_HAIR
            specPower = mix(specPower, u_beardSpecularity, beardTexColor.a);
        #endif            
    #endif

	
    #ifdef USE_SPECULAR
    vec3 specularColor = vec3(0.0, 0.0, 0.0);
    #endif

    vec3 lightColor = vec3(0.0, 0.0, 0.0);
    #if defined(HAS_DYNAMIC_LIGHT) && defined(USE_VERTEX_LIGHTING)
        lightColor += v_lightColor.rgb;
        #if defined(USE_DIRLIGHT_SHADOWMAP)
            vec3 shadowLight = u_DirLightSourceColor[0] * (v_lightColor.w * shadowVal);
            lightColor += shadowLight;
        #elif defined(USE_SPOTLIGHT_SHADOWMAP)
            vec3 shadowLight = u_SpotLightSourceColor[0] * (v_lightColor.w * shadowVal);
            lightColor += shadowLight;
        #endif
            
        #ifdef USE_SPECULAR
            specularColor = v_specColor.rgb;
            #ifdef USE_SHADOWMAP
                specularColor += (v_specColor.w) * shadowLight;
            #endif
        #endif
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
                
            #ifdef USE_SPECULAR
            specularColor += computeSpecularAmount(normal, -ldir, -eyeDir, specPower) * curLightColor;
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 1
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(u_DirLightSourceDirection2);
                #else
                ldir = normalize(u_DirLightSourceDirection2);
                #endif

                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor2, 1.0);
                lightColor += curLightColor;
                
                #ifdef USE_SPECULAR
                specularColor += computeSpecularAmount(normal, -ldir, -eyeDir, specPower) * curLightColor;
                #endif
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 2
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[2]);
                #else
                ldir = normalize(u_DirLightSourceDirection3);
                #endif
                
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor3, 1.0);
                lightColor += curLightColor;
                
                #ifdef USE_SPECULAR
                specularColor += computeSpecularAmount(normal, -ldir, -eyeDir, specPower) * curLightColor;
                #endif
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 3
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[3]);
                #else
                ldir = normalize(u_DirLightSourceDirection4);
                #endif
                
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor4, 1.0);
                lightColor += curLightColor;
                
                #ifdef USE_SPECULAR
                specularColor += computeSpecularAmount(normal, -ldir, -eyeDir, specPower) * curLightColor;
                #endif
            #endif
        #endif

        #if (MAX_POINT_LIGHT_NUM > 0)
            ldir = v_vertexToPointLightDirection[0] * u_PointLightSourceRangeInverse[0];
            attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
            vec3 vertToLight = normalize(v_vertexToPointLightDirection[0]);
            curLightColor = computeLighting(normal, vertToLight, u_PointLightSourceColor[0], attenuation);
            lightColor += curLightColor;
    
            #ifdef USE_SPECULAR
            specularColor += computeSpecularAmount(normal, vertToLight, -eyeDir, specPower) * curLightColor;
            #endif
    
            #if MAX_POINT_LIGHT_NUM > 1
                ldir = v_vertexToPointLightDirection[1] * u_PointLightSourceRangeInverse[1];
                attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
                vertToLight = normalize(v_vertexToPointLightDirection[1]);
                curLightColor = computeLighting(normal, vertToLight, u_PointLightSourceColor[1], attenuation);
                lightColor += curLightColor;
    
                #ifdef USE_SPECULAR
                specularColor += computeSpecularAmount(normal, vertToLight, -eyeDir, specPower) * curLightColor;
                #endif
    
                #if MAX_POINT_LIGHT_NUM > 2
                    ldir = v_vertexToPointLightDirection[2] * u_PointLightSourceRangeInverse[2];
                    attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
                    vertToLight = normalize(v_vertexToPointLightDirection[2]);
                    curLightColor = computeLighting(normal, vertToLight, u_PointLightSourceColor[2], attenuation);
                    lightColor += curLightColor;
    
                    #ifdef USE_SPECULAR
                    specularColor += computeSpecularAmount(normal, vertToLight, -eyeDir, specPower) * curLightColor;
                    #endif
                #endif
            #endif
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
                
            #ifdef USE_SPECULAR
            specularColor += computeSpecularAmount(normal, vertexToSpotLightDirection, -eyeDir, specPower) * curLightColor;
            #endif
    
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
                
                #ifdef USE_SPECULAR
                specularColor += computeSpecularAmount(normal, vertexToSpotLightDirection, -eyeDir, specPower) * curLightColor;
                #endif
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
                
                #ifdef USE_SPECULAR
                specularColor += computeSpecularAmount(normal, vertexToSpotLightDirection, -eyeDir, specPower) * curLightColor;
                #endif
            #endif
        #endif
    #endif

    //Shade component adds additional diffuse to darker areas. Treated as emissive by the debug options (Cloth shader doesn't get this component)
    #ifdef USE_EMISSIVE
        float lambertR = 0.5 * lightColor.r;
        float shadeIntensity = smoothstep(0.0, 1.0, 1.0 - lambertR);
        #if defined(IS_SKIN_SHADER)
            float shadeFactor = shadeIntensity * specTexel.r;
            vec3 shadeColor = u_shadeColor * shadeFactor;
            #if defined(IS_AVATAR_SKIN_SHADER) || defined(IS_AVATAR_FACE_SHADER)
                float inverseFactor = 1.0 - shadeFactor;
                shadeColor *= inverseFactor;
            #endif
        #elif defined(IS_HAIR_SHADER)
            vec3 shadeColor = u_shadeColor * shadeIntensity;
        #endif
    #endif
    
    #ifndef USE_DIFFUSE
    diffuseColor = vec3(0.0, 0.0, 0.0);
    #endif

    //Rim is a part of diffuse, but if diffuse is cheated off, we still want to show the rim. Blue component of secondary texture is a rim mask.
    #ifdef USE_RIM
        float fresnel = dot(eyeDir, normal);
        fresnel = 1.0 - clamp((fresnel * fresnel * u_rimAngle), 0.0, 1.0);
        fresnel = u_flatness * fresnel;
        #ifdef IS_SKIN_SHADER
            diffuseColor += u_rimColor * (fresnel * specTexel.b);
        #else
            diffuseColor += u_rimColor * (fresnel * specTexel.r);
        #endif
    #endif

    #if defined(IS_AVATAR_FACE_SHADER) && defined(USE_FACE_PAINT)
        vec4 facePaintColor = texture2D(u_facePaintTexture, v_texCoords1);

        vec3 paintPrimary = (u_housePrimary * facePaintColor.g) + (1.0 - facePaintColor.g);
        vec3 paintSecondary = (u_houseSecondary * facePaintColor.b) + (1.0 - facePaintColor.b);
        vec3 paintCombined = (paintPrimary * paintSecondary) * vec3(facePaintColor.r);

        #ifdef USE_FACIAL_HAIR
        float saturation = clamp((1.0 - (maskColor.g + beardTexColor.a)) + 0.2, 0.0, 1.0);
        #else
        float saturation = clamp((1.0 - maskColor.g) + 0.2, 0.0, 1.0);
        #endif

        diffuseColor = mix(diffuseColor, paintCombined, facePaintColor.a * saturation);
    #endif

    //Add ambient light at the end.
    #ifdef USE_AMBIENT_COLOR
    const vec3 desaturateColor = vec3(0.3, 0.6, 0.1);
    float amount = dot(u_AmbientLightSourceColor, desaturateColor);
    vec3 desaturated = vec3(amount, amount, amount);
    
    lightColor += u_AmbientLightSourceColor*0.5 + desaturated*0.5;
    #endif

    //Determine final color, based on diffuse, specular, and emissive.
    vec3 finalColor = vec3(0.0, 0.0, 0.0);
    #ifdef USE_SPECULAR
        #ifdef IS_HAIR_SHADER
            finalColor += (specularColor * u_specColor * specTexel.r);
        #else
            vec3 faceSpecular = u_specColor * specTexel.g;

            // Skin specular color shouldn't affect lipstick or facial hair color.
            #ifdef USE_LIPSTICK
                faceSpecular = mix(faceSpecular, vec3(maskColor.r * u_lipSpecIntensity), maskColor.r);
            #endif

            #ifdef USE_FACIAL_HAIR
                faceSpecular = mix(faceSpecular, vec3(beardTexColor.g), beardTexColor.a);
            #endif

            #ifdef USE_FACE_PAINT
                finalColor += (specularColor * faceSpecular) * (1.0 - facePaintColor.a);
            #else
                finalColor += (specularColor * faceSpecular);
            #endif
        #endif
    #endif

    #if defined(USE_EMISSIVE) && (defined(IS_HAIR_SHADER) || defined(IS_SKIN_SHADER))
    diffuseColor += shadeColor;
    #endif
    
    #ifdef USE_UNLIT_DIFFUSE
    finalColor += diffuseColor;
    #else
    finalColor += diffuseColor * lightColor;
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



    #ifdef USE_SCREENDOOR_TRANSPARENCY
        gl_FragColor = vec4(normal, 1.0);
    #else
        gl_FragColor = vec4(finalColor, alpha);
    #endif

	return gl_FragColor;
}