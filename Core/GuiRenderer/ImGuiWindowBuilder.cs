using System.Numerics;
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

        public void BeginWindow(string title, ImGuiWindowFlags flags = ImGuiWindowFlags.None)
        {
            ImGui.Begin(title, flags);
        }

        public void EndWindow()
        {
            ImGui.End();
        }

        public void Draw(string key)
        {
            _binder.Draw(key);
        }
          public void CreateDockSpace()
        {
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize);
            ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            ImGui.Begin("MainDockSpace", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus);

            ImGui.DockSpace(ImGui.GetID("MainDockSpace"));

            ImGui.PopStyleVar(3);
            ImGui.End();
        }
    }
}