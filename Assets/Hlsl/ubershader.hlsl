//#define USE_UNLIT_DIFFUSE
#define USE_DIFFUSE
#define USE_TRANSPARENCY
//#define USE_TRANSPARENCY_TEXTURE
#define USE_ALPHA_TEST
#define HAS_DIRTMAP_TEXTURE
#define USE_AMBIENT_COLOR
#define MAX_DIRECTIONAL_LIGHT_NUM 4
#define HAS_DYNAMIC_LIGHT
#define HAS_EMISSIVE_COMP
#define USE_LIGHTMAP
#define USE_LIGHTMAP_BRIGHTNESS
#define USE_LIGHTMAP_EXPOSURE

//#define HAS_TOP_DIFFUSE_TEXTURE

#define USE_DIFFUSE_COLOR_ADJUST
#define USE_EMISSIVE
#define HAS_EMISSIVE_TEXTURE
#define HAS_EMISSIVE_COMP

#define USE_SPECULAR
#define USE_SPECULAR_RADIUS
#define HAS_SPECULAR_TEXTURE

//#define USE_EMBLEM_IN_DIFFUSE

//#define USE_RIM

float4 v_vertColor;

float4 tex_u_diffuseMap;
float4 tex_u_secondDiffuseMap;
float4 tex_u_thirdDiffuseMap;
float3 tex_u_dirtMap;
float3 tex_u_lightmapMap;
float3 tex_u_emissiveMap;
float4 tex_u_specularMap;
float3 tex_u_rimMap;
float4 tex_u_topDiffuseMap;
float4 tex_u_emblemTexture;
float4 tex_u_teamColorMask;

float3 eyeDir;
float3 normal;

#ifdef HAS_TOP_DIFFUSE_TEXTURE
    float topContrastUtil(float value, float contrast) {
        return (value * contrast) - ((contrast - 1.0) / 2.0);
    }
    #define NEED_LUMA
#endif

#ifdef USE_SHADOWMAP
float randomAngle(vec3 pos, float freq)
{
    float dt = dot((pos * freq), vec3(53.1215, 21.1352, 9.1322));
    return fract(sin(dt) * 2105.2354) * 6.283285;
}
const float ShadowBias = 0.0001;
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
                float curD = texture2D(u_shadowMap, v_shadowTexCoords).r;
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
float3 computeLighting(float3 normalVector, float3 lightDirection, float3 lightColor, float attenuation)
{
    float diffuse = max(dot(normalVector, lightDirection), 0.0);
    float3 diffuseColor = lightColor * diffuse * attenuation;
    
    return diffuseColor;
}

#ifdef USE_SPECULAR
float3 computeSpecularLight(float3 normalVector, float3 lightDirection, float3 eyeDirection)
{
    float3 halfVector = normalize(lightDirection + eyeDirection);
    float specFactor = max(dot(halfVector, normalVector), 0.0);

#ifdef HAS_SPECULAR_TEXTURE
    float4 specTexel = tex_u_specularMap;
#endif
    
#ifdef USE_SPECULAR_RADIUS
    float cosMin = cos(u_specMinRadius);
    float cosMax = cos(u_specMaxRadius);
#else //Must have specular texture.
    float cosMin = specTexel.g;
    float cosMax = specTexel.b;
#endif
    specFactor = clamp((specFactor - cosMax) / (cosMin - cosMax), 0.0, 1.0);
    float specAmount = specFactor*specFactor*specFactor;

#ifdef HAS_SPECULAR_TEXTURE
    specAmount = specAmount * specTexel.r;
#endif
    
    return u_specColor * specAmount;
}
#endif
#endif

#ifdef NEED_LUMA

