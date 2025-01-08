using SDL2;

namespace SDL2Engine.Core.Rendering.Interfaces
{
    internal interface IServiceRenderService
    {
        IntPtr CreateRenderer(IntPtr window, SDL.SDL_RendererFlags renderFlags =
        SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED
        | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
        IntPtr CreateOpenGLContext(IntPtr window);
        void GLMakeCurrent(IntPtr window, IntPtr glContext);
    }
}
