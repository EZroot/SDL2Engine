using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using SDL2;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Rendering
{
    internal class RenderService : IRenderService
    {
        private readonly ISysInfo m_sysInfo;
        private readonly IWindowConfig m_windowConfig; // Replace with a renderer config if I need to


        private IntPtr m_render;
        private OpenGLHandle m_glHandleGui;
        private OpenGLHandle m_glHandle2D;

        public IntPtr RenderPtr => m_render;
        public OpenGLHandle OpenGLHandleGui => m_glHandleGui;
        public OpenGLHandle OpenGLHandle2D => m_glHandle2D;

        public RenderService(ISysInfo sysInfo, IWindowConfig windowConfig)
        {
            m_windowConfig = windowConfig ?? throw new ArgumentNullException(nameof(windowConfig));
            m_sysInfo = sysInfo ?? throw new ArgumentNullException(nameof(sysInfo));
        }

        /// <summary>
        /// Create and return the handle of a renderer based on the platforminfo rendertype (SDL, or OPENGL)
        /// SDL render flags will be ignored if the render type is OPENGL
        /// </summary>
        /// <param name="window"></param>
        /// <param name="renderFlags"></param>
        /// <returns>Renderer handle</returns>
        public nint CreateRenderer(nint window, SDL.SDL_RendererFlags renderFlags =
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED)
        {
            if (PlatformInfo.RendererType == RendererType.OpenGlRenderer)
            {
                var renderer =  CreateOpenGLContext(window);
                CreateImGuiGLBindings(
                    FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER+"/shaders/imgui/imguishader.vert"),
                    FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER+"/shaders/imgui/imguishader.frag"));
                Create2DGLBindings(
                    FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER+"/shaders/2d/2dshader.vert"),
                    FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER+"/shaders/2d/2dshader.frag"));
                
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                return renderer;
            }
            
            // Assume default is sdl
            return CreateRendererSDL(window, renderFlags);
        }
        
        private IntPtr CreateRendererSDL(IntPtr window, SDL.SDL_RendererFlags renderFlags =
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED)
        {
            SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_DRIVER, "opengl");
            LogAvailableRenderDrivers();
            m_render = SDL.SDL_CreateRenderer(window, -1, renderFlags);
            if (m_render == IntPtr.Zero)
            {
                Debug.LogError("Renderer creation failed! SDL_Error: " + SDL.SDL_GetError());
                SDL.SDL_DestroyWindow(window);
                SDL.SDL_Quit();
                throw new InvalidOperationException("Renderer creation failed! SDL_Error: " + SDL.SDL_GetError());
            }

            PrintRenderDriver(m_render);
            return m_render;
        }

        private nint CreateOpenGLContext(nint window)
        {
            IntPtr glContext = SDL.SDL_GL_CreateContext(window);
            if (glContext == IntPtr.Zero)
            {
                Debug.LogError("OpenGL context creation failed! " + SDL.SDL_GetError());
                SDL.SDL_DestroyWindow(window);
                SDL.SDL_Quit();
                throw new InvalidOperationException("OpenGL context creation failed!");
            }

            if (SDL.SDL_GL_MakeCurrent(window, glContext) != 0)
            {
                Debug.LogError("Failed to make OpenGL context current! " + SDL.SDL_GetError());
                SDL.SDL_DestroyWindow(window);
                SDL.SDL_Quit();
                throw new InvalidOperationException("Failed to make OpenGL context current!");
            }

            // enable vsync.
            SDL.SDL_GL_SetSwapInterval(1);

            InitializeOpenGLBindings();

            return glContext;
        }

        private void InitializeOpenGLBindings()
        {
            IBindingsContext context = new SdlBindingsContext();
            GL.LoadBindings(context);

            Debug.Log("OpenGL bindings successfully initialized.");
        }

        public OpenGLHandle Create2DGLBindings(string vertShaderSrc, string fragShaderSrc)
        {
            var shaderProgram = GLHelper.CreateShaderProgram(vertShaderSrc, fragShaderSrc);
            // Vertex and Index data
            float[] vertices =
            {
                0.0f, 0.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };

            int[] indices =
            {
                0, 1, 2,
                2, 3, 0
            };

            // Set up VAO, VBO, and EBO
            var vao = GL.GenVertexArray();
            var vbo = GL.GenBuffer();
            var ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices,
                BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices,
                BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out int vertexBufferSize);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize,
                out int indexBufferSize);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);

            var mandatoryHandle = new OpenGLMandatoryHandles(vao, vbo, ebo, shaderProgram);
            m_glHandle2D = new OpenGLHandle(mandatoryHandle);
            return m_glHandle2D;
        }
        
        public OpenGLHandle CreateImGuiGLBindings(string vertShaderSrc, string fragShaderSrc)
        {
            var shaderProgram = GLHelper.CreateShaderProgram(vertShaderSrc, fragShaderSrc);
            var attribLocationTex = GL.GetUniformLocation(shaderProgram, "Texture");
            var attribLocationProjMtx = GL.GetUniformLocation(shaderProgram, "ProjMtx");
            var attribLocationPosition = GL.GetAttribLocation(shaderProgram, "Position");
            var attribLocationUV = GL.GetAttribLocation(shaderProgram, "UV");
            var attribLocationColor = GL.GetAttribLocation(shaderProgram, "Color");

            int projLocation = GL.GetUniformLocation(shaderProgram, "ProjMtx");

            var vao = GL.GenVertexArray();
            var vbo = GL.GenBuffer();
            var ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)0, IntPtr.Zero, BufferUsageHint.StreamDraw);

            Debug.Log("GL Vao/Vbo created.");

            GL.EnableVertexAttribArray(attribLocationPosition);
            GL.VertexAttribPointer(attribLocationPosition,
                2, VertexAttribPointerType.Float,
                false, 20, (IntPtr)0);

            GL.EnableVertexAttribArray(attribLocationUV);
            GL.VertexAttribPointer(attribLocationUV,
                2, VertexAttribPointerType.Float,
                false, 20, (IntPtr)8);

            GL.EnableVertexAttribArray(attribLocationColor);
            GL.VertexAttribPointer(attribLocationColor,
                4, VertexAttribPointerType.UnsignedByte,
                true, 20, (IntPtr)16);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)0, IntPtr.Zero, BufferUsageHint.StreamDraw);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            var mandatoryHandles =
                new OpenGLMandatoryHandles(vao, vbo, ebo, shaderProgram);
            var attributeLocations = new OpenGLAttributeLocations(attribLocationTex, attribLocationProjMtx,
                attribLocationPosition, attribLocationUV, attribLocationColor, projLocation);
            m_glHandleGui = new OpenGLHandle(mandatoryHandles, attributeLocations);

            return m_glHandleGui;
        }
        
        private void PrintRenderDriver(IntPtr renderer)
        {
            SDL.SDL_RendererInfo rendererInfo;
            if (SDL.SDL_GetRendererInfo(renderer, out rendererInfo) == 0)
            {
                var driver = Marshal.PtrToStringAnsi(rendererInfo.name).ToUpper();
                m_sysInfo.SetInfoCurrentDriver(driver);
                Debug.Log($"SDL Driver Set - <color=green>[{driver}]</color>]");
            }
            else
            {
                Debug.LogError("Failed to get renderer information: " + SDL.SDL_GetError());
            }
        }

        private void LogAvailableRenderDrivers()
        {
            var numDrivers = SDL.SDL_GetNumRenderDrivers();
            if (numDrivers < 1)
            {
                Debug.LogError("No SDL rendering drivers available!");
                return;
            }

            var availableDrivers = new string[numDrivers];
            var driverText = new StringBuilder();
            driverText.Append($"SDL Drivers ({numDrivers})");
            for (int i = 0; i < numDrivers; i++)
            {
                SDL.SDL_RendererInfo rendererInfo;
                if (SDL.SDL_GetRenderDriverInfo(i, out rendererInfo) == 0)
                {
                    var driverName = Marshal.PtrToStringAnsi(rendererInfo.name).ToUpper();
                    availableDrivers[i] = driverName;
                    driverText.Append($"    <color=yellow>{i}# [{driverName}]</color>");
                }
                else
                {
                    Debug.LogError($"Failed to get renderer info for driver index {i}: {SDL.SDL_GetError()}");
                }
            }

            m_sysInfo.SetInfoCurrentAvailableDrivers(availableDrivers);
            Debug.Log($"{driverText}");

        }

        private class SdlBindingsContext : IBindingsContext
        {
            public IntPtr GetProcAddress(string procName)
            {
                return SDL.SDL_GL_GetProcAddress(procName);
            }
        }
    }
}