const float redLuma = 0.3;
const float greenLuma = 0.6;
const float blueLuma = 0.1;
float luma(float3 color)
{
    return redLuma * color.r + greenLuma * color.g + blueLuma * color.b;
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

    float3 diffuseColor;
    #ifdef USE_DIFFUSE
        if (USE_DIFFUSE_COLOR == true){
            diffuseColor = u_diffuseColor;
        }
        float4 diffuseTexColor;
        if (HAS_DIFFUSE_TEXTURE){
            if (HAS_DIFFUSE_TEXTURE) {
                //#ifdef USE_TRANSPARENCY_TEXTURE
                diffuseTexColor = tex_u_diffuseMap;
                #ifdef USE_ALPHA_TEST
                if (UseAsCutout_SWITCH) {
                    if (diffuseTexColor.a < u_alphaTestReferenceValue) {
                        discard;
                    }
                }
                #endif
                if (USE_DIFFUSE_COLOR == false) {
                    diffuseColor = diffuseTexColor.rgb;
                }
            }
            else {
            #if defined(USE_ALPHA_TEST)
                diffuseTexColor = tex_u_diffuseMap;
                if (diffuseTexColor.a < u_alphaTestReferenceValue) {
                    discard;
                }
                if (USE_DIFFUSE_COLOR == false) {
                    diffuseColor = diffuseTexColor.rgb;
                }
            #else 
                if (USE_DIFFUSE_COLOR == false) {
                    diffuseColor = tex_u_diffuseMap;
                }
            #endif
            }
        }

        if (HAS_DIFFUSE_TEXTURE == false){
            if (USE_DIFFUSE_COLOR == false){
                diffuseColor = float3(0.0, 0.0, 0.0);
            }
        }
    
        #ifdef IS_QUIDDITCH_SHADER
            float4 maskColor = tex_u_teamColorMask;
            float3 primaryColor = (u_primaryColor * maskColor.g) + (1.0 - maskColor.g);
            float3 secondaryColor = (u_secondaryColor * maskColor.b) + (1.0 - maskColor.b);
            float3 combinedHouseDiffuse = lerp(diffuseColor, primaryColor * secondaryColor * maskColor.r, maskColor.a);

            #ifdef USE_EMBLEM_IN_DIFFUSE
            diffuseColor = combinedHouseDiffuse;
            #endif

            #ifndef USE_EMBLEM_IN_DIFFUSE
                float4 emblemTexColor;
                if (USE_HOUSE_EMBLEM)
                    emblemTexColor = tex_u_emblemTexture;
            
                if (USE_HOUSE_COLORS) {

                    if (!USE_HOUSE_EMBLEM) {
                        diffuseColor = combinedHouseDiffuse;
                    }
                    else {
                        diffuseColor = lerp(combinedHouseDiffuse, emblemTexColor.rgb, emblemTexColor.a * u_houseSet);
                    }
                }
                else if (USE_HOUSE_EMBLEM) {
                    diffuseColor = lerp(diffuseColor, emblemTexColor.rgb, emblemTexColor.a * u_houseSet);
                }
            #endif

        #endif
        #ifdef USE_DIFFUSE_COLOR_ADJUST
        diffuseColor *= (u_tintColor * u_brightness);
        #endif
   
    #else
        diffuseColor = vec3(0.0);
        #ifdef USE_TRANSPARENCY_TEXTURE
        vec4 diffuseTexColor = vec4(diffuseColor, 1.0);
        #endif
    #endif

    if (HAS_DIFFUSE2_TEXTURE == true){
        float4 diffuse2Color = tex_u_secondDiffuseMap;
        float diffuse2Blend = v_vertColor.r * diffuse2Color.a;
        diffuseColor = lerp(diffuseColor, diffuse2Color.rgb, diffuse2Blend);
    }
    
    if (HAS_DIFFUSE3_TEXTURE == true){
        float4 diffuse3Color = tex_u_thirdDiffuseMap;
        float diffuse3Blend = v_vertColor.g * diffuse3Color.a;
        diffuseColor = lerp(diffuseColor, diffuse3Color.rgb, diffuse3Blend);
    }

    #ifdef HAS_TINT_TEXTURE
        vec3 tintVec = texture2D(u_tintMap, v_tintMapCoords).rgb;
        vec3 tintColor = vec3(0.0);
        #ifdef HAS_TINT_COLOR1
            tintColor += tintVec.r * u_firstTintColor;
        #endif
        #ifdef HAS_TINT_COLOR2
            tintColor += tintVec.g * u_secondTintColor;
        #endif
        #ifdef HAS_TINT_COLOR3
            tintColor += tintVec.b * u_thirdTintColor;
        #endif
        tintColor += vec3(1.0 - clamp(tintVec.r + tintVec.g + tintVec.b, 0.0, 1.0));
        diffuseColor *= tintColor;
    #endif

    #ifdef NEEDS_NORMALS
        vec3 eyeDir = normalize(v_eyeSpacePos);
        #ifdef HAS_NORMAL_TEXTURE
        vec3 normal = normalize((2.0 * texture2D(u_normalMap, v_normalCoords).xyz - 1.0) * vec3(u_normalMapHeight,u_normalMapHeight,1.0));
        #else
        vec3 normal = normalize(v_normal);
        #endif
    #endif
    
    #ifdef HAS_TOP_DIFFUSE_TEXTURE
        float4 topDiffuseColor = tex_u_topDiffuseMap;
        
        float topDiffuseLuma = luma(topDiffuseColor.rgb);
        float topDiffuseBlend = clamp(topContrastUtil(topDiffuseLuma + u_topAlphaBoost, u_topAlphaContrast) + topContrastUtil(normal.y, u_topFalloffContrast), 0.0, 1.0);
        diffuseColor = lerp(diffuseColor, topDiffuseColor.rgb, topDiffuseBlend);
    #endif
    
    #ifdef HAS_DIRTMAP_TEXTURE
    diffuseColor *= tex_u_dirtMap.rgb;
    #endif
    
    #ifdef USE_SPECULAR
    float3 specularColor = float3(0.0, 0.0, 0.0);
    #endif

    float3 lightColor;

    if (BakedDiffuse > 0.5){
        lightColor = float3(0.0, 0.0, 0.0);
    }
    else{
        #if defined(USE_AMBIENT_COLOR)
            lightColor = u_AmbientLightSourceColor;
        #elif defined(USE_COLOR_UNIFORM)
            lightColor = u_color.rgb;
        #else
            lightColor = float3(0.0, 0.0, 0.0);
        #endif
    }
    //Rim and possibly reflection use fresnel
    #if defined(USE_RIM) || defined(USE_REFLECTION_ANGLE)
    float fresnel = dot(eyeDir, normal);
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
    
        #ifdef USE_SPECULAR
            #ifdef HAS_SPECULAR_TEXTURE
                float specIntensity = texture2D(u_specularMap, v_specularCoords).r;
                specularColor = v_specColor.rgb * specIntensity;
                #ifdef USE_SHADOWMAP
                    specularColor += (v_specColor.w * specIntensity) * shadowLight * u_specColor;
                #endif
            #else
                specularColor = v_specColor.rgb;
                #ifdef USE_SHADOWMAP
                    specularColor += v_specColor.w * shadowLight * u_specColor;
                #endif
            #endif
        #endif
    #endif

    //Do lighting calculations. Loops are manually unrolled because old devices seem to be incapable of doing this for some damn reason...
    #if defined(HAS_DYNAMIC_LIGHT) && !defined(USE_VERTEX_LIGHTING)
        float3 curLightColor;
        float3 ldir;
        float attenuation;
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
    
            if (BakedDiffuse != 1.0){
                lightColor += curLightColor;
            }
    
            #ifdef USE_SPECULAR
            specularColor += computeSpecularLight(normal, -ldir, -eyeDir) * curLightColor;
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 1
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[1]);
                #else
                ldir = normalize(u_DirLightSourceDirection2);
                #endif
                
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor2, 1.0);
                if (BakedDiffuse != 1.0){
                    lightColor += curLightColor;
                }
    
                #ifdef USE_SPECULAR
                specularColor += computeSpecularLight(normal, -ldir, -eyeDir) * curLightColor;
                #endif
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 2
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[2]);
                #else
                ldir = normalize(u_DirLightSourceDirection3);
                #endif
                
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor3, 1.0);
                if (BakedDiffuse != 1.0){
                    lightColor += curLightColor;
                }
    
                #ifdef USE_SPECULAR
                specularColor += computeSpecularLight(normal, -ldir, -eyeDir) * curLightColor;
                #endif
            #endif
    
            #if MAX_DIRECTIONAL_LIGHT_NUM > 3
                #ifdef HAS_NORMAL_TEXTURE
                ldir = normalize(v_dirLightDirection[3]);
                #else
                ldir = normalize(u_DirLightSourceDirection4);
                #endif
                
                curLightColor = computeLighting(normal, -ldir, u_DirLightSourceColor4, 1.0);
                if (BakedDiffuse != 1.0){
                    lightColor += curLightColor;
                }
    
                #ifdef USE_SPECULAR
                specularColor += computeSpecularLight(normal, -ldir, -eyeDir) * curLightColor;
                #endif
            #endif
        #endif

        #if (MAX_POINT_LIGHT_NUM > 0)
            ldir = v_vertexToPointLightDirection[0] * u_PointLightSourceRangeInverse[0];
            attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
            vec3 vertToLight = normalize(v_vertexToPointLightDirection[0]);
            curLightColor = computeLighting(normal, vertToLight, u_PointLightSourceColor[0], attenuation);
    
            if (BakedDiffuse != 1.0){
                #if NUM_SCENE_POINT_LIGHTS <= 0
                lightColor += curLightColor;
                #endif
            }
    
            #ifdef USE_SPECULAR
            specularColor += computeSpecularLight(normal, vertToLight, -eyeDir) * curLightColor;
            #endif
    
            #if MAX_POINT_LIGHT_NUM > 1
                ldir = v_vertexToPointLightDirection[1] * u_PointLightSourceRangeInverse[1];
                attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
                vertToLight = normalize(v_vertexToPointLightDirection[1]);
                curLightColor = computeLighting(normal, vertToLight, u_PointLightSourceColor[1], attenuation);
    
                if (BakedDiffuse != 1.0){
                    #if NUM_SCENE_POINT_LIGHTS <= 1
                    lightColor += curLightColor;
                    #endif
                }
    
                #ifdef USE_SPECULAR
                specularColor += computeSpecularLight(normal, vertToLight, -eyeDir) * curLightColor;
                #endif
    
                #if MAX_POINT_LIGHT_NUM > 2
                    ldir = v_vertexToPointLightDirection[2] * u_PointLightSourceRangeInverse[2];
                    attenuation = clamp(1.0 - dot(ldir, ldir), 0.0, 1.0);
                    vertToLight = normalize(v_vertexToPointLightDirection[2]);
                    curLightColor = computeLighting(normal, vertToLight, u_PointLightSourceColor[2], attenuation);
    
                    if (BakedDiffuse != 1.0){
                        #if NUM_SCENE_POINT_LIGHTS <= 2
                        lightColor += curLightColor;
                        #endif
                    }
    
                    #ifdef USE_SPECULAR
                    specularColor += computeSpecularLight(normal, vertToLight, -eyeDir) * curLightColor;
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

            if (BakedDiffuse != 0.0){
                #if NUM_SCENE_SPOT_LIGHTS <= 0
                lightColor += curLightColor;
                #endif
            }
    
            #ifdef USE_SPECULAR
            specularColor += computeSpecularLight(normal, vertexToSpotLightDirection, -eyeDir) * curLightColor;
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
                if (BakedDiffuse == 1.0){
                    #if NUM_SCENE_SPOT_LIGHTS <= 1
                    lightColor += curLightColor;
                    #endif
                }
    
                #ifdef USE_SPECULAR
                specularColor += computeSpecularLight(normal, vertexToSpotLightDirection, -eyeDir) * curLightColor;
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
                if (BakedDiffuse != 1.0){
                    #if NUM_SCENE_SPOT_LIGHTS <= 2
                    lightColor += curLightColor;
                    #endif
                }
    
                #ifdef USE_SPECULAR
                specularColor += computeSpecularLight(normal, vertexToSpotLightDirection, -eyeDir) * curLightColor;
                #endif
            #endif
        #endif
    #endif

    //Calculate emissive component, based on emmisive, rim, and lightmap
    #ifdef HAS_EMISSIVE_COMP
        float3 emissiveComponent = float3(0.0, 0.0, 0.0);
    #endif

    #ifdef USE_LIGHTMAP
        if (HAS_LIGHTMAP_TEXTURE == true){
            #ifdef USE_LIGHTMAP_BRIGHTNESS
                #ifdef USE_LIGHTMAP_EXPOSURE
                    float3 lightMapValue = u_lmPower * tex_u_lightmapMap;
                    lightMapValue = pow(lightMapValue, float3(u_lmExposure, u_lmExposure, u_lmExposure));
                    
                    emissiveComponent += diffuseColor * lightMapValue;
                #else
                    emissiveComponent += u_lmPower * (diffuseColor * tex_u_lightmapMap);
                #endif
            #else
                #ifdef USE_LIGHTMAP_EXPOSURE
                    float3 lightMapValue = 4.0 * tex_u_lightmapMap;
                    lightMapValue = pow(lightMapValue, float3(u_lmExposure, u_lmExposure, u_lmExposure));
    
    
                    emissiveComponent += diffuseColor * lightMapValue;
                #else
                    emissiveComponent += 4.0 * (diffuseColor * tex_u_lightmapMap);
                #endif
            #endif
            //nothing uses second lightmap
            #ifdef HAS_SECOND_LIGHTMAP_TEXTURE
                vec3 lightmap2Value = u_lmPower2 * (texture2D(u_lightmapMap2, v_lightmapCoords).rgb);
                #ifdef USE_LIGHTMAP2_EXPOSURE
                    lightmap2Value = pow(lightmap2Value, vec3(u_lmExposure2));
                #else
                    lightmap2Value = diffuseColor * lightmap2Value;
                #endif
                #ifdef USE_FLICKER
                    float actualFlickerIntensity = u_flickerIntensity / 4.0;
                    float flicker1 = sin(CC_Time.y * u_flicker1Speed) * actualFlickerIntensity + .5 - actualFlickerIntensity;
                    float flicker2 = sin(CC_Time.y * u_flicker2Speed) * actualFlickerIntensity + .5 - actualFlickerIntensity;
                    float flicker = (flicker1 + flicker2);
                    lightmap2Value = flicker * lightmap2Value;
                #endif
                emissiveComponent += lightmap2Value;
            #endif
        }
        else if (USE_LIGHTMAP_COLOR == true){
            #ifdef USE_LIGHTMAP_BRIGHTNESS
                #ifdef USE_LIGHTMAP_EXPOSURE
                    float3 lightMapValue = u_lmPower * u_lightmapColor;
                    lightMapValue = pow(lightMapValue, float3(u_lmExposure, u_lmExposure, u_lmExposure));
                    emissiveComponent += diffuseColor * lightMapValue;
                #else
                    emissiveComponent += u_lmPower * (diffuseColor * u_lightmapColor);
                #endif
            #else
                #ifdef USE_LIGHTMAP_EXPOSURE
                    vec3 lightMapValue = 4.0 * u_lightmapColor;
                    lightMapValue = pow(lightMapValue, vec3(u_lmExposure);
                    emissiveComponent += diffuseColor * lightMapValue;
                #else
                    emissiveComponent += 4.0 * (diffuseColor * u_lightmapColor);
                #endif
            #endif
        }
    #endif

        #ifdef USE_EMISSIVE
        #if defined(HAS_EMISSIVE_TEXTURE)
            #ifdef USE_EMISSIVE_VERT_COLOR
                emissiveComponent += (texture2D(u_emissiveMap, v_emissiveCoords).rgb * v_vertColor.g);
            #else
                emissiveComponent += tex_u_emissiveMap;
            #endif
        #elif defined(USE_EMISSIVE_COLOR)
            #ifdef USE_EMISSIVE_VERT_COLOR
                emissiveComponent += (u_emissiveColor * v_vertColor.g);
            #else
                emissiveComponent += u_emissiveColor;
            #endif
        #endif
    #endif
    
    #ifdef USE_RIM
        #ifdef HAS_RIM_FALLOFF
            #ifdef INVERT_RIM
                float rimAmount = clamp(pow(fresnel * fresnel * u_rimAngle, u_rimFalloff), 0.0, 1.0);
            #else
                float rimAmount = 1.0 - clamp(pow(fresnel * fresnel * u_rimAngle, u_rimFalloff), 0.0, 1.0);
            #endif
        #else
            #ifdef INVERT_RIM
                float rimAmount = clamp((fresnel * fresnel * u_rimAngle), 0.0, 1.0);
            #else
                float rimAmount = 1.0 - clamp((fresnel * fresnel * u_rimAngle), 0.0, 1.0);
            #endif
        #endif
        
    
        #ifdef USE_RIM_VERT_COLOR
            rimAmount *= v_vertColor.b;
        #endif
    
        #if defined(HAS_RIM_TEXTURE)
            emissiveComponent += (texture2D(u_rimMap, v_rimMapCoords).rgb) * rimAmount;
        #elif defined(USE_RIM_COLOR)
            emissiveComponent += u_rimColor * rimAmount;
        #endif
    #endif

    #ifdef HAS_REFLECTION_TEXTURE
        float reflectIntensity = u_reflectionAmount;
        vec3 reflectTexel = textureCube(u_reflectionCubeMap, v_reflectVector).rgb;
        #ifdef USE_REFLECTION_ANGLE
            float reflectFresnel = 1.0 - clamp((fresnel * fresnel * u_reflectionAngle), 0.0, 1.0);
            reflectIntensity *= reflectFresnel;
        #endif
        #ifdef USE_REFLECTION_TINT
            reflectTexel *= u_reflectionTintColor;
        #endif
    #endif

    const float3 desaturateColor = float3(0.3, 0.6, 0.1);

    //Handle transparency. If using cutout, then modulate the emissive, specular, and reflection.
    #ifdef USE_TRANSPARENCY
        //float opacity = 1.0;//diffuseTexColor.a;
        float opacity = u_opacityAmount;
        if (HAS_DIFFUSE_TEXTURE)
            opacity = diffuseTexColor.a;
        /*#if defined(USE_TRANSPARENCY_TEXTURE) && defined(USE_TRANSPARENCY_VERTEX)
            float opacity = diffuseTexColor.a * v_vertColor.a;

        #elif defined(USE_TRANSPARENCY_VERTEX)
            float opacity = v_vertColor.a;
        #else
            float opacity = u_opacityAmount;
        #endif
        if (HAS_DIFFUSE_TEXTURE) {
            //#elif defined(USE_TRANSPARENCY_TEXTURE)
            float opacity = diffuseTexColor.a;
        }*/
        #ifdef USE_TRANSPARENCY_CUTOUT
            #ifdef USE_SPECULAR
            specularColor *= opacity;
            #endif
    
            #ifdef HAS_EMISSIVE_COMP
            emissiveComponent *= opacity;
            #endif
    
            #ifdef HAS_REFLECTION_TEXTURE
            reflectIntensity *= opacity;
            #endif

            #ifdef USE_DIFFUSE
            diffuseColor *= opacity;
            #endif

        #endif
    
        //Add to the opacity based on specular, rim, and reflection.
        float opacityBoost = 0.0;
        #ifdef USE_SPECULAR
            opacityBoost = dot(specularColor, desaturateColor);
        #endif
        #ifdef USE_RIM
            opacityBoost = max(opacityBoost, rimAmount);
        #endif
        #ifdef HAS_REFLECTION_TEXTURE
            vec3 finalReflect = reflectTexel * reflectIntensity;
            float reflectDesaturate = dot(finalReflect, desaturateColor);
            opacityBoost = max(opacityBoost, reflectDesaturate);
        #endif
        opacity = clamp(opacity + opacityBoost, opacity, 1.0);
    #else
        float opacity = 0.0;
    #endif

    //Determine final color, based on diffuse lighting, specular, emissive, and reflection.
    float3 finalColor = float3(0.0, 0.0, 0.0);
    
    #ifdef USE_SPECULAR
        finalColor += specularColor;
    #endif
    
    #ifdef HAS_EMISSIVE_COMP
        finalColor += emissiveComponent;
    #endif
    
    #ifdef HAS_REFLECTION_TEXTURE
        #ifndef USE_TRANSPARENCY
        vec3 finalReflect = reflectTexel * reflectIntensity;
        #endif
        finalColor += finalReflect;
        diffuseColor = diffuseColor * (1.0 - reflectIntensity);
    #endif
    
    #ifdef USE_UNLIT_DIFFUSE
        finalColor += diffuseColor;
    #else
        finalColor += diffuseColor * (lightColor);
    #endif

    #ifdef USE_FOG
        #ifdef NEEDS_NORMALS
        float v_eyeSpaceDepth = -v_eyeSpacePos.z;
        #endif
        float depthFogT = (v_eyeSpaceDepth - u_minFogDistance) / u_maxFogDistance;
        depthFogT = clamp(depthFogT, 0.0, u_maxFog);
        
        float heightFogRange = u_heightFogEnd - u_heightFogStart;
        float heightFogT = ((v_fogWorldPos.y - u_heightFogStart) / heightFogRange);
        heightFogT = clamp(heightFogT, 0.0, u_heightFogDensity);
    
        vec3 heightMixColor = mix(u_heightFogColor, u_fogColor, u_flipFogOrder);
        float heightMixT = mix(heightFogT, depthFogT, u_flipFogOrder);
        finalColor = mix(finalColor, heightMixColor*opacity, heightMixT);
        
        vec3 depthMixColor = mix(u_fogColor, u_heightFogColor, u_flipFogOrder);
        float depthMixT = mix(depthFogT, heightFogT, u_flipFogOrder);
        finalColor = mix(finalColor, depthMixColor*opacity, depthMixT);
    #endif

    #ifdef USE_SCREENDOOR_TRANSPARENCY
        float finalAlpha = opacity * alpha;
        #ifdef RANDOM_DITHER_FALLBACK
            if (HAS_DIFFUSE_TEXTURE == true) {
                if (finalAlpha < rand(v_diffuseCoords)) {
                    discard;
                }
            }
        #else
            if (alpha < thresholdMatrix[int(mod(gl_FragCoord.x, 4.0))][int(mod(gl_FragCoord.y, 4.0))]) {
                discard;
            }
        #endif
        else
        {
            gl_FragColor = vec4(finalColor, 1);
        }
    #else
        return float4(finalColor, opacity * alpha);
    #endif

}


