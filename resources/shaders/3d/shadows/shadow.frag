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
uniform float shadowBias    = 0.0008;
uniform float pcfDiskRadius = 1.0;

const int NUM_SAMPLES = 16;
vec2 poissonDisk[NUM_SAMPLES] = vec2[](
vec2(-0.6134,  0.6176), vec2( 0.5852,  0.5081), vec2( 0.6013, -0.5975), vec2(-0.6302, -0.6041),
vec2(-0.1564,  0.1183), vec2( 0.1569, -0.1253), vec2(-0.1591, -0.2032), vec2( 0.1587,  0.2015),
vec2(-0.3715,  0.3052), vec2( 0.4022, -0.3117), vec2(-0.4076, -0.3388), vec2( 0.3621,  0.3213),
vec2(-0.0870,  0.4639), vec2( 0.1128,  0.4796), vec2(-0.0818, -0.4664), vec2( 0.0901, -0.4872)
);

float PCFShadow(vec4 fragPosLS, vec3 norm)
{
    vec3 projCoords = fragPosLS.xyz / fragPosLS.w;
    projCoords = projCoords * 0.5 + 0.5;

    // Outside the light's frustum, treat as lit.
    if (projCoords.x < 0.0 || projCoords.x > 1.0 ||
    projCoords.y < 0.0 || projCoords.y > 1.0 ||
    projCoords.z > 1.0)
    {
        return 1.0;
    }

    float angleFactor = max(dot(norm, normalize(lightDir)), 0.0);
    float bias = shadowBias * (1.0 - angleFactor);
    float shadowSum = 0.0;

    for (int i = 0; i < NUM_SAMPLES; i++)
    {
        vec2 offset = poissonDisk[i] * pcfDiskRadius / 2048.0;
        shadowSum += texture(shadowMap, vec3(projCoords.xy + offset, projCoords.z - bias));
    }

    return shadowSum / float(NUM_SAMPLES);
}

void main()
{
    vec3 norm = normalize(Normal);
    float shadow = PCFShadow(FragPosLightSpace, norm);

    // debug: show the raw shadow factor
    if (debugShadow)
    {
        FragColor = vec4(vec3(shadow), 1.0);
        return;
    }

    // Use step() to force a binary decision: if shadow < 0.99, it's fully shadowed.
    float isLit = step(0.99, shadow);

    // Calculate standard lighting for lit fragments.
    vec3 viewDir   = normalize(viewPos - FragPos);
    vec3 lightDirN = normalize(lightDir);
    float diff     = max(dot(norm, lightDirN), 0.0);
    vec3 diffuse   = diff * lightColor;
    vec3 lighting  = ambientColor + diffuse;

    vec3 baseColor = texture(diffuseTexture, TexCoord).rgb;

    // If isLit==1.0, use lit color; if 0, use flat black.
    vec3 finalColor = mix(vec3(0.0), baseColor * lighting, isLit);

    FragColor = vec4(finalColor, 1.0);
}
