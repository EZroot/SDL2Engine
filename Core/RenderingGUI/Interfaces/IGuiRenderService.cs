using ImGuiNET;
using SDL2Engine.Core.GuiRenderer.Helpers;
using SDL2Engine.Core.Cameras.Interfaces;
using static SDL2Engine.Core.GuiRenderer.GuiStyles.StyleHelper;

namespace SDL2Engine.Core.GuiRenderer
{
    public interface IGuiRenderService : IDisposable
    {
        void CreateGuiRender(IntPtr window, IntPtr renderer, int width, int height,
            DefaultGuiStyle defaultStyle = DefaultGuiStyle.Dark);
        void SetupIO(int windowWidth, int windowHeight);
        ImGuiDockData InitializeDockSpace(ImGuiDockData dockSettings);
        void RenderFullScreenDockSpace(ImGuiDockData dockSettings);
        void RenderDrawData(IRenderService renderService, ImDrawDataPtr drawData);
        void OnWindowResize(int width, int height);
    }
}