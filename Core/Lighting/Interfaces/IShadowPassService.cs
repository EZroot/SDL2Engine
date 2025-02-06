using OpenTK.Mathematics;

namespace SDL2Engine.Core.Lighting.Interfaces;

public interface IShadowPassService
{
    int DepthTexturePtr { get; }
    void Initialize(int shadowWidth = 2048, int shadowHeight = 2048);
    void RegisterMesh(OpenGLHandle asset);
    void RenderShadowPass(OpenGLHandle asset, Matrix4 model, Matrix4 lightView, Matrix4 lightProjection);
}