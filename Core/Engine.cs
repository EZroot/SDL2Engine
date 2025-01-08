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
using SDL2Engine.Core.GuiRenderer.Helpers;
using System.Drawing;
using System.Numerics;
using ImGuiNativeWrapper;

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
        private readonly IServiceImageLoader m_imageLoader;

        private IntPtr m_window, m_renderer;

        private bool disposed = false;

        private bool TEST_window_isopen = true;

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
            m_guiRenderService.SetupIO(windowWidth, windowHeight);


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

                SDL.SDL_SetRenderDrawColor(m_renderer, 80, 80, 255, 255);
                SDL.SDL_RenderClear(m_renderer);

                ImGui.NewFrame();

                // Render the dockspace
                m_guiRenderService.RenderFullScreenDockSpace();

                // m_guiWindowBuilder.BeginWindow("Test Window", ImGuiWindowFlags.AlwaysVerticalScrollbar);
                //     m_guiWindowBuilder.Draw("Integer");
                //     m_guiWindowBuilder.Draw("Float");
                //     m_guiWindowBuilder.Draw("Double");
                //     m_guiWindowBuilder.Draw("Long");
                //     m_guiWindowBuilder.Draw("Short");
                //     m_guiWindowBuilder.Draw("Byte");
                //     m_guiWindowBuilder.Draw("UInt");
                //     m_guiWindowBuilder.Draw("ULong");
                //     m_guiWindowBuilder.Draw("UShort");
                //     m_guiWindowBuilder.Draw("SByte");
                //     m_guiWindowBuilder.Draw("Bool");
                //     m_guiWindowBuilder.Draw("String");
                //     m_guiWindowBuilder.Draw("Enum");
                //     m_guiWindowBuilder.Draw("Vector2");
                //     m_guiWindowBuilder.Draw("Vector3");
                //     m_guiWindowBuilder.Draw("Vector4");
                //     m_guiWindowBuilder.Draw("Table");
                //     m_guiWindowBuilder.Draw("CellTable");
                //     m_guiWindowBuilder.Draw("Action");
                // m_guiWindowBuilder.EndWindow();

                // ImGui.ShowDebugLogWindow();
                // ImGui.ShowIDStackToolWindow();

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
