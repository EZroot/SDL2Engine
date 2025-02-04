#version 450 core

layout(location = 0) in vec3 FragPos;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;
layout(location = 3) in vec4 FragPosLightSpace;

layout(location = 0) out vec4 FragColor;

uniform sampler2D diffuseTexture;
uniform sampler2DShadow shadowMap;

uniform vec3 lightDir;
uniform vec3 lightColor;
uniform vec3 ambientColor;

uniform bool  debugShadow;
uniform float shadowBias    = 0.001; // bigger bias
uniform float pcfDiskRadius = 0.5;   // smaller radius

// Simple 3x3 sample offsets around projected coords
vec2 poissonDisk[9] = vec2[](
vec2(-0.5,  0.5), vec2( 0.5,  0.5), vec2( 0.5, -0.5),
vec2(-0.5, -0.5), vec2( 0.0,  0.0), vec2( 0.5,  0.0),
vec2( 0.0,  0.5), vec2(-0.5,  0.0), vec2( 0.0, -0.5)
);

float PCFShadow(vec4 fragPosLS, vec3 norm)
{
    vec3 projCoords = fragPosLS.xyz / fragPosLS.w;
    projCoords      = projCoords * 0.5 + 0.5;
    if(projCoords.z > 1.0 || projCoords.x < 0.0 || projCoords.x > 1.0 ||
    projCoords.y < 0.0 || projCoords.y > 1.0)
    return 1.0;

    float b      = shadowBias * (1.0 - max(dot(norm, normalize(lightDir)), 0.0));
    float shadow = 0.0;
    for(int i = 0; i < 9; ++i)
    {
        vec2 offset = poissonDisk[i] * pcfDiskRadius / 2048.0;
        shadow     += texture(shadowMap, vec3(projCoords.xy + offset, projCoords.z - b));
    }
    return shadow / 9.0;
}
void main()
{
    vec3 norm       = normalize(Normal);
    float shadow    = PCFShadow(FragPosLightSpace, norm);

    // Debug shadow pass
    if(debugShadow)
    {
        FragColor = vec4(vec3(shadow), 1.0);
        return;
    }

    float diff       = max(dot(norm, normalize(lightDir)), 0.0);
    vec3 diffuse     = diff * lightColor;
    vec3 lighting    = ambientColor + diffuse * shadow;
    vec4 texColor    = texture(diffuseTexture, TexCoord);
    FragColor        = vec4(texColor.rgb * lighting, texColor.a);
}
