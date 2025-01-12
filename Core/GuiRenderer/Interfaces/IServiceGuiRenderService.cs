using ImGuiNET;
using SDL2;
using static SDL2Engine.Core.GuiRenderer.GuiStyles.StyleHelper;

namespace SDL2Engine.Core.GuiRenderer
{
    public interface IServiceGuiRenderService : IDisposable
    {
        public void CreateGuiRender(IntPtr window, IntPtr renderer, int width, int height, DefaultGuiStyle defaultStyle = DefaultGuiStyle.None);
        void SetupIO(int windowWidth, int windowHeight);
        void RenderFullScreenDockSpace();
        void RenderDrawData(ImDrawDataPtr drawData);
        void OnWindowResize(int width, int height);
    }
}