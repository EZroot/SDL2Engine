using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNativeWrapper;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using SDL2;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.GuiRenderer.Converters;
using SDL2Engine.Core.GuiRenderer.Helpers;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Rendering.Interfaces;
using static SDL2Engine.Core.GuiRenderer.GuiStyles.StyleHelper;
namespace SDL2Engine.Core.GuiRenderer
{
    public class ImGuiRenderService : IGuiRenderService
    {
        private IntPtr m_window;
        private IntPtr m_renderer;
        private int m_width;
        private int m_height;
        private FontTextureLoader m_fontTextureLoader;
        private bool m_disposed;

        public ImGuiRenderService()
        {
        }

        public void CreateGuiRenderSDL(IntPtr window, IntPtr renderer, int width, int height, DefaultGuiStyle defaultStyle = DefaultGuiStyle.Dark)
        {
            m_window = window;
            m_renderer = renderer;
            m_width = width;
            m_height = height;

            m_fontTextureLoader = new FontTextureLoader(renderer);
            m_fontTextureLoader.LoadFontTextureSDL();

            switch (defaultStyle)
            {
                case DefaultGuiStyle.Classic:
                    ImGui.StyleColorsClassic();
                    break;
                case DefaultGuiStyle.Light:
                    ImGui.StyleColorsLight();
                    break;
                case DefaultGuiStyle.Dark:
                    ImGui.StyleColorsDark();
                    break;
                case DefaultGuiStyle.None:
                    break;
            }
        }

        public void CreateGuiRenderOpenGL(IntPtr window, IntPtr renderer, int width, int height, DefaultGuiStyle defaultStyle = DefaultGuiStyle.Dark)
        {
            m_window = window;
            m_renderer = renderer;
            m_width = width;
            m_height = height;

            // todo: opengl version of my font loader once sprites/images are working
            // m_fontTextureLoader = new FontTextureLoader(renderer);
            // m_fontTextureLoader.LoadFontTextureSDL();

            switch (defaultStyle)
            {
                case DefaultGuiStyle.Classic:
                    ImGui.StyleColorsClassic();
                    break;
                case DefaultGuiStyle.Light:
                    ImGui.StyleColorsLight();
                    break;
                case DefaultGuiStyle.Dark:
                    ImGui.StyleColorsDark();
                    break;
                case DefaultGuiStyle.None:
                    break;
            }
        }

        public void SetupIOSDL(int windowWidth, int windowHeight)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(windowWidth, windowHeight);
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
        }

