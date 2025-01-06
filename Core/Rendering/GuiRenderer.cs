using System;
using System.Numerics; // For ImGui
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics; // For OpenGL matrices
using SDL2;
using SDL2Engine.Core.Utils;

public class ImGuiRenderer : IDisposable
{
    private IntPtr _window;
    private int _width;
    private int _height;

    // OpenGL handles
    private int _vertexArray;
    private int _vertexBuffer;
    private int _indexBuffer;
    private int _shaderProgram;
    private int _fontTexture;

    private bool disposed = false;

    public ImGuiRenderer(IntPtr window, int width, int height)
    {
        _window = window;
        _width = width;
        _height = height;

        // Initialize OpenGL resources
        CreateDeviceObjects();
        SetupImGuiStyle();
    }

    private void CreateDeviceObjects()
    {
        // Create and compile shaders
        string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec2 Position;
            layout(location = 1) in vec2 UV;
            layout(location = 2) in vec4 Color;
            
            uniform mat4 projection;
            
            out vec2 Frag_UV;
            out vec4 Frag_Color;
            
            void main()
            {
                Frag_UV = UV;
                Frag_Color = Color;
                gl_Position = projection * vec4(Position.xy, 0.0, 1.0);
            }
        ";

        string fragmentShaderSource = @"
            #version 330 core
            in vec2 Frag_UV;
            in vec4 Frag_Color;
            
            uniform sampler2D Texture;
            
            out vec4 Out_Color;
            
            void main()
            {
                Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
            }
        ";

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);
        CheckShaderCompilation(vertexShader, "Vertex Shader");

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);
        CheckShaderCompilation(fragmentShader, "Fragment Shader");

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);
        CheckProgramLinking(_shaderProgram);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        // Create buffers
        _vertexArray = GL.GenVertexArray();
        _vertexBuffer = GL.GenBuffer();
        _indexBuffer = GL.GenBuffer();

        GL.BindVertexArray(_vertexArray);

        // Vertex buffer
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, 10000 * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

        // Index buffer
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, 2000 * sizeof(ushort), IntPtr.Zero, BufferUsageHint.DynamicDraw);

        // Vertex attributes
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 16, 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 16, 8);

        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 16, 16);

        // Unbind
        GL.BindVertexArray(0);

        // Setup projection matrix
        SetProjection(_width, _height);

        // Load font texture
        CreateFontTexture();
    }

    private void SetProjection(int width, int height)
    {
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0.0f, width, height, 0.0f, -1.0f, 1.0f);
        GL.UseProgram(_shaderProgram);
        int projectionLocation = GL.GetUniformLocation(_shaderProgram, "projection");
        GL.UniformMatrix4(projectionLocation, false, ref projection);
    }

    private void CreateFontTexture()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

        _fontTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        io.Fonts.SetTexID((IntPtr)_fontTexture);
        io.Fonts.ClearTexData();

        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    private void SetupImGuiStyle()
    {
        // Customize ImGui style if desired
        ImGui.StyleColorsDark();
    }

    public void NewFrame()
    {
        ImGuiIOPtr io = ImGui.GetIO();

        // Set DisplaySize to match the window dimensions
        io.DisplaySize = new System.Numerics.Vector2(_width, _height);

        // Start a new ImGui frame
        ImGui.NewFrame();
    }

    public void RenderDrawData(ImDrawDataPtr drawData)
    {
        if (drawData.CmdListsCount == 0)
            return;

        // Calculate the scale for high-DPI displays
        float scaleFactor = 1.0f;
        ImGuiIOPtr io = ImGui.GetIO();
        scaleFactor = io.DisplayFramebufferScale.X;

        // Setup orthographic projection
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.ScissorTest);

        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vertexArray);
        GL.BindTexture(TextureTarget.Texture2D, _fontTexture);

        // Iterate through each command list
        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            ImDrawListPtr cmdList = drawData.CmdLists[n];
            for (int cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
            {
                ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmdi];
                if (pcmd.UserCallback != IntPtr.Zero)
                {
                    // Handle user callbacks if necessary
                    throw new NotImplementedException();
                }
                else
                {
                    GL.Scissor(
                        (int)(pcmd.ClipRect.X * scaleFactor),
                        (int)((_height - pcmd.ClipRect.W) * scaleFactor),
                        (int)((pcmd.ClipRect.Z - pcmd.ClipRect.X) * scaleFactor),
                        (int)((pcmd.ClipRect.W - pcmd.ClipRect.Y) * scaleFactor)
                    );
                    GL.DrawElementsBaseVertex(
                        PrimitiveType.Triangles,
                        (int)pcmd.ElemCount, // Cast from uint to int
                        DrawElementsType.UnsignedShort,
                        (IntPtr)(pcmd.IdxOffset * sizeof(ushort)),
                        (int)pcmd.VtxOffset // Cast from uint to int
                    );
                }
            }
        }

        // Cleanup
        GL.Disable(EnableCap.Blend);
        GL.Disable(EnableCap.ScissorTest);
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }

    public void ProcessEvent(SDL.SDL_Event e)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        switch (e.type)
        {
            case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                io.MouseWheel += e.wheel.y;
                break;
            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                int button = e.button.button;
                bool isDown = e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN;
                if (button == SDL.SDL_BUTTON_LEFT) io.MouseDown[0] = isDown;
                if (button == SDL.SDL_BUTTON_RIGHT) io.MouseDown[1] = isDown;
                if (button == SDL.SDL_BUTTON_MIDDLE) io.MouseDown[2] = isDown;
                break;
            case SDL.SDL_EventType.SDL_TEXTINPUT:
                unsafe
                {
                    byte* textPtr = e.text.text;
                    string text = System.Text.Encoding.UTF8.GetString(textPtr, 32).TrimEnd('\0');
                    io.AddInputCharactersUTF8(text);
                }
                break;
            case SDL.SDL_EventType.SDL_KEYDOWN:
            case SDL.SDL_EventType.SDL_KEYUP:
                int keyIndex = (int)e.key.keysym.sym;
                bool down = e.type == SDL.SDL_EventType.SDL_KEYDOWN;

                // ✅ Ensure keyIndex is within bounds
                if (keyIndex >= 0 && keyIndex < io.KeysData.Count)
                {
                    ImGuiKeyData keyData = io.KeysData[keyIndex];
                    keyData.Down = (byte)(down ? 1 : 0);
                    keyData.DownDuration = down ? 0 : -1;
                    io.KeysData[keyIndex] = keyData;
                }

                io.KeyCtrl = (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_CTRL) != 0;
                io.KeyShift = (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_SHIFT) != 0;
                io.KeyAlt = (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_ALT) != 0;
                io.KeySuper = (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_GUI) != 0;
                break;
        }

        // Update mouse position
        int mouseX, mouseY;
        uint mouseState = SDL.SDL_GetMouseState(out mouseX, out mouseY);
        io.MousePos = new System.Numerics.Vector2(mouseX, mouseY);

        // Update mouse buttons
        io.MouseDown[0] = (mouseState & SDL.SDL_BUTTON(SDL.SDL_BUTTON_LEFT)) != 0;
        io.MouseDown[1] = (mouseState & SDL.SDL_BUTTON(SDL.SDL_BUTTON_RIGHT)) != 0;
        io.MouseDown[2] = (mouseState & SDL.SDL_BUTTON(SDL.SDL_BUTTON_MIDDLE)) != 0;
    }

    private void CheckShaderCompilation(int shader, string name)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == (int)All.False)
        {
            string info = GL.GetShaderInfoLog(shader);
            Debug.LogError($"Error compiling {name}: {info}");
            throw new InvalidOperationException($"Error compiling {name}: {info}");
        }
    }

    private void CheckProgramLinking(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
        if (status == (int)All.False)
        {
            string info = GL.GetProgramInfoLog(program);
            Debug.LogError($"Error linking program: {info}");
            throw new InvalidOperationException($"Error linking program: {info}");
        }
    }

    public void Dispose()
    {
        if (disposed)
            return;

        // Delete OpenGL resources
        if (_fontTexture != 0)
        {
            GL.DeleteTexture(_fontTexture);
            _fontTexture = 0;
        }

        if (_shaderProgram != 0)
        {
            GL.DeleteProgram(_shaderProgram);
            _shaderProgram = 0;
        }

        if (_vertexBuffer != 0)
        {
            GL.DeleteBuffer(_vertexBuffer);
            _vertexBuffer = 0;
        }

        if (_indexBuffer != 0)
        {
            GL.DeleteBuffer(_indexBuffer);
            _indexBuffer = 0;
        }

        if (_vertexArray != 0)
        {
            GL.DeleteVertexArray(_vertexArray);
            _vertexArray = 0;
        }

        disposed = true;
    }
}
