#version 330 core
in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
in vec4 FragPosLightSpace;

out vec4 FragColor;

uniform sampler2D diffuseTexture;
uniform sampler2D shadowMap; // your depth texture

uniform vec3 lightDir;
uniform vec3 lightColor;
uniform vec3 ambientColor;

float ShadowCalculation(vec4 fragPosLightSpace)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // transform to [0, 1]
    projCoords = projCoords * 0.5 + 0.5;

    // if outside the shadow map, no shadow
    if(projCoords.z > 1.0)
    return 0.0;

    // fetch depth from shadow map
    float closestDepth = texture(shadowMap, projCoords.xy).r;
    float currentDepth = projCoords.z;

    // bias to prevent acne (tweak as needed)
    float bias = max(0.005 * (1.0 - dot(normalize(Normal), -lightDir)), 0.005);

    // shadow factor: 1.0 means in shadow
    float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;
    return shadow;
}

void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightDirection = normalize(lightDir);
    float diff = max(dot(norm, -lightDirection), 0.0);
    vec3 diffuse = diff * lightColor;

    float shadow = ShadowCalculation(FragPosLightSpace);
    vec3 lighting = ambientColor + (1.0 - shadow) * diffuse;

    vec4 color = texture(diffuseTexture, TexCoord);
    FragColor = vec4(color.rgb * lighting, color.a);
}
