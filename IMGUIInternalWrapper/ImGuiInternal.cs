using System.Numerics;
using ImGuiNET;

namespace ImGuiNativeWrapper
{
    public static class ImGuiInternal
    {
        public static void DockBuilderAddNode(uint dockId, ImGuiDockNodeFlags flags = ImGuiDockNodeFlags.None)
        {
            ImGuiInternalWrapper.igDockBuilderAddNode(dockId, flags);
        }

        public static void DockBuilderRemoveNode(uint dockId)
        {
            ImGuiInternalWrapper.igDockBuilderRemoveNode(dockId);
        }

        public static void DockBuilderSetNodeSize(uint dockId, Vector2 size)
        {
            ImGuiInternalWrapper.igDockBuilderSetNodeSize(dockId, size);
        }

        public static void DockBuilderSplitNode(uint dockId, ImGuiDir splitDir, float ratio, out uint outIdAtDir, out uint outIdAtOppositeDir)
        {
            ImGuiInternalWrapper.igDockBuilderSplitNode(dockId, splitDir, ratio, out outIdAtDir, out outIdAtOppositeDir);
        }

        public static void DockBuilderDockWindow(string windowName, uint dockId)
        {
            ImGuiInternalWrapper.igDockBuilderDockWindow(windowName, dockId);
        }

        public static void DockBuilderFinish(uint dockId)
        {
            ImGuiInternalWrapper.igDockBuilderFinish(dockId);
        }

        public static IntPtr DockBuilderGetNode(uint dockId)
        {
            return ImGuiInternalWrapper.igDockBuilderGetNode(dockId);
        }

        public static void DockBuilderSetNodePos(uint dockId, Vector2 pos)
        {
            ImGuiInternalWrapper.igDockBuilderSetNodePos(dockId, pos);
        }

        public static void DockBuilderRemoveNodeDockedWindows(uint dockId)
        {
            ImGuiInternalWrapper.igDockBuilderRemoveNodeDockedWindows(dockId);
        }

        public static void DockBuilderRemoveNodeChildNodes(uint dockId)
        {
            ImGuiInternalWrapper.igDockBuilderRemoveNodeChildNodes(dockId);
        }

        public static void DockBuilderCopyDockSpace(uint srcDockId, uint dstDockId, IntPtr windowRemapPairs)
        {
            ImGuiInternalWrapper.igDockBuilderCopyDockSpace(srcDockId, dstDockId, windowRemapPairs);
        }

        public static void DockBuilderCopyNode(uint srcNodeId, uint dstNodeId, IntPtr windowRemapPairs)
        {
            ImGuiInternalWrapper.igDockBuilderCopyNode(srcNodeId, dstNodeId, windowRemapPairs);
        }
    }
}
