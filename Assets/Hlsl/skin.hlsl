#define MAX_DIRECTIONAL_LIGHT_NUM 4
#define MAX_POINT_LIGHT_NUM 2
#define MAX_SPOT_LIGHT_NUM 3
#define USE_DIFFUSE
#define USE_AMBIENT_COLOR
#define USE_SPECULAR
#define USE_EMISSIVE
#define HAS_DYNAMIC_LIGHT
#define USE_SCREENDOOR_TRANSPARENCY
//#define USE_RIM

//#define USE_FACE_PAINT

//#define USE_UNLIT_DIFFUSE

UnityTexture2D tex_u_colorMap;
UnityTexture2D tex_u_secondaryMaps;
UnityTexture2D tex_u_mask;
UnityTexture2D tex_u_emblemTexture;
UnityTexture2D tex_u_beardTexture;
UnityTexture2D tex_u_blushTexture;
UnityTexture2D tex_u_eyeShadowTexture;
UnityTexture2D tex_u_facePaintTexture;
UnityTexture2D tex_u_frecklesTexture;
UnityTexture2D tex_u_moleTexture;

float3 normal;
float3 eyeDir;
float3 v_absWorldSpacePos;
float3 viewPosition;
float4 gl_FragCoord;

float2 v_texCoords;
float2 v_texCoords1;
float2 v_emblemTexCoords;

float3 computeLighting(float3 normalVector, float3 lightDirection, float3 lightColor, float attenuation)
{
    float diffuse;
    if (IS_SKIN_SHADER || IS_HAIR_SHADER) {
        diffuse = dot(normalVector, lightDirection);
        diffuse = max((diffuse + u_lightAngleBoost) / (1.0 + u_lightAngleBoost), 0.0);
    }
    else {
        diffuse = max(dot(normalVector, lightDirection), 0.0);
    }

    float3 diffuseColor = lightColor * diffuse * attenuation;
    return diffuseColor;
}



//Specular power and intensity are the r and g channels of secondary texture. Optinally, specular power can be a passed in uniform.
float computeSpecularAmount(float3 normalVector, float3 lightDirection, float3 eyeDirection, float specPower)
{
    float3 halfVector = normalize(lightDirection + eyeDirection);
    float specFactor = max(dot(halfVector, normalVector), 0.0);
    
    if (IS_HAIR_SHADER || IS_CLOTH_SHADER) {
        specFactor = pow(specFactor, u_specularity);
    }
    else {
        specFactor = pow(specFactor, specPower);
    }
    return specFactor;
}

