using SDL2;
using SDL2Engine.Core.Windowing.Interfaces;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.Utils;
using SDL2Engine.Core.Addressables;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Events;

namespace SDL2Engine.Core.Windowing
{
    internal class WindowService : IServiceWindowService
    {
        private readonly IServiceWindowConfig m_windowConfig;
        private readonly IServiceImageLoader m_imageLoader;

        private nint m_window;

        public nint Window => m_window;
        
        public WindowService(
            IServiceWindowConfig windowConfig,
            IServiceImageLoader imageLoader
            )
        {
            m_windowConfig = windowConfig ?? throw new ArgumentNullException(nameof(windowConfig));
            m_imageLoader = imageLoader ?? throw new ArgumentNullException(nameof(imageLoader));
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
            m_window = SDL.SDL_CreateWindow(
                m_windowConfig.Settings.WindowName,
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                m_windowConfig.Settings.Width,
                m_windowConfig.Settings.Height,
                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI
            );

            if (m_window == IntPtr.Zero)
            {
                Debug.LogError("Window creation failed! SDL_Error: " + SDL.SDL_GetError());
                SDL.SDL_Quit();
                throw new InvalidOperationException("Renderer creation failed! SDL_Error: " + SDL.SDL_GetError());
            }

            SubscribeToEvents();
            return m_window;
        }

        public void SetWindowIcon(IntPtr window, string iconPath)
        {
            if (window == IntPtr.Zero)
            {
                throw new ArgumentException("Invalid window handle.");
            }

            IntPtr iconSurface = m_imageLoader.LoadImage(iconPath);
            SDL.SDL_SetWindowIcon(window, iconSurface);
            Debug.Log($"Window Icon Set: {iconPath}");
            SDL.SDL_FreeSurface(iconSurface); 
        }

        private void SubscribeToEvents()
        {
            EventHub.Subscribe<OnWindowResized>(OnWindowResizedEvent);
        }

        private void UnsubscribeToEvents()
        {
            EventHub.Unsubscribe<OnWindowResized>(OnWindowResizedEvent);
        }
        
        private void OnWindowResizedEvent(object sender, OnWindowResized e)
        {
            m_windowConfig.Save(e.WindowSettings);
        }

        public void Dispose()
        {
            UnsubscribeToEvents();
        }

        ~WindowService()
        {
            Dispose();
        }
    }
}
