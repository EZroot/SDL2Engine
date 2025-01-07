using SDL2;
using SDL2Engine.Core.Windowing.Interfaces;
using Debug = SDL2Engine.Core.Utils.Debug;
using ImGuiNET;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Events;
using SDL2Engine.Core.Configuration.Components;

namespace SDL2Engine.Core
{
    internal class Engine : IDisposable
    {
        private readonly IServiceWindowService m_windowService;
        private readonly IServiceRenderService m_renderService;
        private readonly IServiceGuiRenderService m_guiRenderService;
        private readonly IServiceGuiWindowService m_guiWindowBuilder;
        private readonly IVariableBinder m_guiVariableBinder;
        private readonly IServiceImageLoader m_imageLoader;

        private IntPtr m_window, m_renderer;

        private bool disposed = false;

        public Engine
        (
            IServiceWindowService? windowService,
            IServiceRenderService? renderService,
            IServiceGuiRenderService? guiRenderService,
            IServiceImageLoader? imageLoader,
            IServiceGuiWindowService? guiWindowBuilder,
            IVariableBinder? guiVariableBinder
        )
        {
            m_windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            m_renderService = renderService ?? throw new ArgumentNullException(nameof(renderService));
            m_guiRenderService = guiRenderService ?? throw new ArgumentNullException(nameof(guiRenderService));
            m_guiWindowBuilder = guiWindowBuilder ?? throw new ArgumentNullException(nameof(guiWindowBuilder));
            m_imageLoader = imageLoader ?? throw new ArgumentNullException(nameof(imageLoader));
            m_guiVariableBinder = guiVariableBinder ?? throw new ArgumentNullException(nameof(guiVariableBinder));
        }

        public unsafe void Run()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Debug.LogError("SDL could not initialize! SDL_Error: " + SDL.SDL_GetError());
                return;
            }

            m_imageLoader.Initialize();
            m_window = m_windowService.CreateWindowSDL();
            m_windowService.SetWindowIcon(m_window, "resources/ashh.png");

            m_renderer = m_renderService.CreateRenderer(m_window);

            IntPtr imguiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(imguiContext);

            SDL.SDL_GetWindowSize(m_window, out var windowWidth, out var windowHeight);

            m_guiRenderService.CreateGuiRender(m_window, m_renderer, windowWidth, windowHeight);
            m_guiRenderService.SetupIO(windowWidth,windowHeight);


            // testing variable bindings
            int someInteger = 42;
            float someFloat = 3.14f;
            string someString = "Initial Text";
            bool someBool = true;

            // bind variables
            m_guiVariableBinder.BindVariable("Integer", someInteger);
            m_guiVariableBinder.BindVariable("Float", someFloat);
            m_guiVariableBinder.BindVariable("String", someString);
            m_guiVariableBinder.BindVariable("Bool", someBool);


            bool running = true;
            while (running)
            {
                while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
                {
                    Debug.LogPollEvents(e);
                    if (e.type == SDL.SDL_EventType.SDL_QUIT ||
                        (e.type == SDL.SDL_EventType.SDL_KEYDOWN && e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE))
                    {
                        running = false;
                        break;
                    }
                    if (e.type == SDL.SDL_EventType.SDL_WINDOWEVENT && e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                    {
                        string title = SDL.SDL_GetWindowTitle(m_window);
                        uint flags = SDL.SDL_GetWindowFlags(m_window);
                        bool isFullscreen = (flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) != 0 ||
                                            (flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) != 0;

                        int newWidth = e.window.data1;
                        int newHeight = e.window.data2;
                        m_guiRenderService.OnWindowResize(newWidth, newHeight);

                        var windowSettings = new WindowSettings(title, newWidth, newHeight, isFullscreen);
                        EventHub.Raise(this, new OnWindowResized(windowSettings));
                    }
                    m_guiRenderService.ProcessEvent(e);
                }

                SDL.SDL_SetRenderDrawColor(m_renderer, 80, 80, 80, 255);
                SDL.SDL_RenderClear(m_renderer);

                ImGui.NewFrame();

                m_guiWindowBuilder.BeginWindow("Window");

                m_guiWindowBuilder.Draw("Integer");
                m_guiWindowBuilder.Draw("Float");
                m_guiWindowBuilder.Draw("String");
                m_guiWindowBuilder.Draw("Bool");

                m_guiWindowBuilder.EndWindow();

                ImGui.ShowDemoWindow();

                ImGui.Render();

                var drawData = ImGui.GetDrawData();

                if (drawData.CmdListsCount > 0)
                {
                    m_guiRenderService.RenderDrawData(drawData);
                }

                if ((ImGui.GetIO().ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
                {
                    ImGui.UpdatePlatformWindows();
                    ImGui.RenderPlatformWindowsDefault();
                }

                SDL.SDL_RenderPresent(m_renderer);
            }

            Dispose();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            m_guiRenderService?.Dispose();

            if (m_renderer != IntPtr.Zero)
            {
                SDL.SDL_DestroyRenderer(m_renderer);
            }

            if (m_window != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(m_window);
            }

            SDL.SDL_Quit();
        }

        ~Engine()
        {
            Debug.Log("Disposing engine...");
            Dispose();
        }
    }
}
