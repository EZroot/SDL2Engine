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
using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Physics.Interfaces;    

namespace SDL2Engine.Core
{
    internal class Engine : IDisposable
    {
        private const string RESOURCES_FOLDER = "/home/anon/Repos/SDL_Engine/SDL2Engine/resources";

        public enum ExampleEnum { OptionA, OptionB, OptionC };
        
        private readonly IServiceWindowService m_windowService;
        private readonly IRenderService m_renderService;
        private readonly IGuiRenderService m_guiRenderService;
        private readonly IGuiWindowBuilder m_guiWindowBuilder;
        private readonly IVariableBinder m_guiVariableBinder;
        private readonly IAudioService m_audioService;
        private readonly IImageService m_imageService;
        private readonly ICameraService m_cameraService;
        private readonly IPhysicsService m_physicsService;
        private readonly IServiceProvider m_serviceProvider;
        
        private IntPtr m_window, m_renderer;
        private int m_windowWidth, m_windowHeight;
        private int m_camera;

        private bool disposed = false;
        private bool TEST_window_isopen = true;

        public Engine(IServiceProvider serviceProvider)
        {
            m_serviceProvider = serviceProvider;
            m_windowService = m_serviceProvider.GetService<IServiceWindowService>();
            m_renderService = m_serviceProvider.GetService<IRenderService>();
            m_guiRenderService = m_serviceProvider.GetService<IGuiRenderService>();
            m_guiWindowBuilder = m_serviceProvider.GetService<IGuiWindowBuilder>();
            m_audioService = m_serviceProvider.GetService<IAudioService>();
            m_imageService = m_serviceProvider.GetService<IImageService>();
            m_guiVariableBinder = m_serviceProvider.GetService<IVariableBinder>();
            m_cameraService = m_serviceProvider.GetService<ICameraService>();
            m_physicsService = m_serviceProvider.GetService<IPhysicsService>();
        }

        private void Initialize()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Debug.LogError("SDL could not initialize! SDL_Error: " + SDL.SDL_GetError());
                return;
            }
            
            m_window = m_windowService.CreateWindowSDL();
            m_windowService.SetWindowIcon(m_window, RESOURCES_FOLDER + "/ashh.png");
            m_renderer = m_renderService.CreateRenderer(m_window, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED 
                                                                  | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            IntPtr imguiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(imguiContext);

            SDL.SDL_GetWindowSize(m_window, out var windowWidth, out var windowHeight);
            m_windowWidth = windowWidth;
            m_windowHeight = windowHeight;
            
            m_guiRenderService.CreateGuiRender(m_window, m_renderer, windowWidth, windowHeight);
            m_guiRenderService.SetupIO(windowWidth, windowHeight);
            
            m_camera = m_cameraService.CreateCamera(Vector2.One);
            m_cameraService.SetActiveCamera(m_camera);
            
            m_physicsService.Initialize(9.81f);
            m_physicsService.CreateWindowBoundaries(windowWidth, windowHeight);
        }

        public void Run(IGame game)
        {
            Initialize();
            game.Initialize(m_serviceProvider);

            var accumulator = 0f;
            var fixedStep = 0.02f;
            bool running = true;
            while (running)
            {
                Time.Update();
                accumulator += Time.RawDeltaTime;
                
                while (accumulator >= fixedStep)
                {
                    m_physicsService.UpdatePhysics(fixedStep); 
                    accumulator -= fixedStep;
                }
                
                // Unnessesary because each object can update their own position
                // float alpha = accumulator / fixedStep;
                // m_physicsService.InterpolateObjects(alpha);
                
                while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
                {
                    InputManager.Update(e);
                    Debug.LogPollEvents(e);
                    HandleWindowEvents(e, ref running); 
                }

                SDL.SDL_SetRenderDrawColor(m_renderer, 5, 5, 5, 255);
                SDL.SDL_RenderClear(m_renderer);

                var camera = m_cameraService.GetCamera(m_camera);
                HandleCameraInput(camera);

                game.Update(Time.DeltaTime);
                game.Render();

                ImGui.NewFrame();
                game.RenderGui();
                RenderGUI();
                SDL.SDL_RenderPresent(m_renderer);
            }

            Dispose();
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

            if (e.type == SDL.SDL_EventType.SDL_WINDOWEVENT &&
                e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
            {
                string title = SDL.SDL_GetWindowTitle(m_window);
                uint flags = SDL.SDL_GetWindowFlags(m_window);
                bool isFullscreen = (flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) != 0 ||
                                    (flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) != 0;

                m_windowWidth = e.window.data1;
                m_windowHeight = e.window.data2;
                m_guiRenderService.OnWindowResize(m_windowWidth, m_windowHeight);
                m_physicsService.CreateWindowBoundaries(m_windowWidth, m_windowHeight);

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
            
            m_audioService.Cleanup();
            m_imageService.Cleanup();
            
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
