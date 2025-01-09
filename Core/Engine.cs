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
using static SDL2Engine.Core.Addressables.AssetManager;

namespace SDL2Engine.Core
{
    internal class Engine : IDisposable
    {
        public enum ExampleEnum { OptionA, OptionB, OptionC };
        private readonly IServiceWindowService m_windowService;
        private readonly IServiceRenderService m_renderService;
        private readonly IServiceGuiRenderService m_guiRenderService;
        private readonly IServiceGuiWindowService m_guiWindowBuilder;
        private readonly IVariableBinder m_guiVariableBinder;
        private readonly IServiceAssetManager m_assetManager;
        private readonly IServiceAudioLoader m_audioLoader;

        private IntPtr m_window, m_renderer;

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
            IServiceAudioLoader audioLoader
        )
        {
            m_windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            m_renderService = renderService ?? throw new ArgumentNullException(nameof(renderService));
            m_guiRenderService = guiRenderService ?? throw new ArgumentNullException(nameof(guiRenderService));
            m_guiWindowBuilder = guiWindowBuilder ?? throw new ArgumentNullException(nameof(guiWindowBuilder));
            m_assetManager = assetManager ?? throw new ArgumentNullException(nameof(assetManager));
            m_guiVariableBinder = guiVariableBinder ?? throw new ArgumentNullException(nameof(guiVariableBinder));
            m_audioLoader = audioLoader ?? throw new ArgumentNullException(nameof(audioLoader));
        }

