using SDL2;

namespace SDL2Engine.Core.Rendering.Interfaces
{
    public interface IRenderService
    {
        public nint RenderPtr { get; }
        IntPtr CreateRenderer(IntPtr window, SDL.SDL_RendererFlags renderFlags =
        SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED
        | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
        void GLMakeCurrent(IntPtr window, IntPtr glContext);
    }
}