float4 main_float(){   
    eyeDir = -eyeDir;

    float shadowVal = 1.0;
    
    //DIFFUSE CALCULATIONS. This is the main differentiation between the various flavors of this shader.
    float3 diffuseColor;
    float3 maskColor;
    if (IS_HOUSEROBE_SHADER) {
        maskColor = tex2D(tex_u_mask, v_texCoords);
        float3 houseColor = clamp((float3(1.0, 1.0, 1.0) - maskColor) + u_robeColor, 0.0, 1.0);
        float3 robeColor = tex2D(tex_u_colorMap, v_texCoords) * houseColor;
        float4 emblemColor = tex2D(tex_u_emblemTexture, v_emblemTexCoords);
        diffuseColor = lerp(robeColor, emblemColor.rgb, emblemColor.a * u_houseSet);
    }
    else if (IS_HOUSECLOTH_SHADER) {
        maskColor = tex2D(tex_u_mask, v_texCoords);
        float3 primaryColor = clamp((float3(1.0 - maskColor.r, 1.0 - maskColor.r, 1.0 - maskColor.r)) + u_primaryColor, 0.0, 1.0);
        float3 secondaryColor = clamp((float3(1.0 - maskColor.g, 1.0 - maskColor.g, 1.0 - maskColor.g)) + u_secondaryColor, 0.0, 1.0);
        float3 combinedColor = primaryColor * secondaryColor;
        diffuseColor = tex2D(tex_u_colorMap, v_texCoords) * combinedColor;
    }
    else if (IS_OUTFIT_SHADER) {
        maskColor = tex2D(tex_u_mask, v_texCoords);
        float clampValue = clamp(1.0 - (maskColor.r + maskColor.g + maskColor.b), 0.0, 1.0);
        float3 maskCombine = float3(clampValue, clampValue, clampValue);
        float3 c1 = u_color1 * maskColor.r;
        float3 c2 = u_color2 * maskColor.g;
        float3 c3 = u_color3 * maskColor.b;
        float3 combinedColor = c1 + c2 + c3 + maskCombine;
        diffuseColor = tex2D(tex_u_colorMap, v_texCoords) * combinedColor;
    }
    else if (IS_AVATAR_HAIR_SHADER) {
        diffuseColor = tex2D(tex_u_colorMap, v_texCoords) * u_hairColor;
    }
    else if (IS_AVATAR_SKIN_SHADER) {
        diffuseColor = tex2D(tex_u_colorMap, v_texCoords) * u_skinColor;
    }
    else if (IS_AVATAR_FACE_SHADER) {
        maskColor = tex2D(tex_u_mask, v_texCoords);
        float3 gradientColor = tex2D(tex_u_colorMap, v_texCoords);
        float3 skinColor = u_skinColor;

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

        float3 maskedColor = lerp(skinColor, float3(1.0, 1.0, 1.0), maskColor.b);
        diffuseColor = lerp(maskedColor, u_browColor, maskColor.g);

        float3 lipColor = u_lipColor;
        lipColor = lipColor * skinColor;

        #ifdef USE_LIPSTICK
                diffuseColor = lerp(diffuseColor, u_lipstickColor, maskColor.r) * gradientColor;
        #else
                diffuseColor = lerp(diffuseColor, lipColor, maskColor.r) * gradientColor;
        #endif

        #ifdef USE_FACIAL_HAIR
                vec4 beardTexColor = texture2D(u_beardTexture, v_texCoords1);
                diffuseColor = mix(diffuseColor, u_beardColor * beardTexColor.r, beardTexColor.a);
        #endif
    }
    else {
        diffuseColor = tex2D(tex_u_colorMap, v_texCoords);
    }

    #if defined(USE_SPECULAR) || defined(USE_RIM) || defined(USE_EMISSIVE)
    float3 specTexel = tex2D(tex_u_secondaryMaps, v_texCoords);
    float specPower = (u_specularity) * (1.0 - specTexel.g);

        #ifdef USE_LIPSTICK
            specPower += (maskColor.r * u_lipSpecularity);
        #endif

        #ifdef USE_FACIAL_HAIR
            specPower = mix(specPower, u_beardSpecularity, beardTexColor.a);
        #endif            
    #endif
    
    #ifdef USE_SPECULAR
    float3 specularColor = float3(0.0, 0.0, 0.0);
    #endif
    
    float3 lightColor =  float3(0.0, 0.0, 0.0);

    #if defined(HAS_DYNAMIC_LIGHT)


        float3 curLightColor;
        float3 ldir;
        float attenuation;
        float3 vertToLight;
        #if (MAX_DIRECTIONAL_LIGHT_NUM > 0)
    
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
                
            #ifdef USE_SPECULAR
            specularColor += computeSpecularAmount(normal, -ldir, -eyeDir, specPower) * curLightColor;
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 1
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[1]);
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
            vertToLight = u_PointLightSourcePosition1 - v_absWorldSpacePos;
            ldir = vertToLight * u_PointLightSourceRangeInverse1;
            attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
            vertToLight = normalize(vertToLight);
            curLightColor = computeLighting(normal, vertToLight, u_PointLightSourceColor1, attenuation);
            lightColor += curLightColor;
    
            #ifdef USE_SPECULAR
            specularColor += computeSpecularAmount(normal, vertToLight, -eyeDir, specPower) * curLightColor;
            #endif
    
            #if MAX_POINT_LIGHT_NUM > 1
                vertToLight = u_PointLightSourcePosition2 - v_absWorldSpacePos;
                ldir = vertToLight * u_PointLightSourceRangeInverse2;
                attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
                vertToLight = normalize(vertToLight);
                curLightColor = computeLighting(normal, vertToLight, u_PointLightSourceColor2, attenuation);
                lightColor += curLightColor;
    
                #ifdef USE_SPECULAR
                specularColor += computeSpecularAmount(normal, vertToLight, -eyeDir, specPower) * curLightColor;
                #endif
    
                #if MAX_POINT_LIGHT_NUM > 2
                    vertToLight = u_PointLightSourcePosition3 - v_absWorldSpacePos;
                    ldir = vertToLight * u_PointLightSourceRangeInverse3;
                    attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
                    vertToLight = normalize(v_vertexToPointLightDirection3);
                    curLightColor = computeLighting(normal, vertToLight, u_PointLightSourceColor3, attenuation);
                    lightColor += curLightColor;
    
                    #ifdef USE_SPECULAR
                    specularColor += computeSpecularAmount(normal, vertToLight, -eyeDir, specPower) * curLightColor;
                    #endif
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
                
            #ifdef USE_SPECULAR
            specularColor += computeSpecularAmount(normal, vertToLight, -eyeDir, specPower) * curLightColor;
            #endif
    
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
                
                #ifdef USE_SPECULAR
                specularColor += computeSpecularAmount(normal, vertToLight, -eyeDir, specPower) * curLightColor;
                #endif
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
                
                #ifdef USE_SPECULAR
                specularColor += computeSpecularAmount(normal, vertToLight, -eyeDir, specPower) * curLightColor;
                #endif
            #endif
        #endif
	#endif



    //Shade component adds additional diffuse to darker areas. Treated as emissive by the debug options (Cloth shader doesn't get this component)
    #ifdef USE_EMISSIVE
        float lambertR = 0.5 * lightColor.r;
        float shadeIntensity = smoothstep(0.0, 1.0, 1.0 - lambertR);
        float3 shadeColor;
        if (IS_SKIN_SHADER) {
            float shadeFactor = shadeIntensity * specTexel.r;
            shadeColor = u_shadeColor * shadeFactor;
            if (IS_AVATAR_SKIN_SHADER || IS_AVATAR_FACE_SHADER) {
                float inverseFactor = 1.0 - shadeFactor;
                shadeColor *= inverseFactor;
            }
        }
        if (IS_HAIR_SHADER){
            shadeColor = u_shadeColor * shadeIntensity;
        }
    #endif
    
    #ifndef USE_DIFFUSE
    diffuseColor = float3(0.0, 0.0, 0.0);
    #endif

    //Rim is a part of diffuse, but if diffuse is cheated off, we still want to show the rim. Blue component of secondary texture is a rim mask.
    #ifdef USE_RIM
        float fresnel = dot(eyeDir, normal);
        fresnel = 1.0 - clamp((fresnel * fresnel * u_rimAngle), 0.0, 1.0);
        fresnel = u_flatness * fresnel;
        if (IS_SKIN_SHADER)
        {
            diffuseColor += u_rimColor * (fresnel * specTexel.b);
        }
        else{
            diffuseColor += u_rimColor * (fresnel * specTexel.r);
        }
    #endif

    float4 facePaintColor;
    if (IS_AVATAR_FACE_SHADER) {
        facePaintColor = tex2D(tex_u_facePaintTexture, v_texCoords1);

        float3 paintPrimary = (u_housePrimary * facePaintColor.g) + (1.0 - facePaintColor.g);
        float3 paintSecondary = (u_houseSecondary * facePaintColor.b) + (1.0 - facePaintColor.b);
        float3 paintCombined = (paintPrimary * paintSecondary) * float3(facePaintColor.r, facePaintColor.r, facePaintColor.r);

        #ifdef USE_FACIAL_HAIR
            float saturation = clamp((1.0 - (maskColor.g + beardTexColor.a)) + 0.2, 0.0, 1.0);
        #else
            float saturation = clamp((1.0 - maskColor.g) + 0.2, 0.0, 1.0);
        #endif

        diffuseColor = lerp(diffuseColor, paintCombined, facePaintColor.a * saturation);
    }
        
    //Add ambient light at the end.
    #ifdef USE_AMBIENT_COLOR
    const float3 desaturateColor = float3(0.3, 0.6, 0.1);
    float amount = dot(u_AmbientLightSourceColor, desaturateColor);
    float3 desaturated = float3(amount, amount, amount);

    lightColor += u_AmbientLightSourceColor*0.5 + desaturated*0.5;
    #endif
    
    //Determine final color, based on diffuse, specular, and emissive.
    float3 finalColor = float3(0.0, 0.0, 0.0);
    float3 faceSpecular;
    #ifdef USE_SPECULAR
        if (IS_HAIR_SHADER) {
            finalColor += (specularColor * u_specColor * specTexel.r);
        }
        else {
            faceSpecular = u_specColor * specTexel.g;

            // Skin specular color shouldn't affect lipstick or facial hair color.
            #ifdef USE_LIPSTICK
                faceSpecular = mix(faceSpecular, float3(maskColor.r * u_lipSpecIntensity), maskColor.r);
            #endif

            #ifdef USE_FACIAL_HAIR
                faceSpecular = mix(faceSpecular, float3(beardTexColor.g), beardTexColor.a);
            #endif

            #ifdef USE_FACE_PAINT
                finalColor += (specularColor * faceSpecular) * (1.0 - facePaintColor.a);
            #else
                finalColor += (specularColor * faceSpecular);
            #endif
        }
    #endif
    
    #if defined(USE_EMISSIVE) 
        if (IS_HAIR_SHADER || IS_SKIN_SHADER) {
            diffuseColor += shadeColor;
        }
    #endif
    
    #ifdef USE_UNLIT_DIFFUSE
    finalColor += diffuseColor;
    #else
    finalColor += diffuseColor * lightColor;

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

    finalColor = pow(finalColor, 2.2);
    #ifdef USE_SCREENDOOR_TRANSPARENCY

        float4x4 thresholdMatrix = {
            1.0f / 17.0f,  9.0f / 17.0f,  3.0f / 17.0f, 11.0f / 17.0f,
            13.0f / 17.0f,  5.0f / 17.0f, 15.0f / 17.0f,  7.0f / 17.0f,
            4.0f / 17.0f, 12.0f / 17.0f,  2.0f / 17.0f, 10.0f / 17.0f,
            16.0f / 17.0f,  8.0f / 17.0f, 14.0f / 17.0f,  6.0f / 17.0f
        };

        float finalAlpha = alpha;

        float threshold = thresholdMatrix[int(fmod(gl_FragCoord.x * 1000.0f, 4.0))][int(fmod(gl_FragCoord.y * 1000.0f, 4.0))];
        if (alpha < threshold) {
            return float4(finalColor, 0.0);
        }
        else {
            return float4(finalColor, alpha);
        }
    #endif

    return float4(finalColor, alpha);
}

