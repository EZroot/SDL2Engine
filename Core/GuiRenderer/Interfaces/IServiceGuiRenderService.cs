using ImGuiNET;
using SDL2;

namespace SDL2Engine.Core.GuiRenderer
{
    public interface IServiceGuiRenderService : IDisposable
    {
        public uint DockSpaceMainID { get; }
        public uint DockSpaceLeftID { get; }
        public uint DockSpaceTopID { get; }
        public uint DockSpaceRightID { get; }
        public uint DockSpaceBottomID { get; }
        public uint DockSpaceCenterID { get; }
        void CreateGuiRender(IntPtr window, IntPtr renderer, int width, int height);
        void SetupIO(int windowWidth, int windowHeight);
        void RenderFullScreenDockSpace();
        void RenderDrawData(ImDrawDataPtr drawData);
        void ProcessEvent(SDL.SDL_Event e);
        void OnWindowResize(int width, int height);
    }
}