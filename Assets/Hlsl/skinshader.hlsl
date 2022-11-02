#define MAX_DIRECTIONAL_LIGHT_NUM 4
#define USE_DIFFUSE
#define USE_AMBIENT_COLOR
#define USE_SPECULAR
#define USE_EMISSIVE
#define HAS_DYNAMIC_LIGHT
//#define USE_UNLIT_DIFFUSE
//#define USE_NEW_NORM

float BlendMode_Overlay(float base, float blend)
{
    if (base <= 0.0)
        return 0.0;
    if (blend >= 1.0)
        return 1.0;
    else
        return min(1.0, base / (1.0 - blend));
}

float3 BlendMode_Overlay(float3 base, float3 blend)
{
    return float3(BlendMode_Overlay(base.r, blend.r),
        BlendMode_Overlay(base.g, blend.g),
        BlendMode_Overlay(base.b, blend.b));
}



#if defined(HAS_DYNAMIC_LIGHT) && !defined(USE_VERTEX_LIGHTING)
float3 computeLighting(float3 normalVector, float3 lightDirection, float3 lightColor, float attenuation)
{
#if defined(IS_SKIN_SHADER) || defined(IS_HAIR_SHADER)
    float diffuse = dot(normalVector, lightDirection);
    diffuse = max((diffuse + u_lightAngleBoost) / (1.0 + u_lightAngleBoost), 0.0);
#else
    float diffuse = max(dot(normalVector, lightDirection), 0.0);
#endif
    float3 diffuseColor = lightColor * diffuse * attenuation;
    
    return diffuseColor;
}



