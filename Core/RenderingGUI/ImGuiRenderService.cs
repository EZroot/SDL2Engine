using System.Diagnostics;
using System.Numerics;
using ImGuiNativeWrapper;
using ImGuiNET;
using SDL2;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.GuiRenderer.Converters;
using SDL2Engine.Core.GuiRenderer.Helpers;
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

        // more ew
        private bool m_isDebugConsoleOpen = true;

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
        public ImGuiDockData InitializeDockSpace(ImGuiDockData dockSettings)
        {
            if (dockSettings.MainDock.IsEnabled == false)
            {
                Utils.Debug.LogError("Failed to setup dock. MainDock IsEnabled = false");
            }
            
            dockSettings.MainDock.Id = ImGui.GetID(dockSettings.MainDock.Name);

            if (ImGuiInternal.DockBuilderGetNode(dockSettings.MainDock.Id) == IntPtr.Zero)
            {
                ImGuiInternal.DockBuilderRemoveNode(dockSettings.MainDock.Id);
                ImGuiInternal.DockBuilderAddNode(dockSettings.MainDock.Id, ImGuiDockNodeFlags.PassthruCentralNode);
                ImGuiInternal.DockBuilderSetNodeSize(dockSettings.MainDock.Id, ImGui.GetIO().DisplaySize);

                ImGuiInternal.DockBuilderSplitNode(dockSettings.MainDock.Id, ImGuiDir.Up, 0.0f, out var dockSpaceTopId, out uint remainingID);
                ImGuiInternal.DockBuilderSplitNode(remainingID, ImGuiDir.Down, 0.15f, out var dockSpaceBottomId, out var centerId);
                ImGuiInternal.DockBuilderSplitNode(centerId, ImGuiDir.Left, 0.15f, out var dockSpaceLeftId, out var dockSpaceRightId);

                if (dockSettings.LeftDock.IsEnabled)
                {
                    dockSettings.LeftDock.Id = dockSpaceLeftId;
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.LeftDock.Name, dockSettings.LeftDock.Id);
                    Utils.Debug.Log($"Enabled [{dockSettings.LeftDock.Id}] - {dockSettings.LeftDock.Name}");
                }
                
                if (dockSettings.RightDock.IsEnabled)
                {
                    dockSettings.RightDock.Id = dockSpaceRightId;
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.RightDock.Name, dockSettings.RightDock.Id);
                    Utils.Debug.Log($"Enabled [{dockSettings.RightDock.Id}] - {dockSettings.RightDock.Name}");
                }
                
                if (dockSettings.TopDock.IsEnabled)
                {
                    dockSettings.TopDock.Id = dockSpaceTopId;
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.TopDock.Name, dockSettings.TopDock.Id);
                    Utils.Debug.Log($"Enabled [{dockSettings.TopDock.Id}] - {dockSettings.TopDock.Name}");
                }
                
                if (dockSettings.BottomDock.IsEnabled)
                {
                    dockSettings.BottomDock.Id = dockSpaceBottomId;
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.BottomDock.Name, dockSettings.BottomDock.Id);
                    Utils.Debug.Log($"Enabled [{dockSettings.BottomDock.Id}] - {dockSettings.BottomDock.Name}");
                }
                
                if(dockSettings.RightDock.IsEnabled)
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.RightDock.Name, dockSettings.RightDock.Id);
                if(dockSettings.TopDock.IsEnabled)
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.TopDock.Name, dockSettings.TopDock.Id);
                if(dockSettings.BottomDock.IsEnabled)
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.BottomDock.Name, dockSettings.BottomDock.Id);

                ImGuiInternal.DockBuilderFinish(dockSettings.MainDock.Id);
                dockSettings.IsDockInitialized = true;
            }

            return dockSettings;
        }

        /// <summary>
        /// Renders the full-screen dock space. Should be called every frame.
        /// </summary>
        public void RenderFullScreenDockSpace(ImGuiDockData dockSettings)
        {
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
            | ImGuiWindowFlags.NoBackground
            | ImGuiWindowFlags.MenuBar);

            ImGui.SetWindowPos(Vector2.Zero);
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

            // Fully transparent background for docked windows that use NoBackground flag!
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0));
            ImGui.DockSpace(dockSettings.MainDock.Id, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);
            ImGui.PopStyleColor();

            ImGui.End();
            ImGui.PopStyleVar(4);

            RenderFileMenu();

            if (dockSettings.TopDock.IsEnabled)
            {
                if (ImGui.Begin(dockSettings.TopDock.Name, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove))
                {
                    ImGui.Text("Top docker window");
                }

                ImGui.End();
            }

            if (dockSettings.BottomDock.IsEnabled)
            {
                if (ImGui.Begin(dockSettings.BottomDock.Name, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove))
                {
                    ImGui.Text("Bottom docker window");
                }

                ImGui.End();
            }

            if (dockSettings.LeftDock.IsEnabled)
            {
                if (ImGui.Begin(dockSettings.LeftDock.Name, ImGuiWindowFlags.NoMove))
                {
                    ImGui.Text("Left docker window");
                }

                ImGui.End();
            }

            if (dockSettings.RightDock.IsEnabled)
            {
                if (ImGui.Begin(dockSettings.RightDock.Name, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove))
                {
                    ImGui.Text("Game Screen");
                }

                ImGui.End();
            }

            if (m_isDebugConsoleOpen)
                Utils.Debug.RenderDebugConsole(ref m_isDebugConsoleOpen);

        }

        public void RenderDrawData(ImDrawDataPtr drawData)
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

        public void OnWindowResize(int width, int height)
        {
            m_width = width;
            m_height = height;

            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(width, height);
            io.DisplayFramebufferScale = new Vector2(1.0f, 1.0f);

            SDL.SDL_Rect viewport = new SDL.SDL_Rect { x = 0, y = 0, w = width, h = height };
            SDL.SDL_RenderSetViewport(m_renderer, ref viewport);
        }

        private void RenderFileMenu()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.Button("File"))
                {
                    ImGui.OpenPopup("MoreActionsPopup");
                }

                if (ImGui.Button("Edit"))
                {
                    ImGui.OpenPopup("MoreActionsPopup");
                }

                if (ImGui.Button("Help"))
                {
                    ImGui.OpenPopup("MoreActionsPopup");
                }

                if (ImGui.Button("Debug"))
                {
                    ImGui.OpenPopup("DebugWindowPopup");
                }

                // if (ImGui.BeginPopup("MoreActionsPopup"))
                // {
                //     if (ImGui.Button("Action 1"))
                //     {
                //         Console.WriteLine("Action 1 Triggered");
                //         // ImGui.CloseCurrentPopup();
                //     }
                //     ImGui.Separator();
                //     if (ImGui.SmallButton("Smoll"))
                //     {

                //     }
                //     ImGui.Separator();

                //     if (ImGui.Button("Action 2"))
                //     {
                //         Console.WriteLine("Action 2 Triggered");
                //         // ImGui.CloseCurrentPopup();
                //     }
                //     ImGui.Separator();

                //     if (ImGui.Checkbox("Test", ref test))
                //     {

                //     }
                //     ImGui.Separator();

                //     if (ImGui.ArrowButton("Arrowed", ImGuiDir.Down))
                //     {

                //     }
                //     ImGui.Separator();

                //     if (ImGui.MenuItem("Enable Feature2", "", isChecked))
                //     {
                //         isChecked = !isChecked;
                //     }
                //     if (ImGui.MenuItem("Enable Feature3", "", isChecked))
                //     {
                //         isChecked = !isChecked;
                //     }
                //     if (ImGui.MenuItem("Enable Feature4", "", isChecked))
                //     {
                //         isChecked = !isChecked;
                //     }
                //     ImGui.Separator();

                //     if (ImGui.BeginCombo("Options", items[selectedItem], ImGuiComboFlags.WidthFitPreview))
                //     {
                //         for (int n = 0; n < items.Length; n++)
                //         {
                //             bool isSelected = (selectedItem == n);
                //             if (ImGui.Selectable(items[n], isSelected))
                //             {
                //                 selectedItem = n;
                //             }
                //             if (isSelected)
                //                 ImGui.SetItemDefaultFocus();
                //         }
                //         ImGui.EndCombo();
                //     }

                //     ImGui.EndPopup();
                // }

                if (ImGui.BeginPopup("DebugWindowPopup"))
                {
                    if (ImGui.Button("Show Console Output"))
                    {
                        m_isDebugConsoleOpen = !m_isDebugConsoleOpen;
                    }
                    ImGui.EndPopup();
                }

                var fps = $"Fps: {Time.Fps:F2} (delta: {Time.DeltaTime:F2})";
                var fullHeader = $"Driver: {m_sysInfo.SDLRenderInfo.CurrentRenderDriver} {fps}";
                var windowWidth = ImGui.GetWindowWidth();
                var textWidth = ImGui.CalcTextSize(fullHeader).X;
                ImGui.SameLine(windowWidth - textWidth - ImGui.GetStyle().ItemSpacing.X * 2);
                ImGui.Text(fullHeader);
                ImGui.EndMainMenuBar();
            }
        }

        public void Dispose()
        {
            if (m_disposed) return;
            ImGui.DestroyContext();
            m_fontTextureLoader.Dispose();
            m_disposed = true;
        }
    }
}