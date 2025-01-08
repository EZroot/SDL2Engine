using System.Numerics;
using ImGuiNativeWrapper;
using ImGuiNET;
using SDL2;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.GuiRenderer.Converters;
using SDL2Engine.Core.Input;
using static SDL2Engine.Core.GuiRenderer.GuiStyles.StyleHelper;
namespace SDL2Engine.Core.GuiRenderer
{
    public class ImGuiRenderService : IServiceGuiRenderService
    {
        private IntPtr m_window;
        private IntPtr m_renderer;
        private int m_width;
        private int m_height;
        private FontTextureLoader m_fontTextureLoader;
        private IServiceSysInfo m_sysInfo;
        private bool m_disposed;

        // ew - format plz
        private bool isDockInitialized;
        private uint m_dockSpaceMainID;
        private uint m_dockSpaceLeftID;
        private uint m_dockSpaceTopID;
        private uint m_dockSpaceRightID;
        private uint m_dockSpaceBottomID;
        private uint m_dockSpaceCenterID;
        public uint DockSpaceMainID => m_dockSpaceMainID;
        public uint DockSpaceLeftID => m_dockSpaceLeftID;
        public uint DockSpaceTopID => m_dockSpaceTopID;
        public uint DockSpaceRightID => m_dockSpaceRightID;
        public uint DockSpaceBottomID => m_dockSpaceBottomID;
        public uint DockSpaceCenterID => m_dockSpaceCenterID;



        /********************************************************
        *********************************************************
                REMOVE THIS - DEBUG TOP MENU WINDOW VARIABLES
        *******************************************************
        *******************************************************/
        int selectedItem = 0;
        string[] items = { "Item 1", "Item 2", "Item 3", "Item 4" };
        bool test = false;
        bool isChecked = true;

        public ImGuiRenderService(IServiceSysInfo sysInfo)
        {
            m_sysInfo = sysInfo ?? throw new ArgumentNullException(nameof(sysInfo));
        }

        public void CreateGuiRender(IntPtr window, IntPtr renderer, int width, int height, DefaultGuiStyle defaultStyle = DefaultGuiStyle.Dark)
        {
            m_window = window;
            m_renderer = renderer;
            m_width = width;
            m_height = height;

            m_fontTextureLoader = new FontTextureLoader(renderer);
            m_fontTextureLoader.LoadFontTexture();

            switch (defaultStyle)
            {
                case DefaultGuiStyle.Classic:
                    ImGui.StyleColorsClassic();
                    break;
                case DefaultGuiStyle.Light:
                    ImGui.StyleColorsLight();
                    break;
                case DefaultGuiStyle.Dark:
                    ImGui.StyleColorsDark();
                    break;
                case DefaultGuiStyle.None:
                    break;
            }
        }

        public void SetupIO(int windowWidth, int windowHeight)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(windowWidth, windowHeight);
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
        }

        /// <summary>
        /// Initializes the dock space layout. Should be called once during application setup.
        /// </summary>
        private void InitializeDockSpace()
        {
            m_dockSpaceMainID = ImGui.GetID("MainDockSpace");

            if (ImGuiInternal.DockBuilderGetNode(m_dockSpaceMainID) == IntPtr.Zero)
            {
                ImGuiInternal.DockBuilderRemoveNode(m_dockSpaceMainID);
                ImGuiInternal.DockBuilderAddNode(m_dockSpaceMainID, ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.AutoHideTabBar);
                ImGuiInternal.DockBuilderSetNodeSize(m_dockSpaceMainID, ImGui.GetIO().DisplaySize);

                ImGuiInternal.DockBuilderSplitNode(m_dockSpaceMainID, ImGuiDir.Up, 0.15f, out m_dockSpaceTopID, out uint remainingID);
                ImGuiInternal.DockBuilderSplitNode(remainingID, ImGuiDir.Down, 0.15f, out m_dockSpaceBottomID, out m_dockSpaceCenterID);
                ImGuiInternal.DockBuilderSplitNode(m_dockSpaceCenterID, ImGuiDir.Left, 0.25f, out m_dockSpaceLeftID, out m_dockSpaceRightID);

                ImGuiInternal.DockBuilderDockWindow("Left Window", m_dockSpaceLeftID);
                ImGuiInternal.DockBuilderDockWindow("Right Window", m_dockSpaceRightID);
                ImGuiInternal.DockBuilderDockWindow("Top Window", m_dockSpaceTopID);
                ImGuiInternal.DockBuilderDockWindow("Bottom Window", m_dockSpaceBottomID);

                ImGuiInternal.DockBuilderFinish(m_dockSpaceMainID);
                isDockInitialized = true;
            }
        }

        /// <summary>
        /// Renders the full-screen dock space. Should be called every frame.
        /// </summary>
        public void RenderFullScreenDockSpace()
        {
            if (!isDockInitialized)
                InitializeDockSpace();

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.DockingSeparatorSize, 1.0f);

            ImGui.Begin("DockSpace Window", ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoCollapse
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoScrollWithMouse
            | ImGuiWindowFlags.NoBackground);

            ImGui.SetWindowPos(Vector2.Zero);
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

