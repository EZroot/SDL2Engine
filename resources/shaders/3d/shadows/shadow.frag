#version 450 core

layout(location = 0) in vec3 FragPos;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;
layout(location = 3) in vec4 FragPosLightSpace;

layout(location = 0) out vec4 FragColor;

uniform sampler2D       diffuseTexture;
uniform sampler2DShadow shadowMap;

uniform vec3 lightDir;
uniform vec3 lightColor;
uniform vec3 ambientColor;
uniform vec3 viewPos;

uniform bool debugShadow;
uniform float shadowBias = 0.0008;
uniform float pcfDiskRadius = 1.0;

const int NUM_SAMPLES = 25;  // Increased number of samples
const float RANDOMNESS = 25.0; // Adjust this value to add randomness to the shadow edges

vec2 rotate(vec2 v, float a) {
    float s = sin(a);
    float c = cos(a);
    return vec2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float PCFShadow(vec4 fragPosLS, vec3 norm) {
    vec3 projCoords = fragPosLS.xyz / fragPosLS.w;
    projCoords = projCoords * 0.5 + 0.5;

    if (projCoords.x < 0.0 || projCoords.x > 1.0 ||
    projCoords.y < 0.0 || projCoords.y > 1.0 ||
    projCoords.z > 1.0) {
        return 1.0;
    }

    float normalAdjustment = max(dot(norm, normalize(lightDir)) * 0.1, 0.05);
    float bias = shadowBias + normalAdjustment;

    float shadow = 0.0;
    float totalWeight = 0.0;
    for (int i = 0; i < NUM_SAMPLES; ++i) {
        float angle = float(i) * 6.28318 / RANDOMNESS;
        vec2 offset = rotate(vec2(cos(angle), sin(angle)) * pcfDiskRadius, angle);
        float sampleDepth = texture(shadowMap, vec3(projCoords.xy + offset / 2048.0, projCoords.z - bias)).r;
        float weight = max(0.0, 1.0 - length(offset) / pcfDiskRadius);
        shadow += sampleDepth * weight;
        totalWeight += weight;
    }
    shadow /= totalWeight;

    return shadow;
}

void main() {
    vec3 norm = normalize(Normal);
    //norm = floor(norm * 1.0 + 0.5) / 1.0;  // Quantize normal vectors for pixelated lighting

    float shadow = PCFShadow(FragPosLightSpace, norm);
    if (debugShadow) {
        FragColor = vec4(vec3(shadow), 1.0);
        return;
    }

    float isLit = step(0.99, shadow);
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 lightDirN = normalize(lightDir);
    float diff = max(dot(norm, lightDirN), 0.0);
    vec3 diffuse = diff * lightColor;
    vec3 lighting = ambientColor + diffuse;
    vec3 baseColor = texture(diffuseTexture, TexCoord).rgb;
    vec3 finalColor = mix(vec3(0.0), baseColor * lighting, isLit);

    FragColor = vec4(finalColor, 1.0);
}
