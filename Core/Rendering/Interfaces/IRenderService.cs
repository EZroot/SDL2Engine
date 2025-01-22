using SDL2;

namespace SDL2Engine.Core.Rendering.Interfaces
{
    public interface IRenderService
    {
        nint RenderPtr { get; }
        OpenGLHandle OpenGLHandleGui { get; }
        OpenGLHandle OpenGLHandle2D { get; }
        IntPtr CreateRendererSDL(IntPtr window, SDL.SDL_RendererFlags renderFlags = SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
        nint CreateOpenGLContext(nint window);
        OpenGLHandle CreateOpenGLDeviceObjects(string vertShaderSrc, string fragShaderSrc);
        OpenGLHandle Create2DImageOpenGLDeviceObjects(string vertShaderSrc, string fragShaderSrc);
    }
}
