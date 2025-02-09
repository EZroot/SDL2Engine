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
uniform float shadowBias    = 0.002;
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

    // Outside the light's frustum.
    if (projCoords.x < 0.0 || projCoords.x > 1.0 ||
    projCoords.y < 0.0 || projCoords.y > 1.0 ||
    projCoords.z > 1.0)
    {
        return 1.0;
    }

    float angleFactor = max(dot(norm, normalize(lightDir)), 0.0);
    float bias = shadowBias * (1.0 - angleFactor);

    // Dynamically compute texel size.
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    float shadowSum = 0.0;

    for (int i = 0; i < NUM_SAMPLES; i++)
    {
        vec2 offset = poissonDisk[i] * pcfDiskRadius * texelSize;
        shadowSum += texture(shadowMap, vec3(projCoords.xy + offset, projCoords.z - bias));
    }

    return shadowSum / float(NUM_SAMPLES);
}

void main()
{
    vec3 norm = normalize(Normal);
    float shadow = PCFShadow(FragPosLightSpace, norm);

    // show raw shadow factor
    if (debugShadow)
    {
        FragColor = vec4(vec3(shadow), 1.0);
        return;
    }

    // (Blinn-Phong)
    vec3 viewDir   = normalize(viewPos - FragPos);
    vec3 lightDirN = normalize(lightDir);

    // diffuse
    float diff = max(dot(norm, lightDirN), 0.0);
    vec3 diffuse = diff * lightColor;

    // specular
    vec3 halfwayDir = normalize(lightDirN + viewDir);
    float spec = pow(max(dot(norm, halfwayDir), 0.0), 16.0); // 32 = shininess
    vec3 specular = spec * lightColor;

    // combined: ambient is always applied, diffuse and specular are shadowed.
    vec3 ambient = ambientColor;
    vec3 lighting = ambient + shadow * diffuse + specular;

    vec3 baseColor = texture(diffuseTexture, TexCoord).rgb;
    vec3 finalColor = baseColor * lighting;

//    vec3 projCoords = FragPosLightSpace.xyz / FragPosLightSpace.w;
//    projCoords = projCoords * 0.5 + 0.5;
//    FragColor = vec4(projCoords, 1.0);
    
    FragColor = vec4(finalColor, 1.0);
}
