using ImGuiNET;

namespace SDL2Engine.Core.GuiRenderer.Interfaces
{
    public interface IServiceGuiWindowService
    {
        void BeginWindow(string title, ImGuiWindowFlags flags = ImGuiWindowFlags.None);
        void EndWindow();
        void Draw(string key);
    }
}