void calculateNewOpacity_float(float opacity, float alpha, float4 gl_FragCoord, out float new_opacity) {
    float4x4 thresholdMatrix2 = {
        1.0f / 17.0f,  9.0f / 17.0f,  3.0f / 17.0f, 11.0f / 17.0f,
        13.0f / 17.0f,  5.0f / 17.0f, 15.0f / 17.0f,  7.0f / 17.0f,
        4.0f / 17.0f, 12.0f / 17.0f,  2.0f / 17.0f, 10.0f / 17.0f,
        16.0f / 17.0f,  8.0f / 17.0f, 14.0f / 17.0f,  6.0f / 17.0f
    };
    float threshold = thresholdMatrix2[int(fmod(gl_FragCoord.x * 1000.0f, 4.0))][int(fmod(gl_FragCoord.y * 1000.0f, 4.0))];
    if (alpha < threshold) {
        new_opacity = 0;
    }
    else {
        new_opacity = opacity;
    }
}

