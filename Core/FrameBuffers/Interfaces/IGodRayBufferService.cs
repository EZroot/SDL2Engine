using SDL2Engine.Core.Cameras;
using SDL2Engine.Core.Lighting;

namespace SDL2Engine.Core.Buffers.Interfaces;

public interface IGodRayBufferService
{
    void BindFramebuffer();
    void UnbindFramebuffer();
    void ProcessGodRays(CameraGL3D cam, Light light, int depthTexture);
    int GetTexture();
    void RenderDebug();
}