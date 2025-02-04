#version 450 core

layout (location = 0) in vec3 aPos;

// Only needed if you want to discard fragments based on alpha:
layout (location = 2) in vec2 aTexCoord;
out vec2 vTexCoord;

uniform mat4 model;
uniform mat4 lightView;
uniform mat4 lightProjection;

void main()
{
    // Pass UV to fragment (only if using alpha test)
    vTexCoord = aTexCoord;

    gl_Position = lightProjection * lightView * model * vec4(aPos, 1.0);
}