#ifdef USE_SPECULAR
//Specular power and intensity are the r and g channels of secondary texture. Optinally, specular power can be a passed in uniform.
float computeSpecularAmount(float3 normalVector, float3 lightDirection, float3 eyeDirection, float specPower)
{
    float3 halfVector = normalize(lightDirection + eyeDirection);
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


float3 normal;
float3 eyeDir;

float3 tex_u_colorMap;
float3 tex_u_secondaryMaps;

float3 tex_u_mask;
float4 tex_u_emblemTexture;

float4 tex_u_facePaintTexture;

float Unity_Saturation_float(float3 In, float Saturation)
{
    float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
    return (luma.xxx + Saturation.xxx * (In - luma.xxx));
}

float3 desaturatte(float3 color, float Desaturation)
{
	float3 grayXfer = float3(0.3, 0.6, 0.1);
	float grayf = dot(grayXfer, color);
	float3 gray = float3(grayf, grayf, grayf);
	return lerp(color, gray, Desaturation) + float3(0.2, 0.2, 0.2);
}

float4 main_float(){
	
    float3 blendWeights = abs(normal);
    blendWeights = (blendWeights - 0.2);
    blendWeights = blendWeights * blendWeights * blendWeights;
    blendWeights = max(blendWeights, real3(0.0, 0.0, 0.0));
    blendWeights /= dot(blendWeights, 1.0);

    #ifdef USE_NEW_NORM
    float3 new_normal = float3(normal.x * blendWeights.x, normal.y * blendWeights.y, normal.z * blendWeights.z);
    #else
    float3 new_normal = normal;
    #endif


                
    float3 lowered_normal = normal;
    
    eyeDir = -eyeDir;


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
    
        //float3 eyeDir = normalize(v_eyeSpacePos);
        //float3 normal = normalize(v_normal);

        //DIFFUSE CALCULATIONS. This is the main differentiation between the various flavors of this shader.
    #if defined(IS_HOUSEROBE_SHADER)
        float3 maskColor = tex_u_mask;
        float3 houseColor = clamp((float3(1.0, 1.0, 1.0) - maskColor) + u_robeColor, 0.0, 1.0);
        float3 robeColor = tex_u_colorMap * houseColor;
        float4 emblemColor = tex_u_emblemTexture;
        float3 diffuseColor = lerp(robeColor, emblemColor.rgb, emblemColor.a * u_houseSet);
    #elif defined(IS_HOUSECLOTH_SHADER)
        float3 maskColor = tex_u_mask;
        float3 primaryColor = clamp((float3(1.0 - maskColor.r, 1.0 - maskColor.r, 1.0 - maskColor.r)) + u_primaryColor, 0.0, 1.0);
        float3 secondaryColor = clamp((float3(1.0 - maskColor.g, 1.0 - maskColor.g, 1.0 - maskColor.g)) + u_secondaryColor, 0.0, 1.0);
        float3 combinedColor = primaryColor * secondaryColor;
        float3 diffuseColor = tex_u_colorMap * combinedColor;
    #elif defined(IS_OUTFIT_SHADER)
        float3 maskColor = tex_u_mask;
        float clampValue = clamp(1.0 - (maskColor.r + maskColor.g + maskColor.b), 0.0, 1.0);
        float3 maskCombine = float3(clampValue, clampValue, clampValue);
        float3 c1 = u_color1 * maskColor.r;
        float3 c2 = u_color2 * maskColor.g;
        float3 c3 = u_color3 * maskColor.b;
        float3 combinedColor = c1 + c2 + c3 + maskCombine;
        float3 diffuseColor = tex_u_colorMap * combinedColor;
    #elif defined(IS_AVATAR_HAIR_SHADER)
        float3 diffuseColor = tex_u_colorMap * u_hairColor;
    #elif defined(IS_AVATAR_SKIN_SHADER)
        float3 diffuseColor = tex_u_colorMap * desaturatte(u_skinColor, 0.8);
    #elif defined(IS_AVATAR_FACE_SHADER)
        float3 maskColor = tex_u_mask;
        float3 gradientColor = tex_u_colorMap;
        float3 skinColor = desaturatte(u_skinColor, 0.7);

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
        float3 diffuseColor = lerp(maskedColor, u_browColor, maskColor.g);

        #ifdef USE_LIPSTICK
            diffuseColor = lerp(diffuseColor, u_lipstickColor, maskColor.r) * gradientColor;
        #else
            diffuseColor = lerp(diffuseColor, u_lipColor, maskColor.r) * gradientColor;
        #endif

       #ifdef USE_FACIAL_HAIR
            vec4 beardTexColor = texture2D(u_beardTexture, v_texCoords1);
            diffuseColor = mix(diffuseColor, u_beardColor * beardTexColor.r, beardTexColor.a);
        #endif
    #else
        float3 diffuseColor = tex_u_colorMap;
    #endif

    #if defined(USE_SPECULAR) || defined(USE_RIM) || defined(USE_EMISSIVE)
    float3 specTexel = tex_u_secondaryMaps;
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

    //This should all be gamma
    #if defined(HAS_DYNAMIC_LIGHT)
        //u_DirLightSourceColor1 = pow(u_DirLightSourceColor1, 1 / 2.2);
        //u_DirLightSourceColor2 = pow(u_DirLightSourceColor2, 1 / 2.2);
        //u_DirLightSourceColor3 = pow(u_DirLightSourceColor3, 1 / 2.2);
        //u_DirLightSourceColor4 = pow(u_DirLightSourceColor4, 1 / 2.2);

        float3 curLightColor;
        float3 ldir;
        float attenuation;
        #if (MAX_DIRECTIONAL_LIGHT_NUM > 0)
    
            #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[0]);
            #else
                ldir = normalize(u_DirLightSourceDirection1);
            #endif
    
            #ifdef USE_DIRLIGHT_SHADOWMAP
                curLightColor = computeLighting(new_normal, -ldir, u_DirLightSourceColor[0], 1.0) * shadowVal;
            #else
                curLightColor = computeLighting(new_normal, -ldir, u_DirLightSourceColor1, 1.0);
            #endif
    
            lightColor += curLightColor;
                
            #ifdef USE_SPECULAR
            specularColor += computeSpecularAmount(lowered_normal, -ldir, -eyeDir, specPower) * curLightColor;
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 1
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[1]);
                #else
                ldir = normalize(u_DirLightSourceDirection2);
                #endif

                curLightColor = computeLighting(new_normal, -ldir, u_DirLightSourceColor2, 1.0);
                lightColor += curLightColor;
                
                #ifdef USE_SPECULAR
                specularColor += computeSpecularAmount(lowered_normal, -ldir, -eyeDir, specPower) * curLightColor;
                #endif
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 2
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[2]);
                #else
                ldir = normalize(u_DirLightSourceDirection3);
                #endif
                
                curLightColor = computeLighting(new_normal, -ldir, u_DirLightSourceColor3, 1.0);
                lightColor += curLightColor;
                
                #ifdef USE_SPECULAR
                specularColor += computeSpecularAmount(lowered_normal, -ldir, -eyeDir, specPower) * curLightColor;
                #endif
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 3
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[3]);
                #else
                ldir = normalize(u_DirLightSourceDirection4);
                #endif
                
                curLightColor = computeLighting(new_normal, -ldir, u_DirLightSourceColor4, 1.0);
                lightColor += curLightColor;
                
                #ifdef USE_SPECULAR
                specularColor += computeSpecularAmount(lowered_normal, -ldir, -eyeDir, specPower) * curLightColor;
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
            float3 shadeColor = u_shadeColor * shadeFactor;
            #if defined(IS_AVATAR_SKIN_SHADER) || defined(IS_AVATAR_FACE_SHADER)
                float inverseFactor = 1.0 - shadeFactor;
                shadeColor *= inverseFactor;
            #endif
        #elif defined(IS_HAIR_SHADER)
            float3 shadeColor = u_shadeColor * shadeIntensity;
        #endif

    #endif
    
    #ifndef USE_DIFFUSE
    diffuseColor = float3(0.0, 0.0, 0.0);
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

    #if defined(IS_AVATAR_FACE_SHADER)
        float4 facePaintColor = tex_u_facePaintTexture;

        float3 paintPrimary = (u_housePrimary * facePaintColor.g) + (1.0 - facePaintColor.g);
        float3 paintSecondary = (u_houseSecondary * facePaintColor.b) + (1.0 - facePaintColor.b);
        float3 paintCombined = (paintPrimary * paintSecondary) * float3(facePaintColor.r, facePaintColor.r, facePaintColor.r);

        #ifdef USE_FACIAL_HAIR
        float saturation = clamp((1.0 - (maskColor.g + beardTexColor.a)) + 0.2, 0.0, 1.0);
        #else
        float saturation = clamp((1.0 - maskColor.g) + 0.2, 0.0, 1.0);
        #endif

        diffuseColor = lerp(diffuseColor, paintCombined, facePaintColor.a * saturation);
    #endif
        
    //Add ambient light at the end.
    #ifdef USE_AMBIENT_COLOR

    //u_AmbientLightSourceColor = pow(u_AmbientLightSourceColor, 1 / 2.2);

    const float3 desaturateColor = float3(0.3, 0.6, 0.1);
    float amount = dot(u_AmbientLightSourceColor, desaturateColor);
    float3 desaturated = float3(amount, amount, amount);

    lightColor += u_AmbientLightSourceColor*0.5 + desaturated*0.5;
    #endif
    
    //Determine final color, based on diffuse, specular, and emissive.
    float3 finalColor = float3(0.0, 0.0, 0.0);
    #ifdef USE_SPECULAR
        #ifdef IS_HAIR_SHADER
            finalColor += (specularColor * u_specColor * specTexel.r);
        #else
            float3 faceSpecular = u_specColor * specTexel.g;

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
        #endif
    #endif
    
    #if defined(USE_EMISSIVE) && (defined(IS_HAIR_SHADER) || defined(IS_SKIN_SHADER))
        //diffuseColor = lerp(olddiffuse, olddiffuse * 0.2 + (olddiffuse + shadeColor) * shadeColor * 20 + shadeColor * 10 * colormax, colormax * 15);

        //diffuseColor = pow(diffuseColor, 1/ 2.2);
        //shadeColor = pow(shadeColor,1/ 2.2);

        diffuseColor += shadeColor;

        //diffuseColor = pow(diffuseColor, 2.2);
    #endif
    
    #ifdef USE_UNLIT_DIFFUSE
    finalColor += diffuseColor;
    #else
    //diffuseColor = pow(diffuseColor, 1 / 2.2);
    //finalColor = pow(finalColor, 1 / 2.2);
    finalColor += diffuseColor * lightColor;
    //finalColor = pow(finalColor, 2.2);

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

        /*float s;
        if (finalColor.r <= 0.0031308) {
            s = finalColor.r * 12.92;
        }
        else {
            s = 1.055 * pow(finalColor.r, 1.0 / 2.4) - 0.055;
        }
        finalColor.r = s;

        if (finalColor.g <= 0.0031308) {
            s = finalColor.g * 12.92;
        }
        else {
            s = 1.055 * pow(finalColor.g, 1.0 / 2.4) - 0.055;
        }
        finalColor.g = s;

        if (finalColor.b <= 0.0031308) {
            s = finalColor.b * 12.92;
        }
        else {
            s = 1.055 * pow(finalColor.b, 1.0 / 2.4) - 0.055;
        }
        finalColor.b = s;

        finalColor = pow(finalColor, 2.2);*/
        finalColor = pow(finalColor, 2.2);


        return float4(finalColor, 1.0);
}



