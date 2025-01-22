#version 400 core

in vec2 fragTexCoord;
out vec4 fragColor;   

uniform sampler2D Texture; 

void main()
{
    fragColor = texture(Texture, fragTexCoord);
}