void skin_float(

    //Textures
    UnityTexture2D _u_colorMap,
    UnityTexture2D _u_secondaryMaps,
    UnityTexture2D _u_mask,
    UnityTexture2D _u_emblemTexture,
    UnityTexture2D _u_beardTexture,
    UnityTexture2D _u_blushTexture,
    UnityTexture2D _u_eyeShadowTexture,
    UnityTexture2D _u_facePaintTexture,
    UnityTexture2D _u_frecklesTexture,
    UnityTexture2D _u_moleTexture,

    //Data
    float3 _eyeDir,
    float3 _normal,
    float3 _absWorldPosition,
    float3 _viewPosition,
    float4 _screenPosition,

    //Uvs
    float2 uv0,
    float2 uv1,

    //Out
    out float4 gl_FragColor) 
{
    tex_u_colorMap = _u_colorMap;
    tex_u_secondaryMaps = _u_secondaryMaps;
    tex_u_mask = _u_mask;
    tex_u_emblemTexture = _u_emblemTexture;

    tex_u_beardTexture = _u_beardTexture;
    tex_u_blushTexture = _u_blushTexture;
    tex_u_eyeShadowTexture = _u_eyeShadowTexture;
    tex_u_facePaintTexture = _u_facePaintTexture;
    tex_u_frecklesTexture = _u_frecklesTexture;
    tex_u_moleTexture = _u_moleTexture;

    normal = _normal;
    gl_FragCoord = _screenPosition;
    eyeDir = _eyeDir;
    v_absWorldSpacePos = _absWorldPosition;
    viewPosition = _viewPosition;

    v_texCoords = uv0;
    v_texCoords1 = uv1;
    v_emblemTexCoords = uv1 + u_emblemTexOffset;
    gl_FragColor = main_float();
}