            // Fully transparent background for docked windows that use NoBackground flag!
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0));
            ImGui.DockSpace(m_dockSpaceMainID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.AutoHideTabBar);
            ImGui.PopStyleColor();

            ImGui.End();
            ImGui.PopStyleVar(4);


            if (ImGui.Begin("Top Window", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (ImGui.BeginMainMenuBar())
                {
                    if (ImGui.Button("File"))
                    {
                        ImGui.OpenPopup("MoreActionsPopup");
                    }

                    if (ImGui.BeginCombo("Options", items[selectedItem], ImGuiComboFlags.WidthFitPreview))
                    {
                        for (int n = 0; n < items.Length; n++)
                        {
                            bool isSelected = (selectedItem == n);
                            if (ImGui.Selectable(items[n], isSelected))
                            {
                                selectedItem = n;
                            }
                            if (isSelected)
                                ImGui.SetItemDefaultFocus();
                        }
                        ImGui.EndCombo();
                    }

                    if (ImGui.BeginPopup("MoreActionsPopup"))
                    {
                        if (ImGui.Button("Action 1"))
                        {
                            Console.WriteLine("Action 1 Triggered");
                            ImGui.CloseCurrentPopup();
                        }
                        if (ImGui.SmallButton("Smoll"))
                        {

                        }
                        if (ImGui.Button("Action 2"))
                        {
                            Console.WriteLine("Action 2 Triggered");
                            ImGui.CloseCurrentPopup();
                        }

                        if (ImGui.Checkbox("Test", ref test))
                        {

                        }
                        if (ImGui.Button("Smoll"))
                        {

                        }
                        if (ImGui.ArrowButton("Arrowed", ImGuiDir.Down))
                        {

                        }

                        if (ImGui.MenuItem("Enable Feature2", "", isChecked))
                        {
                            isChecked = !isChecked;
                        }
                        if (ImGui.MenuItem("Enable Feature3", "", isChecked))
                        {
                            isChecked = !isChecked;
                        }
                        if (ImGui.MenuItem("Enable Feature4", "", isChecked))
                        {
                            isChecked = !isChecked;
                        }

                        ImGui.EndPopup();
                    }
                    var fps = $"Fps: {Time.Fps:F2} (delta: {Time.DeltaTime:F2})";
                    var windowWidth = ImGui.GetWindowWidth();
                    var textWidth = ImGui.CalcTextSize(fps).X;
                    ImGui.SameLine(windowWidth - textWidth - ImGui.GetStyle().ItemSpacing.X * 2);
                    ImGui.Text($"Driver: {m_sysInfo.SDLRenderInfo.CurrentRenderDriver} {fps}");
                    ImGui.EndMainMenuBar();
                }

            }
            ImGui.End();


            if (ImGui.Begin("Bottom Window", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove))
            {
                ImGui.Text("BOTTOM dock");
                ImGui.BeginMenuBar();
                ImGui.Text("BOTTOM MENU BAR");
                ImGui.EndMenuBar();
            }
            ImGui.End();
            if (ImGui.Begin("Left Window", ImGuiWindowFlags.NoMove))
            {
                ImGui.Text("LEFT WINDOW BABY");
            }
            ImGui.End();
            if (ImGui.Begin("Right Window", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove))
            {
                ImGui.Text("AYOOOO");
            }
            ImGui.End();
        }

        public unsafe void RenderDrawData(ImDrawDataPtr drawData)
        {
            if (drawData.CmdListsCount == 0) return;

            ImGuiIOPtr io = ImGui.GetIO();
            float scaleX = io.DisplayFramebufferScale.X;
            float scaleY = io.DisplayFramebufferScale.Y;

            SDL.SDL_Rect viewport = new SDL.SDL_Rect { x = 0, y = 0, w = m_width, h = m_height };
            SDL.SDL_RenderSetViewport(m_renderer, ref viewport);

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];
                RenderCommandList(cmdList, scaleX, scaleY);
            }

            SDL.SDL_RenderSetClipRect(m_renderer, IntPtr.Zero);
        }

        private unsafe void RenderCommandList(ImDrawListPtr cmdList, float scaleX, float scaleY)
        {
            VertexConverter converter = new VertexConverter(cmdList);

            for (int cmdIdx = 0; cmdIdx < cmdList.CmdBuffer.Size; cmdIdx++)
            {
                ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmdIdx];
                if (pcmd.UserCallback != IntPtr.Zero) continue;

                SDL.SDL_Rect clipRect = converter.CalculateClipRect(pcmd, scaleX, scaleY, m_width, m_height);
                SDL.SDL_RenderSetClipRect(m_renderer, ref clipRect);

                converter.RenderGeometry(m_renderer, pcmd);
            }
        }

        public void ProcessEvent(SDL.SDL_Event e)
        {
            InputManager.ProcessEvent(e);
        }

        public void OnWindowResize(int width, int height)
        {
            m_width = width;
            m_height = height;

            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(width, height);
            io.DisplayFramebufferScale = new Vector2(1.0f, 1.0f);

            SDL.SDL_Rect viewport = new SDL.SDL_Rect { x = 0, y = 0, w = width, h = height };
            SDL.SDL_RenderSetViewport(m_renderer, ref viewport);

            // Below code no longer needed, DockingSpace does it automatically
            // Debug.Log($"<color=yellow>DockBuilderSetNodeSize:</color><color=white> {width} {height}</color>");
            // ImGuiInternal.DockBuilderSetNodeSize(m_dockSpaceMainID, new Vector2(width, height));
        }

        public void Dispose()
        {
            if (m_disposed) return;
            m_fontTextureLoader.Dispose();
            m_disposed = true;
        }
    }
}