#version 330 core
in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;

out vec4 FragColor;

uniform sampler2D Texture;

// Lighting uniforms - set these from your code.
uniform vec3 lightDir;      // Directional light direction (should be normalized)
uniform vec3 lightColor;
uniform vec3 ambientColor;

void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightDirection = normalize(lightDir);
    // Basic diffuse lighting
    float diff = max(dot(norm, -lightDirection), 0.0);
    vec3 diffuse = diff * lightColor;
    vec3 lighting = ambientColor + diffuse;

    vec4 texColor = texture(Texture, TexCoord);
    FragColor = vec4(texColor.rgb * lighting, texColor.a);
}
