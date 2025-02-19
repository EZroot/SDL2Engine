#version 450 core

// Only needed if you want alpha testing
in vec2 vTexCoord;
uniform sampler2D alphaMask;   // Use your actual texture sampler here
uniform float alphaCutoff = 0.5;

void main()
{
    // We don't output color in a depth-only pass. gl_FragDepth is written automatically.
}
