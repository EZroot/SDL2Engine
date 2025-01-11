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
        private const string SOUND_FOLDER = "/home/anon/Music";
        
        private const int ENGINE_VOLUME = 24; // 0 - 128
        
        public enum ExampleEnum { OptionA, OptionB, OptionC };
        
        private readonly IServiceWindowService m_windowService;
        private readonly IServiceRenderService m_renderService;
        private readonly IServiceGuiRenderService m_guiRenderService;
        private readonly IServiceGuiWindowService m_guiWindowBuilder;
        private readonly IVariableBinder m_guiVariableBinder;
        private readonly IServiceAssetManager m_assetManager;
        private readonly IServiceAudioLoader m_audioLoader;
        private readonly IServiceCameraService m_cameraService;

        private IntPtr m_window, m_renderer;
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
            IServiceCameraService cameraService
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
        }

        public void Run()
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

            m_guiRenderService.CreateGuiRender(m_window, m_renderer, windowWidth, windowHeight);
            m_guiRenderService.SetupIO(windowWidth, windowHeight);

            //Camera test
            m_camera = m_cameraService.CreateCamera(Vector2.One);
            m_cameraService.SetActiveCamera(m_camera);
                
            //Sprite Test
            var spriteTexture = m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/ashh.png");
            SDL.SDL_Rect dstRectAsh = new SDL.SDL_Rect { x = 0, y = 0, w = spriteTexture.Width, h = spriteTexture.Height };
            var startPosition = new Vector2(48, 174);
            var originalScale = new Vector2(spriteTexture.Width, spriteTexture.Height);
            var position = startPosition;
            var currentScale = 1.0f;

            //Lil pokemans
            var spriteTexturePokemans = new TextureData[] { 
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER+"/poliwhirl.png"),
                };

            var originalScales = new List<Vector2>();
            var currScales = new List<float>();
            var dstRects = new List<SDL.SDL_Rect>();
            for(var i = 0; i < spriteTexturePokemans.Length; i++)
            {
                var width = spriteTexturePokemans[i].Width;
                var height = spriteTexturePokemans[i].Height;
                int row = i / 10;
                int col = i % 10;
                var spacing = -20;
                var startPos = new Vector2(330 + (col * (width + spacing)), 0 + (row * (height + spacing)));

                // var startPos = new Vector2(150 + (i*width + width + 64), 60 * j+i);
                var rec =  new SDL.SDL_Rect { x = (int)startPos.X, y = (int)startPos.Y, w = width, h = height };
                dstRects.Add(rec);
                currScales.Add(1.0f);
                originalScales.Add(new Vector2(width,height));
            }

            var songPath = SOUND_FOLDER + "/skidrow-portal.wav";//pokemon.wav";

            // Music Test
            // var song = m_assetManager.LoadSound(songPath, Addressables.AudioLoader.AudioType.Music);
            // m_assetManager.PlaySound(song, 16, true);

            // Soundfx Test
            var song = m_assetManager.LoadSound(songPath);
            m_assetManager.PlaySound(song, ENGINE_VOLUME);

            bool running = true;
            while (running)
            {
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

                if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_w))
                    position.Y -= 20f * Time.DeltaTime;
                if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_a))
                    position.X -= 20f * Time.DeltaTime;
                if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_s))
                    position.Y += 20f * Time.DeltaTime;
                if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_d))
                    position.X += 20f * Time.DeltaTime;

                if (dstRectAsh.x != (int)position.X || dstRectAsh.y != (int)position.Y)
                {
                    // Debug.Log($"X:{position.X} Y:{position.Y}");
                    dstRectAsh.x = (int)(position.X);
                    dstRectAsh.y = (int)(position.Y);
                }

                var baseScale = 0.75f;
                var amplitude = m_audioLoader.GetAmplitudeByType(FreqBandType.LowMid);
                var scaleFactor = baseScale + amplitude; // A highre freq band, to hopefully grab vocals
                currentScale = MathHelper.Lerp(currentScale, scaleFactor, 0.1f);
                var maxScale = 3f;
                currentScale = Math.Min(currentScale, maxScale);
                dstRectAsh.w = (int)(originalScale.X * currentScale);
                dstRectAsh.h = (int)(originalScale.Y * currentScale);

                var pokemansBaseScale = 0.5f;
                var pokemansMaxScale = 5f;
                
                for (var i = 0; i < spriteTexturePokemans.Length; i++)
                {
                    var pulseOffset = (i * 0.5f) % MathHelper.TwoPi;  
                    // Grab low freq band for the bass
                    var dynamicScaleFactor = pokemansBaseScale 
                    +  m_audioLoader.GetAmplitudeByType(FreqBandType.Bass) 
                    * (5f + (float)Math.Sin(Time.TotalTime + pulseOffset));

                    currScales[i] = MathHelper.Lerp(currScales[i], dynamicScaleFactor, 0.1f );
                    currScales[i] = Math.Min(currScales[i], pokemansMaxScale);
                    var ogScale = originalScales[i];
                    var bounceX = 10f * (float)Math.Sin(Time.TotalTime * 2f + i * 0.5f); 
                    var bounceY = 5f * (float)Math.Cos(Time.TotalTime * 3f + i * 0.3f);  
                    var rec = dstRects[i];
                    rec.w = (int)(ogScale.X * currScales[i]);
                    rec.h = (int)(ogScale.Y * currScales[i]);
                    rec.x += (int)bounceX;
                    rec.y += (int)bounceY;
                    m_assetManager.DrawTexture(m_renderer, spriteTexturePokemans[i].Id, ref rec, camera);
                }

                m_assetManager.DrawTexture(m_renderer, spriteTexture.Id, ref dstRectAsh);
                
                ImGui.NewFrame();
                
                m_guiRenderService.RenderFullScreenDockSpace();
                
                RenderGUI();
                SDL.SDL_RenderPresent(m_renderer);
            }

            m_assetManager.UnloadTexture(spriteTexture.Id);
            for(var i = 0; i < spriteTexturePokemans.Length; i++)
            {
                m_assetManager.UnloadTexture(spriteTexturePokemans[i].Id);
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
        }
        
        /// <summary>
        /// Handles user input for camera movement and zoom.
        /// </summary>
        private void HandleCameraInput(ICamera camera)
        {
            float cameraSpeed = 50f * Time.DeltaTime; // Adjust speed as needed
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
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_EQUALS)) // '+' key
                camera.SetZoom(camera.Zoom + 0.1f);
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_MINUS)) // '-' key
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
