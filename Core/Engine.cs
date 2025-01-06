using SDL2;
using SDL2Engine.Core.Rendering;
using SDL2Engine.Core.Windowing.Interfaces;
using Debug = SDL2Engine.Core.Utils.Debug;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Rendering.GLBindingsContext;

namespace SDL2Engine.Core
{
    internal class Engine : IDisposable
    {
        private readonly IServiceWindowService m_windowService;
        private readonly IServiceRenderService m_renderService;

        private IntPtr m_window, m_renderer, m_glContext;
        private ImGuiRenderer _imguiRenderer;  // ImGui renderer

        public Engine
        (
            IServiceWindowService? windowService,
            IServiceRenderService? renderService
        )
        {
            m_windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            m_renderService = renderService ?? throw new ArgumentNullException(nameof(renderService));
        }

        public void Run()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Debug.LogError("SDL could not initialize! SDL_Error: " + SDL.SDL_GetError());
                return;
            }

            m_window = m_windowService.CreateWindowOpenGL();
            m_renderer = m_renderService.CreateRenderer(m_window);
            m_glContext = m_renderService.CreateOpenGLContext(m_window);

            m_renderService.GLMakeCurrent(m_window, m_glContext);

            GL.LoadBindings(new SDL2BindingsContext());
                // 🔧 Create and set the ImGui context
    // Create and set the ImGui context
    IntPtr imguiContext = ImGui.CreateContext();
    ImGui.SetCurrentContext(imguiContext);

    // Initialize ImGui Renderer
    SDL.SDL_GetWindowSize(m_window, out int windowWidth, out int windowHeight);
    _imguiRenderer = new ImGuiRenderer(m_window, windowWidth, windowHeight);

            bool running = true;
            while (running)
            {
                while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
                {
                    if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    {
                        running = false;
                    }
                    else if (e.type == SDL.SDL_EventType.SDL_KEYDOWN &&
                             e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE)
                    {
                        running = false;
                    }
                    
                    // Process ImGui input
                    _imguiRenderer.ProcessEvent(e);
                }

                SDL.SDL_SetRenderDrawColor(m_renderer, 0, 0, 255, 255);
                SDL.SDL_RenderClear(m_renderer);

                // Start the ImGui frame
                _imguiRenderer.NewFrame();

                // Your ImGui drawing code here (e.g., ImGui.ShowDemoWindow())

                // Render ImGui
                ImGui.Render();
                _imguiRenderer.RenderDrawData(ImGui.GetDrawData());

                SDL.SDL_RenderPresent(m_renderer);
            }

            SDL.SDL_DestroyRenderer(m_renderer);
            SDL.SDL_DestroyWindow(m_window);
            SDL.SDL_Quit();
        }

        public void Dispose()
        {
            if (m_renderer != IntPtr.Zero)
            {
                SDL.SDL_DestroyRenderer(m_renderer);
            }

            if (m_window != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(m_window);
            }

            _imguiRenderer?.Dispose();  // Ensure ImGui resources are released

            SDL.SDL_Quit();
        }

        ~Engine()
        {
            Debug.Log("Disposing engine...");
            Dispose();
        }
    }
}
