#define vec4 float4
#define vec3 float3
#define vec2 float2
#define mix lerp
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

#define USE_DIFFUSE

float4 processHMShader(FragInputs input, LayerTexCoord layerTexCoord){

    float shadowVal = 1.0;

    vec3 gradient = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(u_diffuse), sampler_u_diffuse, layerTexCoord.base).rgb;
    vec3 maskColor = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(u_mask), sampler_u_mask, layerTexCoord.base).rgb;
    vec3 diffuseColor = mix(vec3(1.0, 1.0, 1.0), u_diffuseColor, maskColor.g) * gradient;
    //Darken based on head-space normal.
    //vec3 headNorm = normalize(v_headNormal);



    //vec4 gl_FragColor = vec4(1.0, 1.0, 1.0, 0.0);
    vec4 gl_FragColor = vec4(diffuseColor, 1.0);
	return gl_FragColor;
}