void skinshader_float(float3 _u_colorMap, float3 _u_secondaryMaps, float3 _normal, float3 _eyeDir, out float4 gl_FragColor){
    normal = _normal;
    eyeDir = _eyeDir;
    tex_u_colorMap = _u_colorMap;
    tex_u_secondaryMaps = _u_secondaryMaps;
    gl_FragColor = main_float();
}

void hairshader_float(float3 _u_colorMap, float3 _u_secondaryMaps, float3 _normal, float3 _eyeDir, out float4 gl_FragColor){
    normal = _normal;
    eyeDir = _eyeDir;
    tex_u_colorMap = _u_colorMap;
    tex_u_secondaryMaps = _u_secondaryMaps;

    gl_FragColor = main_float();
}

void clothshader_float(float3 _u_colorMap, float3 _u_secondaryMaps, float3 _normal, float3 _eyeDir, out float4 gl_FragColor){
    normal = _normal;
    eyeDir = _eyeDir;
    tex_u_colorMap = _u_colorMap;
    tex_u_secondaryMaps = _u_secondaryMaps;

    gl_FragColor = main_float();
}

void houserobeshader_float(float3 _u_colorMap, float3 _u_secondaryMaps, float3 _u_mask, float4 u_emblemTexture, float3 _normal, float3 _eyeDir, out float4 gl_FragColor){
    normal = _normal;
    eyeDir = _eyeDir;
    tex_u_mask = _u_mask;
    tex_u_emblemTexture = u_emblemTexture;
    tex_u_colorMap = _u_colorMap;
    tex_u_secondaryMaps = _u_secondaryMaps;

    gl_FragColor = main_float();
}

