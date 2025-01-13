using System.Numerics;
using ImGuiNET;
namespace SDL2Engine.Core.GuiRenderer
{
    public static class ImGuiDebugConsoleWindow
    {
        public static void ShowDebugConsole(ref bool isOpen)
        {
            // if (ImGui.Begin("Console Output", ref isOpen, ImGuiWindowFlags.MenuBar))
            // {
            //     ImGui.BeginMenuBar();
            //     if (ImGui.Button("Input"))
            //     {
            //         Utils.Debug.IsDebugModePollEvents = !Utils.Debug.IsDebugModePollEvents;
            //         Utils.Debug.Log($"Debug Input Poll: {Utils.Debug.IsDebugModePollEvents}");
            //     }
            //     if (ImGui.Button("EventHub"))
            //     {
            //         Utils.Debug.IsDebugModeEventHub = !Utils.Debug.IsDebugModeEventHub;
            //         Utils.Debug.Log($"Debug EventHub: {Utils.Debug.IsDebugModeEventHub}");
            //     }
            //     ImGui.EndMenuBar();

            //     Vector2 availableSize = ImGui.GetContentRegionAvail();
            //     ImGui.BeginChild("ConsoleRegion", availableSize, ImGuiChildFlags.AutoResizeX | ImGuiChildFlags.AutoResizeY);
            //     ImGui.TextUnformatted(Utils.Debug.ConsoleText);
            //     ImGui.SetScrollHereY(1.0f);
            //     ImGui.EndChild();
            //     ImGui.End();
            // }
        }
    }
}