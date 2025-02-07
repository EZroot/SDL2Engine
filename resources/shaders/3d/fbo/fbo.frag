#version 450 core
in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D screenTexture;
uniform sampler2D godrayTexture;

// Fixed resolution for testing.
const vec2 resolution = vec2(1920.0, 1080.0);

const float FXAA_REDUCE_MIN = 1.0 / 128.0;
const float FXAA_REDUCE_MUL = 1.0 / 8.0;
const float FXAA_SPAN_MAX   = 8.0;

const float bloomThreshold = 1.0; 
const float bloomIntensity = 0.5;

// Chromatic aberration parameters
const float aberrationAmount = 0.001; 

// Tone mapping parameter
const float exposure = .7;

// Vignette parameters
const float vignetteStrength = 0.5; 

// Film grain parameters
const float grainIntensity = 0.05;

//---------------------------------------------------------------------
// FXAA: returns an anti-aliased color by sampling neighboring pixels.
vec3 applyFXAA(vec2 uv)
{
    vec3 rgbNW = texture(screenTexture, uv + (vec2(-1.0, -1.0) / resolution)).rgb;
    vec3 rgbNE = texture(screenTexture, uv + (vec2( 1.0, -1.0) / resolution)).rgb;
    vec3 rgbSW = texture(screenTexture, uv + (vec2(-1.0,  1.0) / resolution)).rgb;
    vec3 rgbSE = texture(screenTexture, uv + (vec2( 1.0,  1.0) / resolution)).rgb;
    vec3 rgbM  = texture(screenTexture, uv).rgb;

    float lumaNW = dot(rgbNW, vec3(0.299, 0.587, 0.114));
    float lumaNE = dot(rgbNE, vec3(0.299, 0.587, 0.114));
    float lumaSW = dot(rgbSW, vec3(0.299, 0.587, 0.114));
    float lumaSE = dot(rgbSE, vec3(0.299, 0.587, 0.114));
    float lumaM  = dot(rgbM,  vec3(0.299, 0.587, 0.114));

    float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
    float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));

    vec2 dir;
    dir.x = -((lumaNW + lumaNE) - (lumaSW + lumaSE));
    dir.y =  ((lumaNW + lumaSW) - (lumaNE + lumaSE));

    float dirReduce = max((lumaNW + lumaNE + lumaSW + lumaSE) * (0.25 * FXAA_REDUCE_MUL), FXAA_REDUCE_MIN);
    float rcpDirMin = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirReduce);
    dir = clamp(dir * rcpDirMin, vec2(-FXAA_SPAN_MAX), vec2(FXAA_SPAN_MAX)) / resolution;

    vec3 rgbA = 0.5 * (
    texture(screenTexture, uv + dir * (1.0/3.0 - 0.5)).rgb +
    texture(screenTexture, uv + dir * (2.0/3.0 - 0.5)).rgb
    );
    vec3 rgbB = rgbA * 0.5 + 0.25 * (
    texture(screenTexture, uv + dir * -0.5).rgb +
    texture(screenTexture, uv + dir * 0.5).rgb
    );
    float lumaB = dot(rgbB, vec3(0.299, 0.587, 0.114));
    return (lumaB < lumaMin || lumaB > lumaMax) ? rgbA : rgbB;
}

vec3 applyBloom(vec2 uv, vec3 baseColor)
{
    vec3 bloomColor = vec3(0.0);
    float count = 0.0;
    // Simple 3x3 kernel.
    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            vec2 offset = vec2(x, y) / resolution;
            vec3 colorSample = texture(screenTexture, uv + offset).rgb;
            float brightness = dot(colorSample, vec3(0.299, 0.587, 0.114));
            if (brightness > bloomThreshold)
            {
                bloomColor += colorSample;
                count += 1.0;
            }
        }
    }
    if (count > 0.0)
    bloomColor /= count;
    return baseColor + bloomColor * bloomIntensity;
}

//---------------------------------------------------------------------
// Chromatic aberration: samples color channels with a slight offset.
vec3 applyChromaticAberration(vec2 uv)
{
    float offset = aberrationAmount;
    float r = texture(screenTexture, uv + vec2(offset, 0.0)).r;
    float g = texture(screenTexture, uv).g;
    float b = texture(screenTexture, uv - vec2(offset, 0.0)).b;
    return vec3(r, g, b);
}

//---------------------------------------------------------------------
// Tone mapping: simple Reinhard tone mapping.
vec3 applyToneMapping(vec3 color)
{
    return color / (color + vec3(1.0));
}

//---------------------------------------------------------------------
// Vignette: darkens the edges.
vec3 applyVignette(vec2 uv, vec3 color)
{
    vec2 pos = uv - vec2(0.5);
    float vig = smoothstep(0.8, 0.4, length(pos));
    return color * mix(vec3(1.0), vec3(vig), vignetteStrength);
}

//---------------------------------------------------------------------
// Film grain: adds pseudo-random noise.
vec3 applyFilmGrain(vec2 uv, vec3 color)
{
    float noise = fract(sin(dot(uv * resolution, vec2(12.9898, 78.233))) * 43758.5453);
    return color + noise * grainIntensity;
}

void main()
{
    vec3 fxaaColor = applyFXAA(TexCoord);
    vec3 caColor = applyChromaticAberration(TexCoord);
    vec3 color = fxaaColor;//mix(fxaaColor, caColor, 0.1);
//    color = applyBloom(TexCoord, color);
    
//    color = applyToneMapping(color * exposure);
//    color = applyVignette(TexCoord, color);
//    color = applyFilmGrain(TexCoord, color);

    vec3 godrayColor = texture(godrayTexture, TexCoord).rgb;
    vec3 finalColor = color + godrayColor;
    
    FragColor = vec4(finalColor, 1.0);
}
