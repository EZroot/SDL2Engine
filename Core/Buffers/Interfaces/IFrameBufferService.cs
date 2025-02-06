namespace SDL2Engine.Core.Buffers.Interfaces;

public interface IFrameBufferService
{
    void BindFramebuffer(int screenWidth, int screenHeight);
    public void UnbindFramebuffer();
    public void RenderFramebuffer();
}