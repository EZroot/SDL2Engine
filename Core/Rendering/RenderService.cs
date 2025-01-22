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


        public IntPtr CreateRendererSDL(IntPtr window, SDL.SDL_RendererFlags renderFlags =
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

        public nint CreateOpenGLContext(nint window)
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

public OpenGLHandle Create2DImageOpenGLDeviceObjects(string vertShaderSrc, string fragShaderSrc)
{
    var g_ShaderHandle = GL.CreateProgram();
    int vert = CompileShader(ShaderType.VertexShader, vertShaderSrc);
    int frag = CompileShader(ShaderType.FragmentShader, fragShaderSrc);
    GL.AttachShader(g_ShaderHandle, vert);
    GL.AttachShader(g_ShaderHandle, frag);
    GL.LinkProgram(g_ShaderHandle);

    GL.GetProgram(g_ShaderHandle, GetProgramParameterName.LinkStatus, out int linkStatus);
    if (linkStatus == 0)
    {
        string infoLog = GL.GetProgramInfoLog(g_ShaderHandle);
        throw new Exception($"Shader program linking failed: {infoLog}");
    }

    GL.DetachShader(g_ShaderHandle, vert);
    GL.DetachShader(g_ShaderHandle, frag);
    GL.DeleteShader(vert);
    GL.DeleteShader(frag);

    // Vertex and Index data
    float[] vertices = {
        0.0f,  0.0f, 0.0f,  0.0f, 1.0f, // Bottom-left
        32.0f, 0.0f, 0.0f,  1.0f, 1.0f, // Bottom-right
        32.0f, 32.0f, 0.0f,  1.0f, 0.0f, // Top-right
        0.0f,  32.0f, 0.0f,  0.0f, 0.0f  // Top-left
    };

    int[] indices = {
        0, 1, 2, // First triangle
        2, 3, 0  // Second triangle
    };

    // Set up VAO, VBO, and EBO
    var vao = GL.GenVertexArray();
    var vbo = GL.GenBuffer();
    var ebo = GL.GenBuffer();

    GL.BindVertexArray(vao);

    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
    GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
    GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out int vertexBufferSize);
    GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out int indexBufferSize);

    GL.EnableVertexAttribArray(0); // Position
    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
    GL.EnableVertexAttribArray(1); // Texture Coordinates
    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

    GL.BindVertexArray(0);

    m_glHandle2D = new OpenGLHandle(vao, vbo, ebo, g_ShaderHandle);
    return m_glHandle2D;
}

        public OpenGLHandle CreateOpenGLDeviceObjects(string vertShaderSrc, string fragShaderSrc)
        {
            var g_ShaderHandle = GL.CreateProgram();
            int vert = CompileShader(ShaderType.VertexShader, vertShaderSrc);
            int frag = CompileShader(ShaderType.FragmentShader, fragShaderSrc);
            GL.AttachShader(g_ShaderHandle, vert);
            GL.AttachShader(g_ShaderHandle, frag);
            GL.LinkProgram(g_ShaderHandle);
            GL.GetProgram(g_ShaderHandle, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                string infoLog = GL.GetProgramInfoLog(g_ShaderHandle);
                throw new Exception($"Shader program linking failed: {infoLog}");
            }

            GL.DetachShader(g_ShaderHandle, vert);
            GL.DetachShader(g_ShaderHandle, frag);
            GL.DeleteShader(vert);
            GL.DeleteShader(frag);
            Debug.Log("Shaders finished compiling.");
            var g_AttribLocationTex = GL.GetUniformLocation(g_ShaderHandle, "Texture");
            var g_AttribLocationProjMtx = GL.GetUniformLocation(g_ShaderHandle, "ProjMtx");
            var g_AttribLocationPosition = GL.GetAttribLocation(g_ShaderHandle, "Position");
            var g_AttribLocationUV = GL.GetAttribLocation(g_ShaderHandle, "UV");
            var g_AttribLocationColor = GL.GetAttribLocation(g_ShaderHandle, "Color");

            int projLocation = GL.GetUniformLocation(g_ShaderHandle, "ProjMtx");

            var g_VaoHandle = GL.GenVertexArray();
            var g_VboHandle = GL.GenBuffer();
            var g_ElementsHandle = GL.GenBuffer();

            GL.BindVertexArray(g_VaoHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, g_VboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)0, IntPtr.Zero, BufferUsageHint.StreamDraw);

            Debug.Log("GL Vao/Vbo created.");

            GL.EnableVertexAttribArray(g_AttribLocationPosition);
            GL.VertexAttribPointer(g_AttribLocationPosition,
                2, VertexAttribPointerType.Float,
                false, 20, (IntPtr)0);

            GL.EnableVertexAttribArray(g_AttribLocationUV);
            GL.VertexAttribPointer(g_AttribLocationUV,
                2, VertexAttribPointerType.Float,
                false, 20, (IntPtr)8);

            GL.EnableVertexAttribArray(g_AttribLocationColor);
            GL.VertexAttribPointer(g_AttribLocationColor,
                4, VertexAttribPointerType.UnsignedByte,
                true, 20, (IntPtr)16);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, g_ElementsHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)0, IntPtr.Zero, BufferUsageHint.StreamDraw);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            m_glHandleGui = new OpenGLHandle(
                g_VaoHandle, g_VboHandle, g_ElementsHandle, g_ShaderHandle,
                g_AttribLocationTex, g_AttribLocationProjMtx, g_AttribLocationPosition, g_AttribLocationUV,
                g_AttribLocationColor, projLocation);

            return m_glHandleGui;
        }

        private int CompileShader(ShaderType type, string src)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, src);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Compile error ({type}): {infoLog}");
            }

            return shader;
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