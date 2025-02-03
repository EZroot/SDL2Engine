#version 400 core

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;

// Instanced model matrix (4 vec4 attributes)
layout(location = 2) in mat4 instanceMatrix;

uniform mat4 projViewMatrix;

out vec2 TexCoord;

void main()
{
    gl_Position = projViewMatrix * instanceMatrix * vec4(aPosition, 0.0, 1.0);
    TexCoord = aTexCoord;
}