        public unsafe void Run()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Debug.LogError("SDL could not initialize! SDL_Error: " + SDL.SDL_GetError());
                return;
            }

            m_window = m_windowService.CreateWindowSDL();
            m_windowService.SetWindowIcon(m_window, "resources/ashh.png");

            m_renderer = m_renderService.CreateRenderer(m_window);

            IntPtr imguiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(imguiContext);

            SDL.SDL_GetWindowSize(m_window, out var windowWidth, out var windowHeight);

            m_guiRenderService.CreateGuiRender(m_window, m_renderer, windowWidth, windowHeight);
            m_guiRenderService.SetupIO(windowWidth, windowHeight);


            CustomBindTesting();

            //Sprite Test
            var spriteTexture = m_assetManager.LoadTexture(m_renderer, "resources/ashh.png");
            SDL.SDL_Rect dstRectAsh = new SDL.SDL_Rect { x = 0, y = 0, w = spriteTexture.Width, h = spriteTexture.Height };
            var startPosition = new Vector2(48, 174);
            var originalScale = new Vector2(spriteTexture.Width, spriteTexture.Height);
            var position = startPosition;//new Vector2();
            var currentScale = 1.0f;

            //Lil pokemans
            var spriteTexturePokemans = new TextureData[] { 
                m_assetManager.LoadTexture(m_renderer, "resources/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/ninetales.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/charizard.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/gengar.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/jigglypuff.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/moltres.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/poliwhirl.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/squirtle.png"),
                m_assetManager.LoadTexture(m_renderer, "resources/ninetales.png"),
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

            var songPath = "/home/anon/Music/pokemon.wav"; //"resources/sound/skidrow-portal.wav"

            // Music Test
            // var song = m_assetManager.LoadSound(songPath, Addressables.AudioLoader.AudioType.Music);
            // m_assetManager.PlaySound(song, 16, true);

            // Soundfx Test
            var song = m_assetManager.LoadSound(songPath);
            m_assetManager.PlaySound(song, 16);

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
                // Multiplying by 60 because we want to account for the game running at 60 fps
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
                var scaleFactor = baseScale + m_audioLoader.PlayingSongMidFreqBand;
                currentScale = MathHelper.Lerp(currentScale, scaleFactor, 0.1f);
                var maxScale = 3f;
                currentScale = Math.Min(currentScale, maxScale);
                dstRectAsh.w = (int)(originalScale.X * currentScale);
                dstRectAsh.h = (int)(originalScale.Y * currentScale);

                SDL.SDL_SetRenderDrawColor(m_renderer, 25, 25, 25, 255);
                SDL.SDL_RenderClear(m_renderer);

                var pokemansBaseScale = 0.5f;
                var pokemansMaxScale = 5f;

                for (var i = 0; i < spriteTexturePokemans.Length; i++)
                {
                    var pulseOffset = (i * 0.5f) % MathHelper.TwoPi;  
                    var dynamicScaleFactor = pokemansBaseScale + m_audioLoader.PlayingSongLowFreqBand * (5f + (float)Math.Sin(Time.TotalTime + pulseOffset));
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
                    m_assetManager.DrawTexture(m_renderer, spriteTexturePokemans[i].Id, ref rec);
                }

                m_assetManager.DrawTexture(m_renderer, spriteTexture.Id, ref dstRectAsh);

                ImGui.NewFrame();

                m_guiRenderService.RenderFullScreenDockSpace();

                // m_guiWindowBuilder.BeginWindow("Test Window", ImGuiWindowFlags.AlwaysVerticalScrollbar);
                //     m_guiWindowBuilder.Draw("Table");
                //     m_guiWindowBuilder.Draw("CellTable");
                //     m_guiWindowBuilder.Draw("Action");
                // m_guiWindowBuilder.EndWindow();

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

            m_assetManager.UnloadTexture(spriteTexture.Id);
            for(var i = 0; i < spriteTexturePokemans.Length; i++)
            {
                m_assetManager.UnloadTexture(spriteTexturePokemans[i].Id);
            }

            Dispose();
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

        private void CustomBindTesting()
        {
            // Numeric types
            int someInteger = 42;
            float someFloat = 3.14f;
            double someDouble = 2.71828;
            long someLong = 1234567890;
            short someShort = 32000;
            byte someByte = 255;
            uint someUInt = 123456u;
            ulong someULong = 12345678901234567890ul;
            ushort someUShort = 65000;
            sbyte someSByte = -100;

            bool someBool = true;

            string someString = "Initial Text";
            string[] someTabs = { "Tab 1", "Tab 2", "Tab 3" };

            ExampleEnum someEnum = ExampleEnum.OptionA;

            Vector2 someVector2 = new Vector2(1.0f, 2.0f);
            Vector3 someVector3 = new Vector3(1.0f, 2.0f, 3.0f);
            Vector4 someVector4 = new Vector4(1.0f, 2.0f, 3.0f, 4.0f);

            Color someColor = Color.Aqua;

            // Table
            var tableInputData = new ImGuiInputData("Alice", "Alice");
            var tableInputData1 = new ImGuiInputData("Bob", "Bob");
            var tableInputData2 = new ImGuiInputData("Charlie", "Charlie");
            var tableInputData3 = new ImGuiInputData("30", "30", true);
            var tableInputData4 = new ImGuiInputData("25", "25", true);
            var tableInputData5 = new ImGuiInputData("22", "22", true);
            var tableInputData6 = new ImGuiInputData("Engineer", "Engineer");
            var tableInputData7 = new ImGuiInputData("Designer", "Designer", true);
            var tableInputData8 = new ImGuiInputData("Manager", "Manager");

            var tableFlags = ImGuiTableFlags.None;
            var labelOnRight = true;
            var table = new ImGuiTableData(
                tableFlags,
                labelOnRight,
                new ImGuiColumnData("Name", tableInputData, tableInputData1, tableInputData2),
                new ImGuiColumnData("Age", tableInputData3, tableInputData4, tableInputData5),
                new ImGuiColumnData("Ocupation", tableInputData6, tableInputData7, tableInputData8)
            );

            ImGuiCellData someCell = new ImGuiCellData("First", "a", "b", "c");
            ImGuiCellData someCell1 = new ImGuiCellData("Second", "d", "e", "f");
            ImGuiCellData someCell2 = new ImGuiCellData("Third", "g", "h", "i");
            ImGuiCellTableData cellTable = new ImGuiCellTableData(someCell, someCell1, someCell2);

            Action action = () => { Debug.Log("Button pressed"); TEST_window_isopen = !TEST_window_isopen; };

            m_guiVariableBinder.BindVariable("Integer", someInteger);
            m_guiVariableBinder.BindVariable("Float", someFloat);
            m_guiVariableBinder.BindVariable("Double", someDouble);
            m_guiVariableBinder.BindVariable("Long", someLong);
            m_guiVariableBinder.BindVariable("Short", someShort);
            m_guiVariableBinder.BindVariable("Byte", someByte);
            m_guiVariableBinder.BindVariable("UInt", someUInt);
            m_guiVariableBinder.BindVariable("ULong", someULong);
            m_guiVariableBinder.BindVariable("UShort", someUShort);
            m_guiVariableBinder.BindVariable("SByte", someSByte);
            m_guiVariableBinder.BindVariable("Bool", someBool);
            m_guiVariableBinder.BindVariable("String", someString);
            // m_guiVariableBinder.BindVariable("Tabs", someTabs);
            m_guiVariableBinder.BindVariable("Enum", someEnum);
            m_guiVariableBinder.BindVariable("Vector2", someVector2);
            m_guiVariableBinder.BindVariable("Vector3", someVector3);
            m_guiVariableBinder.BindVariable("Vector4", someVector4);
            m_guiVariableBinder.BindVariable("Table", table);
            m_guiVariableBinder.BindVariable("CellTable", cellTable);
            m_guiVariableBinder.BindVariable("Action", action);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            m_assetManager.Cleanup();
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
