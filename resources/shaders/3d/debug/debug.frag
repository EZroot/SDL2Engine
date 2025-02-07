#version 330 core
in vec2 TexCoord;
out vec4 FragColor;
uniform sampler2D debugTexture;
uniform bool showRawDepth; // true: show stored depth; false: show linearized depth
uniform float nearPlane;   // light's near plane
uniform float farPlane;    // light's far plane

float LinearizeDepth(float depth)
{
    float z = depth * 2.0 - 1.0; // back to NDC
    return (2.0 * nearPlane * farPlane) / (farPlane + nearPlane - z * (farPlane - nearPlane));
}

void main() {
    float depth = texture(debugTexture, TexCoord).r;
    if(showRawDepth)
    FragColor = vec4(vec3(depth), 1.0);
    else {
        float linearDepth = LinearizeDepth(depth) / farPlane; // normalized linear depth (0..1)
        FragColor = vec4(vec3(linearDepth), 1.0);
    }
}
