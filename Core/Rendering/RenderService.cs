using SDL2;
using SDL2Engine.Core.Configuration;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Rendering
{
    internal class RenderService : IServiceRenderService
    {
        private readonly IServiceWindowConfig m_windowConfig; // replace with a renderer config
        public RenderService(IServiceWindowConfig? windowConfig)
        {
            m_windowConfig = windowConfig ?? throw new ArgumentNullException(nameof(windowConfig));
        }

        public IntPtr CreateRenderer(IntPtr window)
        {
            IntPtr renderer = SDL.SDL_CreateRenderer(
                window,
                -1,
                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC
            );

            if (renderer == IntPtr.Zero)
            {
                Debug.LogError("Renderer creation failed! SDL_Error: " + SDL.SDL_GetError());
                SDL.SDL_DestroyWindow(window);
                SDL.SDL_Quit();
                throw new InvalidOperationException("Renderer creation failed! SDL_Error: " + SDL.SDL_GetError());
            }
            return renderer;
        }

        public nint CreateOpenGLContext(nint window)
        {
            IntPtr glContext = SDL.SDL_GL_CreateContext(window);
            if (glContext == IntPtr.Zero)
            {
                Debug.LogError("OpenGL context creation failed! SDL_Error: " + SDL.SDL_GetError());
                SDL.SDL_DestroyWindow(window);
                SDL.SDL_Quit();
                throw new InvalidOperationException("OpenGL context creation failed! SDL_Error: " + SDL.SDL_GetError());
            }
            return glContext;
        }

        public void GLMakeCurrent(IntPtr window, IntPtr glContext)
        {
            if (SDL.SDL_GL_MakeCurrent(window, glContext) != 0)
            {
                Debug.LogError("Failed to make OpenGL context current! SDL_Error: " + SDL.SDL_GetError());
                throw new InvalidOperationException("Failed to make OpenGL context current! SDL_Error: " + SDL.SDL_GetError());
            }
        }

    }
}
