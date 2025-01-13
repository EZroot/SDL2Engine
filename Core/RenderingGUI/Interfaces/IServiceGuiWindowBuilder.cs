using ImGuiNET;

namespace SDL2Engine.Core.GuiRenderer.Interfaces
{
    public interface IServiceGuiWindowBuilder
    {
        void BeginWindow(string title, ImGuiWindowFlags flags = ImGuiWindowFlags.None);
        void EndWindow();
        void Draw(string key);
    }
}