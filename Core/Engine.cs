using SDL2;
using SDL2Engine.Core.Windowing.Interfaces;
using Debug = SDL2Engine.Core.Utils.Debug;
using ImGuiNET;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Events;
using SDL2Engine.Core.CoreSystem.Configuration.Components;
using SDL2Engine.Core.GuiRenderer.Helpers;
using System.Drawing;
using System.Numerics;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Rendering;
using static SDL2Engine.Core.Addressables.AssetManager;

namespace SDL2Engine.Core
{
    internal class Engine : IDisposable
    {
        private const string RESOURCES_FOLDER = "/home/anon/Repos/SDL2Engine/resources";

        public enum ExampleEnum { OptionA, OptionB, OptionC };
        
        private readonly IServiceWindowService m_windowService;
        private readonly IServiceRenderService m_renderService;
        private readonly IServiceGuiRenderService m_guiRenderService;
        private readonly IServiceGuiWindowService m_guiWindowBuilder;
        private readonly IVariableBinder m_guiVariableBinder;
        private readonly IServiceAssetManager m_assetManager;
        private readonly IServiceAudioLoader m_audioLoader;
        private readonly IServiceCameraService m_cameraService;
        private readonly IGame m_game;

        private IntPtr m_window, m_renderer;
        private int m_windowWidth, m_windowHeight;
        private int m_camera;

        
        private bool disposed = false;

        private bool TEST_window_isopen = true;

        public Engine
        (
            IServiceWindowService windowService,
            IServiceRenderService renderService,
            IServiceGuiRenderService guiRenderService,
            IServiceAssetManager assetManager,
            IServiceGuiWindowService guiWindowBuilder,
            IVariableBinder guiVariableBinder,
            IServiceAudioLoader audioLoader,
            IServiceCameraService cameraService,
            IGame game
        )
        {
            m_windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            m_renderService = renderService ?? throw new ArgumentNullException(nameof(renderService));
            m_guiRenderService = guiRenderService ?? throw new ArgumentNullException(nameof(guiRenderService));
            m_guiWindowBuilder = guiWindowBuilder ?? throw new ArgumentNullException(nameof(guiWindowBuilder));
            m_assetManager = assetManager ?? throw new ArgumentNullException(nameof(assetManager));
            m_guiVariableBinder = guiVariableBinder ?? throw new ArgumentNullException(nameof(guiVariableBinder));
            m_audioLoader = audioLoader ?? throw new ArgumentNullException(nameof(audioLoader));
            m_cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
            m_game = game ?? throw new ArgumentNullException(nameof(game));
        }

        public void Run()
        {
            Initialize();
            m_game.Initialize();

            bool running = true;
            while (running)
            {
                /*
                 * var currentTime = SDL.SDL_GetTicks();
                   var deltaTime = (currentTime - lastTime) / 1000f;
                   lastTime = currentTime;
                 */
                
                Time.Update();
                
                while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
                {
                    InputManager.Update(e);
                    Debug.LogPollEvents(e);
                    HandleWindowEvents(e, ref running);
                }
                
                SDL.SDL_SetRenderDrawColor(m_renderer, 25, 25, 45, 255);
                SDL.SDL_RenderClear(m_renderer);

                var camera = m_cameraService.GetCamera(m_camera);
                HandleCameraInput(camera);
                
                m_game.Update(Time.DeltaTime);
                m_game.Render();

                ImGui.NewFrame();

                m_guiRenderService.RenderFullScreenDockSpace();

                RenderGUI();


                SDL.SDL_RenderPresent(m_renderer);
            }


            Dispose();
        }

        private void Initialize()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Debug.LogError("SDL could not initialize! SDL_Error: " + SDL.SDL_GetError());
                return;
            }
            
            m_window = m_windowService.CreateWindowSDL();
            m_windowService.SetWindowIcon(m_window, RESOURCES_FOLDER+"/ashh.png");
            m_renderer = m_renderService.CreateRenderer(m_window);

            IntPtr imguiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(imguiContext);

            SDL.SDL_GetWindowSize(m_window, out var windowWidth, out var windowHeight);
            m_windowWidth = windowWidth;
            m_windowHeight = windowHeight;
            
            m_guiRenderService.CreateGuiRender(m_window, m_renderer, windowWidth, windowHeight);
            m_guiRenderService.SetupIO(windowWidth, windowHeight);
            
            m_camera = m_cameraService.CreateCamera(Vector2.One);
            m_cameraService.SetActiveCamera(m_camera);
        }
        
        private void RenderGUI()
        {
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
        }

        private void HandleWindowEvents(SDL.SDL_Event e, ref bool isRunning)
        {
            if (e.type == SDL.SDL_EventType.SDL_QUIT ||
                      (e.type == SDL.SDL_EventType.SDL_KEYDOWN && e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE))
            {
                isRunning = false;
            }

            if (e.type == SDL.SDL_EventType.SDL_WINDOWEVENT && e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
            {
                string title = SDL.SDL_GetWindowTitle(m_window);
                uint flags = SDL.SDL_GetWindowFlags(m_window);
                bool isFullscreen = (flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) != 0 ||
                                    (flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) != 0;

                m_windowWidth = e.window.data1;
                m_windowHeight = e.window.data2;
                m_guiRenderService.OnWindowResize(m_windowWidth, m_windowHeight);

                var windowSettings = new WindowSettings(title, m_windowWidth, m_windowHeight, isFullscreen);
                EventHub.Raise(this, new OnWindowResized(windowSettings));
            }
        }
        
        /// <summary>
        /// Handles user input for camera movement and zoom.
        /// </summary>
        private void HandleCameraInput(ICamera camera)
        {
            float cameraSpeed = 50f * Time.DeltaTime;
            Vector2 movement = Vector2.Zero;
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_UP))
                movement.Y -= cameraSpeed;
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_DOWN))
                movement.Y += cameraSpeed;
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_LEFT))
                movement.X -= cameraSpeed;
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_RIGHT))
                movement.X += cameraSpeed;
            
            camera.Move(movement);
            
            // Zoom controls
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_EQUALS))
                camera.SetZoom(camera.Zoom + 0.1f);
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_MINUS))
                camera.SetZoom(camera.Zoom - 0.1f);
        }
        
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            m_assetManager.Cleanup();
            m_cameraService.Cleanup();
            m_guiRenderService.Dispose();
            
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