void ubershader_float(float4 _u_diffuseMap, float4 _u_secondDiffuseMap, float4 _u_thirdDiffuseMap, float3 _u_dirtMap, float3 _u_lightmapMap, float3 _u_emissiveMap, float4 _u_specularMap, float3 _u_rimMap, float4 _u_topDiffuseMap, float4 _v_vertColor, float3 _eyeDir, float3 _normal, out float4 gl_FragColor){
    tex_u_diffuseMap = _u_diffuseMap;
    tex_u_secondDiffuseMap = _u_secondDiffuseMap;
    tex_u_thirdDiffuseMap = _u_thirdDiffuseMap;
    tex_u_dirtMap = _u_dirtMap;
    tex_u_lightmapMap = _u_lightmapMap;
    tex_u_emissiveMap = _u_emissiveMap;
    tex_u_rimMap = _u_rimMap;
    tex_u_specularMap = _u_specularMap;
    tex_u_topDiffuseMap = _u_topDiffuseMap;
    v_vertColor = _v_vertColor;
    eyeDir = _eyeDir;
    normal = _normal;
    gl_FragColor = main_float();
}

void quidditchshader_float(float4 _u_diffuseMap, float4 _u_secondDiffuseMap, float4 _u_thirdDiffuseMap, float3 _u_dirtMap, float3 _u_lightmapMap, float3 _u_emissiveMap, float4 _u_specularMap, float3 _u_rimMap, float4 _u_topDiffuseMap, float4 _u_emblemTexture, float4 _u_teamColorMask, float4 _v_vertColor, float3 _eyeDir, float3 _normal, out float4 gl_FragColor){
    tex_u_diffuseMap = _u_diffuseMap;
    tex_u_secondDiffuseMap = _u_secondDiffuseMap;
    tex_u_thirdDiffuseMap = _u_thirdDiffuseMap;
    tex_u_dirtMap = _u_dirtMap;
    tex_u_lightmapMap = _u_lightmapMap;
    tex_u_emissiveMap = _u_emissiveMap;
    tex_u_rimMap = _u_rimMap;
    tex_u_specularMap = _u_specularMap;
    tex_u_topDiffuseMap = _u_topDiffuseMap;
    tex_u_emblemTexture = _u_emblemTexture;
    tex_u_teamColorMask = _u_teamColorMask;
    v_vertColor = _v_vertColor;
    eyeDir = _eyeDir;
    normal = _normal;
    gl_FragColor = main_float();
}