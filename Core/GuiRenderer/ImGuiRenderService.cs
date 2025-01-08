using ImGuiNativeWrapper;
using ImGuiNET;
using SDL2;
using SDL2Engine.Core.GuiRenderer.Converters;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Utils;
using System.Numerics;
namespace SDL2Engine.Core.GuiRenderer
{
    public class ImGuiRenderService : IServiceGuiRenderService
    {
        private IntPtr m_window;
        private IntPtr m_renderer;
        private int m_width;
        private int m_height;
        private FontTextureLoader m_fontTextureLoader;
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

        public ImGuiRenderService()
        {

        }

        public void CreateGuiRender(IntPtr window, IntPtr renderer, int width, int height)
        {
            m_window = window;
            m_renderer = renderer;
            m_width = width;
            m_height = height;

            m_fontTextureLoader = new FontTextureLoader(renderer);
            m_fontTextureLoader.LoadFontTexture();
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
    public void InitializeDockSpace()
    {
        m_dockSpaceMainID = ImGui.GetID("MainDockSpace");

        // Check if the dock node is already initialized
        if (ImGuiInternal.DockBuilderGetNode(m_dockSpaceMainID) == IntPtr.Zero)
        {
            // Remove any existing layout for this dock space
            ImGuiInternal.DockBuilderRemoveNode(m_dockSpaceMainID);

            // Add a new node with the PassthruCentralNode flag to allow the background to be visible
            ImGuiInternal.DockBuilderAddNode(m_dockSpaceMainID, ImGuiDockNodeFlags.PassthruCentralNode);

            // Set the size of the dock space to the current display size
            ImGuiInternal.DockBuilderSetNodeSize(m_dockSpaceMainID, ImGui.GetIO().DisplaySize);

            // Split the dock space into a left and right node
            uint dockSpaceLeftID, dockSpaceRightID;
            ImGuiInternal.DockBuilderSplitNode(
                m_dockSpaceMainID,
                ImGuiDir.Left,
                0.25f, // 25% of the width for the left pane
                out dockSpaceLeftID,
                out dockSpaceRightID
            );

            // Dock the left and right windows into their respective nodes
            ImGuiInternal.DockBuilderDockWindow("Left Window", dockSpaceLeftID);
            ImGuiInternal.DockBuilderDockWindow("Right Window", dockSpaceRightID);

            // Optionally, dock a central window if needed
            // ImGuiInternal.DockBuilderDockWindow("Central Window", dockSpaceMainID);

            // Finalize the dock builder to apply the layout
            ImGuiInternal.DockBuilderFinish(m_dockSpaceMainID);

            isDockInitialized = true;
        }
    }

    /// <summary>
    /// Renders the full-screen dock space. Should be called every frame.
    /// </summary>
    public void RenderFullScreenDockSpace()
    {
        // Initialize the dock space layout once
        if (!isDockInitialized)
        {
            InitializeDockSpace();
        }

        // Set style variables to make the dock space window borderless and full-screen
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        // Begin the DockSpace window
        ImGui.Begin("DockSpace Window",
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoDocking |
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoScrollWithMouse
        );

        // Ensure the DockSpace window covers the entire viewport
        ImGui.SetWindowPos(Vector2.Zero);
        ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

        // Remove window padding to make the dock space fill the window
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        // Create the dock space
        ImGui.DockSpace(m_dockSpaceMainID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        // Restore the window padding
        ImGui.PopStyleVar();

        // End the DockSpace window
        ImGui.End();

        // Restore the style variables
        ImGui.PopStyleVar(3);

        // Render the Left Window
        if (ImGui.Begin("Left Window",
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoBringToFrontOnFocus
        ))
        {
            ImGui.Text("Hello, bottom!");
        }
        ImGui.End();

        // Render the Right Window
        if (ImGui.Begin("Right Window",
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoBringToFrontOnFocus
        ))
        {
            ImGui.Text("Haaaaaaaaaom!");
        }
        ImGui.End();

        // Optionally, render the Central Window if you have one
        /*
        if (ImGui.Begin("Central Window",
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoBringToFrontOnFocus
        ))
        {
            ImGui.Text("This is the central window.");
        }
        ImGui.End();
        */
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

            Debug.Log($"Dock builder Set to {width} {height}");
            ImGuiInternal.DockBuilderSetNodeSize(m_dockSpaceMainID, new Vector2(width, height));
        }

        public void Dispose()
        {
            if (m_disposed) return;
            m_fontTextureLoader.Dispose();
            m_disposed = true;
        }
    }
}