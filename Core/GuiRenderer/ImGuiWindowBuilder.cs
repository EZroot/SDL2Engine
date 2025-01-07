using ImGuiNET;
using SDL2Engine.Core.GuiRenderer.Interfaces;
namespace SDL2Engine.Core.GuiRenderer
{
public class ImGuiWindowBuilder : IServiceGuiWindowService
    {
        private IVariableBinder _binder;

        public ImGuiWindowBuilder(IVariableBinder binder)
        {
            _binder = binder;
        }

        public void BeginWindow(string title)
        {
            ImGui.Begin(title);
        }

        public void EndWindow()
        {
            ImGui.End();
        }

        public void Draw(string key)
        {
            _binder.Draw(key);
        }
    }
}