#version 450 core
in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D godrayTex;
uniform sampler2D depthTex;

uniform mat4 view;
uniform mat4 projection;
uniform mat4 invProjection;
uniform mat4 invView;
uniform vec3 cameraPos;
uniform vec3 lightPos;
uniform vec3 lightColor;

uniform int numSamples;
uniform float density;
uniform float decay;
uniform float weight;
uniform float exposure;

vec3 ReconstructWorldPosition(vec2 uv, float depth)
{
    float z = depth * 2.0 - 1.0;
    vec4 clipPos = vec4(uv * 2.0 - 1.0, z, 1.0);
    vec4 viewPos = invProjection * clipPos;
    viewPos /= viewPos.w;
    vec4 worldPos = invView * viewPos;
    return worldPos.xyz;
}

void main()
{
    float depth = texture(depthTex, TexCoord).r;
    vec3 worldPos = ReconstructWorldPosition(TexCoord, depth);
    // March away from the light instead of toward it.
    vec3 dirFromLight = normalize(worldPos - lightPos);

    vec3 samplePos = worldPos;
    vec4 accumulated = vec4(0.0);
    float illuminationDecay = 1.0;

    for (int i = 0; i < numSamples; i++)
    {
        samplePos += dirFromLight * density;
        vec4 clipPos = projection * view * vec4(samplePos, 1.0);
        clipPos /= clipPos.w;
        vec2 sampleUV = clipPos.xy * 0.5 + 0.5;
        if (sampleUV.x < 0.0 || sampleUV.x > 1.0 ||
        sampleUV.y < 0.0 || sampleUV.y > 1.0)
        break;
        float intensity = texture(godrayTex, sampleUV).r;
        vec4 sampleColor = vec4(1.0, 0.85, 0.6, 0.0) * intensity;

        accumulated += sampleColor * illuminationDecay * weight;
        illuminationDecay *= decay;
    }

    FragColor = vec4(accumulated.rgb * exposure, 1.0);
}
