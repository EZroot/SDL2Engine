using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SDL2;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Rendering
{
    internal class RenderService : IRenderService
    {
        private readonly ISysInfo m_sysInfo;
        private readonly IWindowConfig m_windowConfig; // Replace with a renderer config if needed

        private IntPtr m_render;
        private OpenGLHandle m_glHandleGui;
        private OpenGLHandle m_glHandle2D;
        private OpenGLHandle m_glHandle3D;
        private OpenGLHandle m_glHandleDebug;

        private int m_fbo2D;
        private int m_fboTexture2D;
        private int m_fboWidth;
        private int m_fboHeight;

        private List<(Vector2 Start, Vector2 End, Color4 Color)> m_debugLines = new List<(Vector2, Vector2, Color4)>();
        private List<(Vector2 TopLeft, Vector2 BottomRight, Color4 Color)> m_debugRects = new List<(Vector2, Vector2, Color4)>();

        // Fields for screen quad rendering
        private int m_screenQuadShader;
        private int m_screenQuadVao;

        public IntPtr RenderPtr => m_render;
        public OpenGLHandle OpenGLHandleGui => m_glHandleGui;
        public OpenGLHandle OpenGLHandle2D => m_glHandle2D;
        public OpenGLHandle OpenGLHandle3D => m_glHandle3D;

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
                var renderer = CreateOpenGLContext(window);
                CreateImGuiGLBindings(
                    FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/imgui/imguishader.vert"),
                    FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/imgui/imguishader.frag"));
                Create2DGLBindings(
                    FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/2d/2dshader.vert"),
                    FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/2d/2dshader.frag"));
                Create3DGLBindings(
                    FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/3d.vert"),
                    FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/3d.frag"), 1f);
                CreateDebugGLBindings(
                    FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/debugging/debug.vert"),
                    FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/debugging/debug.frag"));

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                // Initialize framebuffer
                SDL.SDL_GetWindowSize(window, out int windowWidth, out int windowHeight);
                InitializeFrameBuffer(windowWidth, windowHeight);

                return renderer;
            }

            // Assume default is SDL
            return CreateRendererSDL(window, renderFlags);
        }

        /// <summary>
        /// Initializes the framebuffer for 2D rendering
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void InitializeFrameBuffer(int width, int height)
        {
            m_fboWidth = width;
            m_fboHeight = height;

            // Generate framebuffer
            m_fbo2D = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_fbo2D);

            // Create texture to attach to framebuffer
            m_fboTexture2D = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, m_fboTexture2D);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, m_fboWidth, m_fboHeight, 0,
                          PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // Attach texture to framebuffer
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                                    TextureTarget.Texture2D, m_fboTexture2D, 0);

            // Create and attach a renderbuffer for depth and stencil (optional, if needed)
            int rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, m_fboWidth, m_fboHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
                                       RenderbufferTarget.Renderbuffer, rbo);

            // Check framebuffer completeness
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer is not complete!");
            }

            // Unbind framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            Debug.Log("Framebuffer for 2D rendering initialized.");
        }

        /// <summary>
        /// Handles window resize events to adjust framebuffer size
        /// </summary>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        public void OnWindowResize(int newWidth, int newHeight)
        {
            m_fboWidth = newWidth;
            m_fboHeight = newHeight;

            // Resize framebuffer texture
            GL.BindTexture(TextureTarget.Texture2D, m_fboTexture2D);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, m_fboWidth, m_fboHeight, 0,
                          PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            Debug.Log($"Framebuffer resized to {m_fboWidth}x{m_fboHeight}.");
        }

        /// <summary>
        /// The main render loop handling framebuffer operations
        /// </summary>
        /// <param name="projection"></param>
        public void Render(Matrix4 projection)
        {
            // 1. Bind framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_fbo2D);
            GL.Viewport(0, 0, m_fboWidth, m_fboHeight);

            // 2. Clear framebuffer
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            // 3. Render 2D content
            Render2DContent(projection);

            // 4. Render debug primitives
            RenderDebugPrimitives(projection);

            // 5. Unbind framebuffer to render to screen
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            // GL.Viewport(0, 0, m_windowConfig.Settings.Width, m_windowConfig.Settings.Height); // Assuming m_sysInfo has window dimensions

            // 7. Render framebuffer texture to screen
            RenderFramebufferTextureToScreen();
        }

        private void Render2DContent(Matrix4 projection)
        {
            // Bind the 2D shader program
            GL.UseProgram(m_glHandle2D.Handles.Shader);

            // Set projection matrix uniform
            int projLocation = GL.GetUniformLocation(m_glHandle2D.Handles.Shader, "uProjection");
            GL.UniformMatrix4(projLocation, false, ref projection);

            // Bind VAO and render
            GL.BindVertexArray(m_glHandle2D.Handles.Vao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            // Cleanup
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        private void RenderFramebufferTextureToScreen()
        {
            // Initialize screen quad shader and VAO if not already done
            if (m_screenQuadShader == 0)
            {
                InitializeScreenQuadShader();
            }

            // Use screen quad shader
            GL.UseProgram(m_screenQuadShader);

            // Bind the framebuffer texture
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, m_fboTexture2D);
            GL.Uniform1(GL.GetUniformLocation(m_screenQuadShader, "screenTexture"), 0);

            // Bind VAO for screen quad and render
            GL.BindVertexArray(m_screenQuadVao);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            // Cleanup
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);
        }

        private void InitializeScreenQuadShader()
        {
            string vertexShaderSrc = @"
                #version 330 core
                layout(location = 0) in vec2 aPos;
                layout(location = 1) in vec2 aTexCoords;

                out vec2 TexCoords;

                void main()
                {
                    TexCoords = aTexCoords;
                    gl_Position = vec4(aPos, 0.0, 1.0);
                }";

            string fragmentShaderSrc = @"
                #version 330 core
                out vec4 FragColor;

                in vec2 TexCoords;

                uniform sampler2D screenTexture;

                void main()
                {
                    FragColor = texture(screenTexture, TexCoords);
                }";

            m_screenQuadShader = GLHelper.CreateShaderProgram(vertexShaderSrc, fragmentShaderSrc);

            float[] quadVertices = {
                // positions   // texCoords
                -1.0f,  1.0f,  0.0f, 1.0f,
                -1.0f, -1.0f,  0.0f, 0.0f,
                 1.0f,  1.0f,  1.0f, 1.0f,
                 1.0f, -1.0f,  1.0f, 0.0f,
            };

            m_screenQuadVao = GL.GenVertexArray();
            int screenQuadVbo = GL.GenBuffer();
            GL.BindVertexArray(m_screenQuadVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, screenQuadVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            Debug.Log("Screen Quad Shader and VAO initialized.");
        }

        public void DrawLine(Vector2 start, Vector2 end, Color4 color)
        {
            m_debugLines.Add((start, end, color));
        }

        public void DrawRect(Vector2 topLeft, Vector2 bottomRight, Color4 color)
        {
            m_debugRects.Add((topLeft, bottomRight, color));
        }

        public void RenderDebugPrimitives(Matrix4 projection)
        {
            if (m_glHandleDebug == null)
                return;

            GL.UseProgram(m_glHandleDebug.Handles.Shader);

            // Set projection matrix uniform
            int projLocation = GL.GetUniformLocation(m_glHandleDebug.Handles.Shader, "uProjection");
            GL.UniformMatrix4(projLocation, false, ref projection);

            GL.BindVertexArray(m_glHandleDebug.Handles.Vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_glHandleDebug.Handles.Vbo);

            List<float> vertices = new List<float>();

            // Add lines
            foreach (var line in m_debugLines)
            {
                vertices.Add(line.Start.X);
                vertices.Add(line.Start.Y);
                vertices.Add(line.Color.R);
                vertices.Add(line.Color.G);
                vertices.Add(line.Color.B);
                vertices.Add(line.Color.A);

                vertices.Add(line.End.X);
                vertices.Add(line.End.Y);
                vertices.Add(line.Color.R);
                vertices.Add(line.Color.G);
                vertices.Add(line.Color.B);
                vertices.Add(line.Color.A);
            }

            // Add rectangles as lines
            foreach (var rect in m_debugRects)
            {
                Vector2 topRight = new Vector2(rect.BottomRight.X, rect.TopLeft.Y);
                Vector2 bottomLeft = new Vector2(rect.TopLeft.X, rect.BottomRight.Y);

                // Top
                vertices.Add(rect.TopLeft.X);
                vertices.Add(rect.TopLeft.Y);
                vertices.Add(rect.Color.R);
                vertices.Add(rect.Color.G);
                vertices.Add(rect.Color.B);
                vertices.Add(rect.Color.A);

                vertices.Add(topRight.X);
                vertices.Add(topRight.Y);
                vertices.Add(rect.Color.R);
                vertices.Add(rect.Color.G);
                vertices.Add(rect.Color.B);
                vertices.Add(rect.Color.A);

                // Right
                vertices.Add(topRight.X);
                vertices.Add(topRight.Y);
                vertices.Add(rect.Color.R);
                vertices.Add(rect.Color.G);
                vertices.Add(rect.Color.B);
                vertices.Add(rect.Color.A);

                vertices.Add(rect.BottomRight.X);
                vertices.Add(rect.BottomRight.Y);
                vertices.Add(rect.Color.R);
                vertices.Add(rect.Color.G);
                vertices.Add(rect.Color.B);
                vertices.Add(rect.Color.A);

                // Bottom
                vertices.Add(rect.BottomRight.X);
                vertices.Add(rect.BottomRight.Y);
                vertices.Add(rect.Color.R);
                vertices.Add(rect.Color.G);
                vertices.Add(rect.Color.B);
                vertices.Add(rect.Color.A);

                vertices.Add(bottomLeft.X);
                vertices.Add(bottomLeft.Y);
                vertices.Add(rect.Color.R);
                vertices.Add(rect.Color.G);
                vertices.Add(rect.Color.B);
                vertices.Add(rect.Color.A);

                // Left
                vertices.Add(bottomLeft.X);
                vertices.Add(bottomLeft.Y);
                vertices.Add(rect.Color.R);
                vertices.Add(rect.Color.G);
                vertices.Add(rect.Color.B);
                vertices.Add(rect.Color.A);

                vertices.Add(rect.TopLeft.X);
                vertices.Add(rect.TopLeft.Y);
                vertices.Add(rect.Color.R);
                vertices.Add(rect.Color.G);
                vertices.Add(rect.Color.B);
                vertices.Add(rect.Color.A);
            }

            // Update buffer data
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_glHandleDebug.Handles.Vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Count * sizeof(float), vertices.ToArray());

            // Draw lines
            GL.DrawArrays(PrimitiveType.Lines, 0, vertices.Count / 6);

            // Cleanup
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Clear debug primitives after rendering
            m_debugLines.Clear();
            m_debugRects.Clear();
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

            // Enable vsync
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
                // positions        // texCoords
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

        public OpenGLHandle Create3DGLBindings(string vertShaderSrc, string fragShaderSrc, float aspect)
        {
            var shaderProgram = GLHelper.CreateShaderProgram(vertShaderSrc, fragShaderSrc);

            // Full cube vertex data (36 vertices: 6 faces * 2 triangles * 3 vertices)
            float[] vertices =
            {
                // Front face
                -0.5f, -0.5f, 0.5f, 0.0f, 0.0f,
                0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
                0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
                0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
                -0.5f, 0.5f, 0.5f, 0.0f, 1.0f,
                -0.5f, -0.5f, 0.5f, 0.0f, 0.0f,

                // Back face
                -0.5f, -0.5f, -0.5f, 1.0f, 0.0f,
                0.5f, -0.5f, -0.5f, 0.0f, 0.0f,
                0.5f, 0.5f, -0.5f, 0.0f, 1.0f,
                0.5f, 0.5f, -0.5f, 0.0f, 1.0f,
                -0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
                -0.5f, -0.5f, -0.5f, 1.0f, 0.0f,

                // Left face
                -0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
                -0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
                -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
                -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
                -0.5f, -0.5f, 0.5f, 0.0f, 0.0f,
                -0.5f, 0.5f, 0.5f, 1.0f, 0.0f,

                // Right face
                0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
                0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
                0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
                0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
                0.5f, -0.5f, 0.5f, 0.0f, 0.0f,
                0.5f, 0.5f, 0.5f, 1.0f, 0.0f,

                // Top face
                -0.5f, 0.5f, -0.5f, 0.0f, 1.0f,
                0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
                0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
                0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
                -0.5f, 0.5f, 0.5f, 0.0f, 0.0f,
                -0.5f, 0.5f, -0.5f, 0.0f, 1.0f,

                // Bottom face
                -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
                0.5f, -0.5f, -0.5f, 1.0f, 1.0f,
                0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
                0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
                -0.5f, -0.5f, 0.5f, 0.0f, 0.0f,
                -0.5f, -0.5f, -0.5f, 0.0f, 1.0f
            };

            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices,
                BufferUsageHint.StaticDraw);

            // Position attribute
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            // Texture coordinate attribute
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.BindVertexArray(0);

            // Set up transformation matrices
            Matrix4 model = Matrix4.Identity;
            Matrix4 view = Matrix4.CreateTranslation(0f, 0f, -3f);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f), aspect, 0.1f, 100f);

            GL.UseProgram(shaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);

            m_glHandle3D = new OpenGLHandle(new OpenGLMandatoryHandles(vao, vbo, 0, shaderProgram));
            return m_glHandle3D;
        }

        public OpenGLHandle CreateDebugGLBindings(string vertShaderSrc, string fragShaderSrc)
        {
            var shaderProgram = GLHelper.CreateShaderProgram(vertShaderSrc, fragShaderSrc);

            var vao = GL.GenVertexArray();
            var vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            // Allocate initial buffer size, can be resized dynamically
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 6 * 1000, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // Position attribute
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            // Color attribute
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, 6 * sizeof(float), 2 * sizeof(float));

            GL.BindVertexArray(0);

            var mandatoryHandles = new OpenGLMandatoryHandles(vao, vbo, 0, shaderProgram);
            m_glHandleDebug = new OpenGLHandle(mandatoryHandles);
            return m_glHandleDebug;
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
