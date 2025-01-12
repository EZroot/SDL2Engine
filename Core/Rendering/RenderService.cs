using System.Runtime.InteropServices;
using System.Text;
using SDL2;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Rendering
{
    internal class RenderService : IServiceRenderService
    {
        private readonly IServiceSysInfo m_sysInfo;
        private readonly IServiceWindowConfig m_windowConfig; // Replace with a renderer config if I need to

        private IntPtr m_render;
        public RenderService(IServiceSysInfo sysInfo, IServiceWindowConfig windowConfig)
        {
            m_windowConfig = windowConfig ?? throw new ArgumentNullException(nameof(windowConfig));
            m_sysInfo = sysInfo ?? throw new ArgumentNullException(nameof(sysInfo));
        }

        public IntPtr CreateRenderer(IntPtr window, SDL.SDL_RendererFlags renderFlags = 
        SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED 
        | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC)
        {
            SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_DRIVER, "opengl"); 
            LogAvailableRenderDrivers();
            m_render = SDL.SDL_CreateRenderer(window, -1, renderFlags);
            if (m_render == IntPtr.Zero)
            {
                Debug.LogError("Renderer creation failed! SDL_Error: " + SDL.SDL_GetError());
                SDL.SDL_DestroyWindow(window);
                SDL.SDL_Quit();
                throw new InvalidOperationException("Renderer creation failed! SDL_Error: " + SDL.SDL_GetError());
            }
            PrintRenderDriver(m_render);
            return m_render;
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

        public IntPtr GetRenderer()
        {
            return m_render;
        }

        private void PrintRenderDriver(IntPtr renderer)
        {
            SDL.SDL_RendererInfo rendererInfo;
            if (SDL.SDL_GetRendererInfo(renderer, out rendererInfo) == 0)
            {
                var driver = Marshal.PtrToStringAnsi(rendererInfo.name).ToUpper();
                m_sysInfo.SetInfoCurrentDriver(driver);
                Debug.Log($"SDL Driver Set - <color=green>[{driver}]</color>]");
            }
            else
            {
                Debug.LogError("Failed to get renderer information: " + SDL.SDL_GetError());
            }
        }

        private void LogAvailableRenderDrivers()
        {
            var numDrivers = SDL.SDL_GetNumRenderDrivers();
            if (numDrivers < 1)
            {
                Debug.LogError("No SDL rendering drivers available!");
                return;
            }

            var availableDrivers = new string[numDrivers];
            var driverText = new StringBuilder();
            driverText.Append($"SDL Drivers ({numDrivers})");
            for (int i = 0; i < numDrivers; i++)
            {
                SDL.SDL_RendererInfo rendererInfo;
                if (SDL.SDL_GetRenderDriverInfo(i, out rendererInfo) == 0)
                {
                    var driverName =  Marshal.PtrToStringAnsi(rendererInfo.name).ToUpper();
                    availableDrivers[i] = driverName;
                    driverText.Append($"    <color=yellow>{i}# [{driverName}]</color>");
                }
                else
                {
                    Debug.LogError($"Failed to get renderer info for driver index {i}: {SDL.SDL_GetError()}");
                }
            }
            
            m_sysInfo.SetInfoCurrentAvailableDrivers(availableDrivers);
            Debug.Log($"{driverText}");

        }

    }
}
