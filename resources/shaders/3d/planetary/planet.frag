#version 450 core

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
in vec4 FragPosLightSpace;

out vec4 FragColor;

uniform sampler2D planetTexture;
uniform sampler2D cloudTexture;
uniform sampler2DShadow shadowMap;

uniform vec3 lightDir;
uniform vec3 viewPos;
uniform vec3 lightColor;
uniform vec3 ambientColor;
uniform float shininess;
uniform float cloudBlend;
uniform float shadowBias;
uniform float pcfDiskRadius;
uniform float atmosphereIntensity;
uniform float rimPower;
uniform float subsurfaceScatterIntensity;
uniform vec3 subsurfaceColor;
uniform vec3 rimColor;
uniform float terminatorSoftness; // Controls the blend between light and dark

const int NUM_SAMPLES = 16;
vec2 poissonDisk[NUM_SAMPLES] = vec2[](
vec2(-0.6134,  0.6176), vec2( 0.5852,  0.5081), vec2( 0.6013, -0.5975), vec2(-0.6302, -0.6041),
vec2(-0.1564,  0.1183), vec2( 0.1569, -0.1253), vec2(-0.1591, -0.2032), vec2( 0.1587,  0.2015),
vec2(-0.3715,  0.3052), vec2( 0.4022, -0.3117), vec2(-0.4076, -0.3388), vec2( 0.3621,  0.3213),
vec2(-0.0870,  0.4639), vec2( 0.1128,  0.4796), vec2(-0.0818, -0.4664), vec2( 0.0901, -0.4872)
);

float PCFShadow(vec4 fragPosLS, vec3 norm) {
    vec3 projCoords = fragPosLS.xyz / fragPosLS.w;
    projCoords = projCoords * 0.5 + 0.5;

    if (projCoords.x < 0.0 || projCoords.x > 1.0 ||
    projCoords.y < 0.0 || projCoords.y > 1.0 ||
    projCoords.z > 1.0) {
        return 1.0;
    }

    float angleFactor = max(dot(norm, normalize(lightDir)), 0.0);
    float bias = shadowBias * (1.0 - angleFactor);
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    float shadowSum = 0.0;

    for (int i = 0; i < NUM_SAMPLES; i++) {
        vec2 offset = poissonDisk[i] * pcfDiskRadius * texelSize;
        shadowSum += texture(shadowMap, vec3(projCoords.xy + offset, projCoords.z - bias));
    }

    return shadowSum / float(NUM_SAMPLES);
}

// Improved smooth terminator transition
float smoothLighting(float NdotL) {
    return smoothstep(-terminatorSoftness, terminatorSoftness, NdotL);
}

// Atmospheric scattering softens the terminator
vec3 computeAtmosphere(vec3 normal, vec3 viewDir, float NdotL) {
    float fresnel = pow(1.0 - max(dot(normal, viewDir), 0.0), 3.0);
    float atmosphereBlend = smoothLighting(NdotL); // Blend in shadowed areas
    return atmosphereIntensity * vec3(0.4, 0.6, 1.0) * fresnel * atmosphereBlend;
}

// Subsurface scattering adds depth
vec3 computeSubsurfaceScattering(vec3 normal, vec3 lightDir) {
    float scatterFactor = max(dot(-normal, lightDir), 0.0);
    return subsurfaceScatterIntensity * scatterFactor * subsurfaceColor;
}

// Rim lighting
vec3 computeRimLighting(vec3 normal, vec3 viewDir) {
    float rim = pow(1.0 - max(dot(normal, viewDir), 0.0), rimPower);
    return rim * rimColor;
}

void main() {
    vec3 norm = normalize(Normal);
    vec3 L = normalize(lightDir);
    vec3 V = normalize(viewPos - FragPos);
    vec3 H = normalize(L + V);

    float NdotL = dot(norm, L);
    float diff = smoothLighting(NdotL); // Smooth terminator transition
    vec3 diffuse = diff * lightColor;

    // Specular lighting (Blinn-Phong)
    float spec = pow(max(dot(norm, H), 0.0), shininess);
    vec3 specular = spec * lightColor;

    // Shadow calculation
    float shadowFactor = PCFShadow(FragPosLightSpace, norm);

    // Base planet texture with cloud blend
    vec3 baseColor = texture(planetTexture, TexCoord).rgb;
    vec4 cloudSample = texture(cloudTexture, TexCoord);
    baseColor = mix(baseColor, vec3(1.0), cloudBlend * cloudSample.r);

    // Apply lighting with soft shadow factor
    vec3 lighting = ambientColor + shadowFactor * (diffuse + specular);
    vec3 finalColor = lighting * baseColor;

    // Compute additional effects
    vec3 atmosphere = computeAtmosphere(norm, V, NdotL);
    vec3 subsurface = computeSubsurfaceScattering(norm, L);
    vec3 rimLight = computeRimLighting(norm, V);

    // Apply effects
    finalColor += atmosphere + subsurface + rimLight;

    FragColor = vec4(finalColor, 1.0);
}
