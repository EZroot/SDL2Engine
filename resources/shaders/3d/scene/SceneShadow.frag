#version 330 core
in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
in vec4 FragPosLightSpace;

out vec4 FragColor;

uniform sampler2D diffuseTexture;
uniform sampler2DShadow shadowMap; // use sampler2DShadow for hardware PCF

uniform vec3 lightDir;
uniform vec3 lightColor;
uniform vec3 ambientColor;
uniform bool debugShadow;

float ShadowCalculation(vec4 fragPosLightSpace)
{
    // Perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // Transform to [0, 1] range
    projCoords = projCoords * 0.5 + 0.5;
    // Invert Y coordinate to match our flipped light space matrix
    projCoords.y = 1.0 - projCoords.y;

    // Calculate bias to prevent shadow acne
    float bias = max(0.005 * (1.0 - dot(normalize(Normal), -normalize(lightDir))), 0.005);

    // Hardware shadow mapping: the third coordinate is the reference depth
    return texture(shadowMap, vec3(projCoords.xy, projCoords.z - bias));
}

void main()
{
    vec3 norm = normalize(Normal);
    float shadow = ShadowCalculation(FragPosLightSpace);

    if(debugShadow)
    {
        FragColor = vec4(vec3(shadow), 1.0);
        return;
    }

    float diff = max(dot(norm, -normalize(lightDir)), 0.0);
    vec3 diffuse = diff * lightColor;
    vec3 lighting = ambientColor + diffuse * shadow;
    vec4 texColor = texture(diffuseTexture, TexCoord);
    FragColor = vec4(texColor.rgb * lighting, texColor.a);
}
