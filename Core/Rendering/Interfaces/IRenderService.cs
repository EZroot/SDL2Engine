using SDL2;

namespace SDL2Engine.Core.Rendering.Interfaces
{
    public interface IRenderService
    {
        nint RenderPtr { get; }
        OpenGLHandle OpenGLHandleGui { get; }
        OpenGLHandle OpenGLHandle2D { get; }

        nint CreateRenderer(nint window, SDL.SDL_RendererFlags renderFlags =
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
    }
}
