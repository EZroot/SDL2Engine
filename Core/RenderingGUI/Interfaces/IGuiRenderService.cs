using ImGuiNET;
using SDL2Engine.Core.GuiRenderer.Helpers;
using SDL2Engine.Core.Rendering.Interfaces;
using static SDL2Engine.Core.GuiRenderer.GuiStyles.StyleHelper;

namespace SDL2Engine.Core.GuiRenderer
{
    public interface IGuiRenderService : IDisposable
    {
        void CreateGuiRenderSDL(IntPtr window, IntPtr renderer, int width, int height, DefaultGuiStyle defaultStyle = DefaultGuiStyle.None);
        void CreateGuiRenderOpenGL(IntPtr window, IntPtr renderer, int width, int height,
            DefaultGuiStyle defaultStyle = DefaultGuiStyle.Dark);
        void SetupIOSDL(int windowWidth, int windowHeight);
        void SetupIOGL(int windowWidth, int windowHeight);
        ImGuiDockData InitializeDockSpace(ImGuiDockData dockSettings);
        void RenderFullScreenDockSpace(ImGuiDockData dockSettings);
        void RenderDrawData(ImDrawDataPtr drawData);
        void RenderDrawDataGL(IRenderService renderService, ImDrawDataPtr drawData);
        void OnWindowResize(int width, int height);
    }
}