        public void SetupIOGL(int windowWidth, int windowHeight)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(windowWidth, windowHeight);
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);
            int fontTexture = GL.GenTexture();
            
            GL.BindTexture(TextureTarget.Texture2D, fontTexture);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
                width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            io.Fonts.SetTexID((IntPtr)fontTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
                width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            io.Fonts.SetTexID((IntPtr)fontTexture);
            io.Fonts.ClearTexData();           
        }

        /// <summary>
        /// Initializes the dock space layout. Should be called once during application setup.
        /// </summary>
        public ImGuiDockData InitializeDockSpace(ImGuiDockData dockSettings)
        {
            if (dockSettings.MainDock.IsEnabled == false)
            {
                Utils.Debug.LogError("Failed to setup dock. MainDock IsEnabled = false");
            }
            
            dockSettings.MainDock.Id = ImGui.GetID(dockSettings.MainDock.Name);

            if (ImGuiInternal.DockBuilderGetNode(dockSettings.MainDock.Id) == IntPtr.Zero)
            {
                ImGuiInternal.DockBuilderRemoveNode(dockSettings.MainDock.Id);
                ImGuiInternal.DockBuilderAddNode(dockSettings.MainDock.Id, ImGuiDockNodeFlags.PassthruCentralNode);
                ImGuiInternal.DockBuilderSetNodeSize(dockSettings.MainDock.Id, ImGui.GetIO().DisplaySize);

                ImGuiInternal.DockBuilderSplitNode(dockSettings.MainDock.Id, ImGuiDir.Up, 0.0f, out var dockSpaceTopId, out uint remainingID);
                ImGuiInternal.DockBuilderSplitNode(remainingID, ImGuiDir.Down, 0.15f, out var dockSpaceBottomId, out var centerId);
                ImGuiInternal.DockBuilderSplitNode(centerId, ImGuiDir.Left, 0.15f, out var dockSpaceLeftId, out var dockSpaceRightId);

                if (dockSettings.LeftDock.IsEnabled)
                {
                    dockSettings.LeftDock.Id = dockSpaceLeftId;
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.LeftDock.Name, dockSettings.LeftDock.Id);
                    Utils.Debug.Log($"Enabled [{dockSettings.LeftDock.Id}] - {dockSettings.LeftDock.Name}");
                }
                
                if (dockSettings.RightDock.IsEnabled)
                {
                    dockSettings.RightDock.Id = dockSpaceRightId;
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.RightDock.Name, dockSettings.RightDock.Id);
                    Utils.Debug.Log($"Enabled [{dockSettings.RightDock.Id}] - {dockSettings.RightDock.Name}");
                }
                
                if (dockSettings.TopDock.IsEnabled)
                {
                    dockSettings.TopDock.Id = dockSpaceTopId;
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.TopDock.Name, dockSettings.TopDock.Id);
                    Utils.Debug.Log($"Enabled [{dockSettings.TopDock.Id}] - {dockSettings.TopDock.Name}");
                }
                
                if (dockSettings.BottomDock.IsEnabled)
                {
                    dockSettings.BottomDock.Id = dockSpaceBottomId;
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.BottomDock.Name, dockSettings.BottomDock.Id);
                    Utils.Debug.Log($"Enabled [{dockSettings.BottomDock.Id}] - {dockSettings.BottomDock.Name}");
                }
                
                if(dockSettings.RightDock.IsEnabled)
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.RightDock.Name, dockSettings.RightDock.Id);
                if(dockSettings.TopDock.IsEnabled)
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.TopDock.Name, dockSettings.TopDock.Id);
                if(dockSettings.BottomDock.IsEnabled)
                    ImGuiInternal.DockBuilderDockWindow(dockSettings.BottomDock.Name, dockSettings.BottomDock.Id);

                ImGuiInternal.DockBuilderFinish(dockSettings.MainDock.Id);
                dockSettings.IsDockInitialized = true;
            }

            return dockSettings;
        }

        /// <summary>
        /// Renders the full-screen dock space. Should be called every frame.
        /// </summary>
        public void RenderFullScreenDockSpace(ImGuiDockData dockSettings)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.DockingSeparatorSize, 1.0f);

            var flags = ImGuiWindowFlags.NoTitleBar
                        | ImGuiWindowFlags.NoCollapse
                        | ImGuiWindowFlags.NoResize
                        | ImGuiWindowFlags.NoMove
                        | ImGuiWindowFlags.NoBringToFrontOnFocus
                        | ImGuiWindowFlags.NoDocking
                        | ImGuiWindowFlags.NoScrollbar
                        | ImGuiWindowFlags.NoScrollWithMouse
                        | ImGuiWindowFlags.NoBackground;

            if (dockSettings.HasFileMenu)
            {
                flags = ImGuiWindowFlags.NoTitleBar
                        | ImGuiWindowFlags.NoCollapse
                        | ImGuiWindowFlags.NoResize
                        | ImGuiWindowFlags.NoMove
                        | ImGuiWindowFlags.NoBringToFrontOnFocus
                        | ImGuiWindowFlags.NoDocking
                        | ImGuiWindowFlags.NoScrollbar
                        | ImGuiWindowFlags.NoScrollWithMouse
                        | ImGuiWindowFlags.NoBackground
                        | ImGuiWindowFlags.MenuBar;
            }

            ImGui.Begin("DockSpace Window", flags);

            ImGui.SetWindowPos(Vector2.Zero);
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

            // Fully transparent background for docked windows that use NoBackground flag!
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0));
            ImGui.DockSpace(dockSettings.MainDock.Id, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.AutoHideTabBar);
            ImGui.PopStyleColor();

            ImGui.End();
            ImGui.PopStyleVar(4);
            
            if (dockSettings.TopDock.IsEnabled)
            {
                if (ImGui.Begin(dockSettings.TopDock.Name, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove))
                {
                }

                ImGui.End();
            }

            if (dockSettings.BottomDock.IsEnabled)
            {
                if (ImGui.Begin(dockSettings.BottomDock.Name, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove))
                {
                }

                ImGui.End();
            }

            if (dockSettings.LeftDock.IsEnabled)
            {
                if (ImGui.Begin(dockSettings.LeftDock.Name, ImGuiWindowFlags.NoMove))
                {
                }

                ImGui.End();
            }

            if (dockSettings.RightDock.IsEnabled)
            {
                if (ImGui.Begin(dockSettings.RightDock.Name, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove))
                {
                }

                ImGui.End();
            }
        }

        public void RenderDrawData(ImDrawDataPtr drawData)
        {
            if (drawData.CmdListsCount == 0) return;

            ImGuiIOPtr io = ImGui.GetIO();
            float scaleX = io.DisplayFramebufferScale.X;
            float scaleY = io.DisplayFramebufferScale.Y;

            SDL.SDL_Rect viewport = new SDL.SDL_Rect { x = 0, y = 0, w = m_width, h = m_height };
            SDL.SDL_RenderSetViewport(m_renderer, ref viewport);

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];
                RenderCommandList(cmdList, scaleX, scaleY);
            }

            SDL.SDL_RenderSetClipRect(m_renderer, IntPtr.Zero);
        }

        private void RenderCommandList(ImDrawListPtr cmdList, float scaleX, float scaleY)
        {
            VertexConverter converter = new VertexConverter(cmdList);

            for (int cmdIdx = 0; cmdIdx < cmdList.CmdBuffer.Size; cmdIdx++)
            {
                ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmdIdx];
                if (pcmd.UserCallback != IntPtr.Zero) continue;

                SDL.SDL_Rect clipRect = converter.CalculateClipRect(pcmd, scaleX, scaleY, m_width, m_height);
                SDL.SDL_RenderSetClipRect(m_renderer, ref clipRect);

                converter.RenderGeometry(m_renderer, pcmd);
            }
        }



        public void RenderDrawDataGL(IRenderService renderService, ImDrawDataPtr drawData)
        {
            var glHandle = renderService.OpenGLHandleGui;

            // 1. Backup GL state you care about (blend, scissor, etc.)
            bool blendEnabled = GL.IsEnabled(EnableCap.Blend);
            bool cullEnabled = GL.IsEnabled(EnableCap.CullFace);
            bool depthTestEnabled = GL.IsEnabled(EnableCap.DepthTest);
            bool scissorEnabled = GL.IsEnabled(EnableCap.ScissorTest);

            GL.GetInteger(GetPName.BlendSrcRgb, out int oldBlendSrcRgb);
            GL.GetInteger(GetPName.BlendDstRgb, out int oldBlendDstRgb);
            GL.GetInteger(GetPName.BlendSrcAlpha, out int oldBlendSrcAlpha);
            GL.GetInteger(GetPName.BlendDstAlpha, out int oldBlendDstAlpha);
            GL.GetInteger(GetPName.BlendEquationRgb, out int oldBlendEquationRgb);
            GL.GetInteger(GetPName.BlendEquationAlpha, out int oldBlendEquationAlpha);

            // 2. Setup desired GL state for ImGui
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ScissorTest);

            float L = 0.0f;
            float R = drawData.DisplaySize.X;
            float T = 0.0f;
            float B = drawData.DisplaySize.Y;

            var ortho = new OpenTK.Mathematics.Matrix4(
                2.0f / (R - L), 0.0f, 0.0f, 0.0f,
                0.0f, 2.0f / (T - B), 0.0f, 0.0f,
                0.0f, 0.0f, -1.0f, 0.0f,
                (R + L) / (L - R), (T + B) / (B - T), 0.0f, 1.0f
            );

            // 4. Use our ImGui shader & set uniforms
            GL.UseProgram(glHandle.ShaderHandle);
            GL.Uniform1((int)glHandle.AttribLocationTex, 0); // set 'Texture' sampler to 0
            GL.UniformMatrix4((int)glHandle.AttribLocationProjMtx, false, ref ortho);

            // 5. Render command lists
            GL.BindVertexArray(glHandle.VaoHandle);

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];
                int vertexSize = cmdList.VtxBuffer.Size;
                int indexSize = cmdList.IdxBuffer.Size;

                // Upload vertex/index data
                GL.BindBuffer(BufferTarget.ArrayBuffer, glHandle.VboHandle);
                GL.BufferData(BufferTarget.ArrayBuffer,
                    vertexSize * Marshal.SizeOf<ImDrawVert>(),
                    cmdList.VtxBuffer.Data,
                    BufferUsageHint.StreamDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, glHandle.ElementsHandle);
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                    indexSize * sizeof(ushort),
                    cmdList.IdxBuffer.Data,
                    BufferUsageHint.StreamDraw);

                int idxOffset = 0;
                for (int cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
                {
                    ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmdi];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        // (If you use callbacks, handle them...)
                    }
                    else
                    {

                        float clipX = pcmd.ClipRect.X;
                        float clipY = pcmd.ClipRect.Y;
                        float clipW = pcmd.ClipRect.Z - clipX;
                        float clipH = pcmd.ClipRect.W - clipY;
                        int fbHeight = (int)drawData.DisplaySize.Y;
                        GL.Scissor(
                            (int)clipX,
                            (int)(fbHeight - (clipY + clipH)),
                            (int)clipW,
                            (int)clipH
                        );


                        // Bind texture
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);

                        // Draw
                        GL.DrawElements(
                            PrimitiveType.Triangles,
                            (int)pcmd.ElemCount,
                            DrawElementsType.UnsignedShort,
                            (IntPtr)(idxOffset * sizeof(ushort))
                        );
                    }

                    idxOffset += (int)pcmd.ElemCount;
                }
            }

            // Cleanup
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // 6. Restore GL state (blend, scissor, etc.) if you need it
            if (!blendEnabled) GL.Disable(EnableCap.Blend);
            if (cullEnabled) GL.Enable(EnableCap.CullFace);
            if (depthTestEnabled) GL.Enable(EnableCap.DepthTest);
            if (!scissorEnabled) GL.Disable(EnableCap.ScissorTest);

            // Also restore all the blend src/dest/equation if needed:
            // GL.BlendEquationSeparate((BlendEquationMode)oldBlendEquationRgb, (BlendEquationMode)oldBlendEquationAlpha);
            // GL.BlendFuncSeparate((BlendingFactor)oldBlendSrcRgb, (BlendingFactor)oldBlendDstRgb,
            //                      (BlendingFactor)oldBlendSrcAlpha, (BlendingFactor)oldBlendDstAlpha);
        }

        public void OnWindowResize(int width, int height)
        {
            m_width = width;
            m_height = height;

            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(width, height);
            io.DisplayFramebufferScale = new Vector2(1.0f, 1.0f);

            SDL.SDL_Rect viewport = new SDL.SDL_Rect { x = 0, y = 0, w = width, h = height };
            SDL.SDL_RenderSetViewport(m_renderer, ref viewport);
        }

        public void Dispose()
        {
            if (m_disposed) return;
            ImGui.DestroyContext();
            m_fontTextureLoader.Dispose();
            m_disposed = true;
        }
    }
}