using SDL2;
using SDL2Engine.Core.Windowing.Interfaces;
using SDL2Engine.Core.Configuration;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Windowing
{
    internal class WindowService : IServiceWindowService
    {
        private readonly IServiceWindowConfig m_windowConfig;

        public WindowService(IServiceWindowConfig? windowConfig)
        {
            m_windowConfig = windowConfig  ?? throw new ArgumentNullException(nameof(windowConfig));
        }

        /// <summary>
        /// Create a window using OpenGL Renderer.
        /// Requires manual opengl management.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IntPtr CreateWindowOpenGL()
        {
            SDL.SDL_WindowFlags windowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL;

            IntPtr window = SDL.SDL_CreateWindow(
                m_windowConfig.Settings.WindowName,
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                m_windowConfig.Settings.Width,
                m_windowConfig.Settings.Height,
                windowFlags
            );

            if (window == IntPtr.Zero)
            {
                Debug.LogError("Window creation failed! SDL_Error: " + SDL.SDL_GetError());
                SDL.SDL_Quit();
                throw new InvalidOperationException("Renderer creation failed! SDL_Error: " + SDL.SDL_GetError());
            }
            
            return window;
        }

        /// <summary>
        /// Create a window using SDL Default Renderer.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IntPtr CreateWindowSDL()
        {
            IntPtr window = SDL.SDL_CreateWindow(
                m_windowConfig.Settings.WindowName,
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                m_windowConfig.Settings.Width,
                m_windowConfig.Settings.Height,
                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI
            );

            if (window == IntPtr.Zero)
            {
                Debug.LogError("Window creation failed! SDL_Error: " + SDL.SDL_GetError());
                SDL.SDL_Quit();
                throw new InvalidOperationException("Renderer creation failed! SDL_Error: " + SDL.SDL_GetError());
            }
            return window;
        }
    }
}
