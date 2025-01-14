using ImGuiNET;
using SDL2Engine.Core.GuiRenderer.Helpers;
using static SDL2Engine.Core.GuiRenderer.GuiStyles.StyleHelper;

namespace SDL2Engine.Core.GuiRenderer
{
    public interface IGuiRenderService : IDisposable
    {
        public void CreateGuiRender(IntPtr window, IntPtr renderer, int width, int height, DefaultGuiStyle defaultStyle = DefaultGuiStyle.None);
        void SetupIO(int windowWidth, int windowHeight);
        ImGuiDockData InitializeDockSpace(ImGuiDockData dockSettings);
        void RenderFullScreenDockSpace(ImGuiDockData dockSettings);
        void RenderDrawData(ImDrawDataPtr drawData);
        void OnWindowResize(int width, int height);
    }
}