void houseclothshader_float(float3 _u_colorMap, float3 _u_secondaryMaps, float3 _u_mask, float3 _normal, float3 _eyeDir, out float4 gl_FragColor){
    normal = _normal;
    eyeDir = _eyeDir;
    tex_u_mask = _u_mask;
    tex_u_colorMap = _u_colorMap;
    tex_u_secondaryMaps = _u_secondaryMaps;

    gl_FragColor = main_float();
}

void avatarfaceshader_float(float3 _u_colorMap, float3 _u_secondaryMaps, float3 _u_mask, float4 _u_facePaintTexture, float3 _normal, float3 _eyeDir, out float4 gl_FragColor){
    normal = _normal;
    eyeDir = _eyeDir;
    tex_u_colorMap = _u_colorMap;
    tex_u_secondaryMaps = _u_secondaryMaps;
    tex_u_mask = _u_mask;
    tex_u_facePaintTexture = _u_facePaintTexture;

    gl_FragColor = main_float();
}

void avatarskinshader_float(float3 _u_colorMap, float3 _u_secondaryMaps, float3 _normal, float3 _eyeDir, out float4 gl_FragColor){
    normal = _normal;
    eyeDir = _eyeDir;
    tex_u_colorMap = _u_colorMap;
    tex_u_secondaryMaps = _u_secondaryMaps;

    gl_FragColor = main_float();
}

void avatarhairshader_float(float3 _u_colorMap, float3 _u_secondaryMaps, float3 _normal, float3 _eyeDir, out float4 gl_FragColor){
    normal = _normal;
    eyeDir = _eyeDir;
    tex_u_colorMap = _u_colorMap;
    tex_u_secondaryMaps = _u_secondaryMaps;

    gl_FragColor = main_float();
}

void outfitshader_float(float3 _u_colorMap, float3 _u_secondaryMaps, float3 _u_mask, float3 _normal, float3 _eyeDir, out float4 gl_FragColor){
    normal = _normal;
    eyeDir = _eyeDir;
    tex_u_mask = _u_mask;
    tex_u_colorMap = _u_colorMap;
    tex_u_secondaryMaps = _u_secondaryMaps;

    gl_FragColor = main_float();
}