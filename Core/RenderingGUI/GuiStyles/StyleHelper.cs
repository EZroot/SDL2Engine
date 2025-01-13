using System.Numerics;
using ImGuiNET;

namespace SDL2Engine.Core.GuiRenderer.GuiStyles
{
    public static class StyleHelper
    {
        public enum DefaultGuiStyle { None, Classic, Light, Dark }

        private static void PushCustomTransparentStyle()
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            // Backup current style
            // ImGuiStyle backupStyle = style;

            // // Create a new style based on the current one
            // ImGuiStyle newStyle = backupStyle;

            // // Set WindowBg and other relevant colors to transparent
            // newStyle.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0, 0, 0, 0);
            // newStyle.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0, 0, 0, 0);
            // newStyle.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0, 0, 0, 0);
            // newStyle.Colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0, 0, 0, 0);

            // // Apply the new style
            // ImGui.PushStyleColor(ImGuiCol.WindowBg, newStyle.Colors[(int)ImGuiCol.WindowBg]);
            // ImGui.PushStyleColor(ImGuiCol.ChildBg, newStyle.Colors[(int)ImGuiCol.ChildBg]);
            // ImGui.PushStyleColor(ImGuiCol.PopupBg, newStyle.Colors[(int)ImGuiCol.PopupBg]);
            // ImGui.PushStyleColor(ImGuiCol.DockingEmptyBg, newStyle.Colors[(int)ImGuiCol.DockingEmptyBg]);
        }

        private static void PopCustomStyle()
        {
            ImGui.PopStyleColor(4); // Number of PushStyleColor calls
        }
    }
}