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

        private OpenGLHandle m_glHandle;
        private IntPtr m_render;
        public IntPtr RenderPtr => m_render;
        public OpenGLHandle OpenGLHandle => m_glHandle;

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

        public OpenGLHandle CreateOpenGLDeviceObjects()
        {
            // 1. Create a simple ImGui shader program
            //    (Here you’d compile your own vertex/fragment shader & link them)
            var g_ShaderHandle = GL.CreateProgram();
            int vert = CompileShader(ShaderType.VertexShader, MyImGuiVertexShaderSrc);
            int frag = CompileShader(ShaderType.FragmentShader, MyImGuiFragmentShaderSrc);
            GL.AttachShader(g_ShaderHandle, vert);
            GL.AttachShader(g_ShaderHandle, frag);
            GL.LinkProgram(g_ShaderHandle);
            GL.DetachShader(g_ShaderHandle, vert);
            GL.DetachShader(g_ShaderHandle, frag);
            GL.DeleteShader(vert);
            GL.DeleteShader(frag);
            Debug.Log("Shaders finished compiling.");
            // 2. Get uniform/attribute locations
            var g_AttribLocationTex = GL.GetUniformLocation(g_ShaderHandle, "Texture");
            var g_AttribLocationProjMtx = GL.GetUniformLocation(g_ShaderHandle, "ProjMtx");
            var g_AttribLocationPosition = GL.GetAttribLocation(g_ShaderHandle, "Position");
            var g_AttribLocationUV = GL.GetAttribLocation(g_ShaderHandle, "UV");
            var g_AttribLocationColor = GL.GetAttribLocation(g_ShaderHandle, "Color");

            // 3. Create buffers/arrays
            var g_VaoHandle = GL.GenVertexArray();
            var g_VboHandle = GL.GenBuffer();
            var g_ElementsHandle = GL.GenBuffer();

            GL.BindVertexArray(g_VaoHandle);

            // 4. Setup VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, g_VboHandle);
            // Just allocate empty for now
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)0, IntPtr.Zero, BufferUsageHint.StreamDraw);

            Debug.Log("GL Vao/Vbo created.");

            // 5. Setup attributes to match ImDrawVert layout:
            //    struct ImDrawVert { ImVec2 pos; ImVec2 uv; ImU32 col; }
            //    Typically 20 bytes per vertex: (2 floats + 2 floats + 4 bytes color)
            GL.EnableVertexAttribArray(g_AttribLocationPosition);
            GL.VertexAttribPointer(g_AttribLocationPosition,
                2, VertexAttribPointerType.Float,
                false, 20, (IntPtr)0);

            GL.EnableVertexAttribArray(g_AttribLocationUV);
            GL.VertexAttribPointer(g_AttribLocationUV,
                2, VertexAttribPointerType.Float,
                false, 20, (IntPtr)8);

            GL.EnableVertexAttribArray(g_AttribLocationColor);
            // Note the 'true' in VertexAttribPointer means normalized for unsigned bytes
            GL.VertexAttribPointer(g_AttribLocationColor,
                4, VertexAttribPointerType.UnsignedByte,
                true, 20, (IntPtr)16);

            // 6. Setup IBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, g_ElementsHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)0, IntPtr.Zero, BufferUsageHint.StreamDraw);

            // Unbind
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            m_glHandle = new OpenGLHandle(g_VaoHandle, g_VboHandle, g_ElementsHandle, g_ShaderHandle,
                g_AttribLocationTex, g_AttribLocationProjMtx, g_AttribLocationPosition, g_AttribLocationUV,
                g_AttribLocationColor);

            return m_glHandle;
        }

// Minimal naive shader strings
        private const string MyImGuiVertexShaderSrc = @"
#version 330 core
layout (location = 0) in vec2 Position;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec4 Color;

uniform mat4 ProjMtx;

out vec2 Frag_UV;
out vec4 Frag_Color;

void main()
{
    Frag_UV = UV;
    Frag_Color = Color;
    gl_Position = ProjMtx * vec4(Position, 0, 1);
}";

        private const string MyImGuiFragmentShaderSrc = @"
#version 330 core
in vec2 Frag_UV;
in vec4 Frag_Color;

uniform sampler2D Texture;

out vec4 Out_Color;

void main()
{
    Out_Color = Frag_Color * texture(Texture, Frag_UV);
}";

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