namespace SDL2Engine.Core.Buffers.Interfaces;

public interface IFrameBufferService
{
    public bool IsInitialized { get; }
    void Initialize();
    void BindFramebuffer();
    public void UnbindFramebuffer();
    public void RenderFramebuffer();
    int GetTexture();
    int GetDepthTexture();
    void Resize(int newWidth, int newHeight);
}