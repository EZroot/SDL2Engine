using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Geometry
{
    public class Shader
    {
        public int ProgramId { get; }
        private readonly Dictionary<string, int> _uniformLocations = new Dictionary<string, int>();

        public Shader(string vertPath, string fragPath)
        {
            string vertSrc = FileHelper.ReadFileContents(vertPath);
            string fragSrc = FileHelper.ReadFileContents(fragPath);
            ProgramId = GLHelper.CreateShaderProgram(vertSrc, fragSrc);
        }

        /// <summary>
        /// Binds the shader program for use.
        /// </summary>
        public void Bind()
        {
            GL.UseProgram(ProgramId);
        }

        /// <summary>
        /// Unbinds any shader program.
        /// </summary>
        public void UnBind()
        {
            GL.UseProgram(0);
        }

        /// <summary>
        /// Caches the locations of the specified uniforms. Call this once after linking your shader.
        /// </summary>
        /// <param name="uniformNames">Names of the uniforms to cache.</param>
        public void CacheUniforms(params string[] uniformNames)
        {
            foreach (var name in uniformNames)
            {
                int loc = GL.GetUniformLocation(ProgramId, name);
                if (loc == -1)
                {
                    Console.WriteLine($"Warning: Uniform '{name}' not found in shader.");
                }
                _uniformLocations[name] = loc;
            }
        }

        /// <summary>
        /// Gets the cached location for a uniform.
        /// </summary>
        public int GetUniformLocation(string uniformName)
        {
            return _uniformLocations.TryGetValue(uniformName, out var loc) ? loc : -1;
        }

        public void SetUniformMatrix4(string uniformName, bool transpose, ref Matrix4 matrix)
        {
            int loc = GetUniformLocation(uniformName);
            if (loc != -1)
            {
                GL.UniformMatrix4(loc, transpose, ref matrix);
            }
        }

        public void SetUniform3(string uniformName, ref Vector3 vector)
        {
            int loc = GetUniformLocation(uniformName);
            if (loc != -1)
            {
                GL.Uniform3(loc, vector);
            }
        }

        public void SetUniform1(string uniformName, int value)
        {
            int loc = GetUniformLocation(uniformName);
            if (loc != -1)
            {
                GL.Uniform1(loc, value);
            }
        }
        
        private void CheckForGLErrors()
        {
            ErrorCode errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                // Handle the error appropriately
                // For example, you could throw an exception or log the error
                throw new Exception($"OpenGL Error: {errorCode}");
            }
        }
    }
}
