using ImGuiNET;
using SDL2;
namespace SDL2Engine.Core.GuiRenderer.Converters
{
    public class VertexConverter
    {
        private ImDrawListPtr m_cmdList;
        private float[] m_xy;
        private float[] m_uv;
        private int[] m_color;

        public VertexConverter(ImDrawListPtr cmdList)
        {
            m_cmdList = cmdList;
            m_xy = new float[cmdList.VtxBuffer.Size * 2];
            m_uv = new float[cmdList.VtxBuffer.Size * 2];
            m_color = new int[cmdList.VtxBuffer.Size];

            ConvertVertexBuffer();
        }

        private unsafe void ConvertVertexBuffer()
        {
            for (int i = 0; i < m_cmdList.VtxBuffer.Size; i++)
            {
                ImDrawVertPtr vertPtr = m_cmdList.VtxBuffer[i];
                ImDrawVert vert = new ImDrawVert { col = vertPtr.col, pos = vertPtr.pos, uv = vertPtr.uv };

                m_xy[i * 2] = vert.pos.X;
                m_xy[i * 2 + 1] = vert.pos.Y;
                m_uv[i * 2] = vert.uv.X;
                m_uv[i * 2 + 1] = vert.uv.Y;
                m_color[i] = (int)vert.col;
            }
        }

        public SDL.SDL_Rect CalculateClipRect(ImDrawCmdPtr pcmd, float scaleX, float scaleY, int width, int height)
        {
            SDL.SDL_Rect clipRect = new SDL.SDL_Rect
            {
                x = (int)(pcmd.ClipRect.X * scaleX),
                y = (int)(pcmd.ClipRect.Y * scaleY),
                w = (int)((pcmd.ClipRect.Z - pcmd.ClipRect.X) * scaleX),
                h = (int)((pcmd.ClipRect.W - pcmd.ClipRect.Y) * scaleY)
            };

            clipRect.x = Math.Max(clipRect.x, 0);
            clipRect.y = Math.Max(clipRect.y, 0);
            clipRect.w = Math.Min(clipRect.w, width - clipRect.x);
            clipRect.h = Math.Min(clipRect.h, height - clipRect.y);

            return clipRect;
        }

        public unsafe void RenderGeometry(IntPtr renderer, ImDrawCmdPtr pcmd)
        {
            ushort* idxBuffer = (ushort*)m_cmdList.IdxBuffer.Data.ToPointer();

            SDL.SDL_RenderGeometryRaw(
                renderer,
                (nint)pcmd.TextureId,
                m_xy, sizeof(float) * 2,
                m_color, sizeof(int),
                m_uv, sizeof(float) * 2,
                m_cmdList.VtxBuffer.Size,
                (nint)(idxBuffer + pcmd.IdxOffset),
                (int)pcmd.ElemCount,
                sizeof(ushort)
            );
        }
    }
}