using OpenTK.Graphics.OpenGL4;

namespace SDL2Engine.Core.Utils;

public class GLHelper
{
    /// <summary>
    /// Create, compile, attach, link and return shader program
    /// </summary>
    /// <param name="vertSrc"></param>
    /// <param name="fragSrc"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static int CreateShaderProgram(string vertSrc, string fragSrc)
    {
        var g_ShaderHandle = GL.CreateProgram();
        int vert = CompileShader(ShaderType.VertexShader, vertSrc);
        int frag = CompileShader(ShaderType.FragmentShader, fragSrc);
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
        return g_ShaderHandle;
    }
    
    private static int CompileShader(ShaderType type, string src)
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

}