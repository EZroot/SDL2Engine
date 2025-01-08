using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;

/// <summary>
/// Custom unwrapping of dock node builders from cimgui.dll
/// The cimgui-native.h wasnt unwrapped in the original repo, very annoying ):<
/// There are better forks but this should do fine for now
/// </summary>
namespace ImGuiNativeWrapper
{
    internal static class ImGuiInternalWrapper
    {
        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void igDockBuilderAddNode(uint dock_id, ImGuiDockNodeFlags flags);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void igDockBuilderRemoveNode(uint dock_id);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void igDockBuilderSetNodeSize(uint dock_id, Vector2 size);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void igDockBuilderSplitNode(uint dock_id, ImGuiDir split_dir, float ratio, out uint out_id_at_dir, out uint out_id_at_opposite_dir);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void igDockBuilderDockWindow(string window_name, uint dock_id);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void igDockBuilderFinish(uint dock_id);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr igDockBuilderGetNode(uint dock_id);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void igDockBuilderSetNodePos(uint dock_id, Vector2 pos);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void igDockBuilderRemoveNodeDockedWindows(uint dock_id);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void igDockBuilderRemoveNodeChildNodes(uint dock_id);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void igDockBuilderCopyDockSpace(uint src_dock_id, uint dst_dock_id, IntPtr window_remap_pairs);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern void igDockBuilderCopyNode(uint src_node_id, uint dst_node_id, IntPtr window_remap_pairs);
    }
}
