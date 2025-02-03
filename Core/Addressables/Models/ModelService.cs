using System.Globalization;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SDL2Engine.Core.Addressables.Models.Interfaces;
using SDL2Engine.Core.Utils;

public class ModelService : IModelService
{
    public OpenGLHandle Load3DModel(string path, string vertShaderPath, string fragShaderPath, float aspect)
    {
        string vertShaderSrc = File.ReadAllText(vertShaderPath);
        string fragShaderSrc = File.ReadAllText(fragShaderPath);
        int shaderProgram = GLHelper.CreateShaderProgram(vertShaderSrc, fragShaderSrc);

        // Custom OBJ loader: parse positions, texture coordinates, and normals.
        List<Vector3> positions = new List<Vector3>();
        List<Vector2> texCoords = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<float> finalVertices = new List<float>();

        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("#") || string.IsNullOrWhiteSpace(trimmed))
                continue;

            string[] parts = trimmed.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            switch (parts[0])
            {
                case "v":
                    positions.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)));
                    break;
                case "vt":
                    texCoords.Add(new Vector2(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture)));
                    break;
                case "vn":
                    normals.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)));
                    break;
                case "f":
                    // Triangulate the face (assumes convex polygon)
                    int faceVertexCount = parts.Length - 1;
                    for (int i = 1; i < faceVertexCount - 1; i++)
                    {
                        AppendVertex(parts[1], positions, texCoords, normals, finalVertices);
                        AppendVertex(parts[i + 1], positions, texCoords, normals, finalVertices);
                        AppendVertex(parts[i + 2], positions, texCoords, normals, finalVertices);
                    }

                    break;
            }
        }

        float[] vertices = finalVertices.ToArray();
        if (vertices.Length == 0)
            throw new Exception("No vertices loaded from model file.");

        // Calculate vertex count (each vertex consists of 8 floats).
        int vertexCount = vertices.Length / 8;

        // Create VAO/VBO.
        int vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        GL.BindVertexArray(0);

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Matrix4.CreateTranslation(0f, 0f, -3f);
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), aspect, 0.1f, 100f);
        GL.UseProgram(shaderProgram);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref model);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), false, ref view);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);
        GL.UseProgram(0);

        // Here we assume your OpenGLMandatoryHandles class has been updated to store the vertex count.
        // For example, its constructor might now be:
        //   OpenGLMandatoryHandles(int vao, int vbo, int vertexCount, int shader)
        // and it exposes a VertexCount property.
        return new OpenGLHandle(new OpenGLMandatoryHandles(vao, vbo, 0, shaderProgram, vertexCount));
    }

    public OpenGLHandle LoadQuad(string vertShaderPath, string fragShaderPath, float aspect)
    {
        string vertSrc = File.ReadAllText(vertShaderPath);
        string fragSrc = File.ReadAllText(fragShaderPath);
        int shader = GLHelper.CreateShaderProgram(vertSrc, fragSrc);

        float[] vertices = new float[]
        {
            // positions         // normals      // tex coords
            -1f, 1f, 0f, 0f, 0f, 1f, 0f, 1f,
            -1f, -1f, 0f, 0f, 0f, 1f, 0f, 0f,
            1f, -1f, 0f, 0f, 0f, 1f, 1f, 0f,

            -1f, 1f, 0f, 0f, 0f, 1f, 0f, 1f,
            1f, -1f, 0f, 0f, 0f, 1f, 1f, 0f,
            1f, 1f, 0f, 0f, 0f, 1f, 1f, 1f,
        };

        int vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        GL.BindVertexArray(0);

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Matrix4.CreateTranslation(0f, 0f, -3f);
        Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), aspect, 0.1f, 100f);
        GL.UseProgram(shader);
        GL.UniformMatrix4(GL.GetUniformLocation(shader, "model"), false, ref model);
        GL.UniformMatrix4(GL.GetUniformLocation(shader, "view"), false, ref view);
        GL.UniformMatrix4(GL.GetUniformLocation(shader, "projection"), false, ref proj);
        GL.UseProgram(0);

        int vertexCount = vertices.Length / 8;
        return new OpenGLHandle(new OpenGLMandatoryHandles(vao, vbo, 0, shader, vertexCount));
    }

    public OpenGLHandle LoadCube(string vertShaderPath, string fragShaderPath, float aspect)
    {
        string vertSrc = File.ReadAllText(vertShaderPath);
        string fragSrc = File.ReadAllText(fragShaderPath);
        int shader = GLHelper.CreateShaderProgram(vertSrc, fragSrc);

        float[] vertices = new float[]
        {
            // Front face
            -1f, -1f, 1f, 0f, 0f, 1f, 0f, 0f,
            1f, -1f, 1f, 0f, 0f, 1f, 1f, 0f,
            1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f,
            1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f,
            -1f, 1f, 1f, 0f, 0f, 1f, 0f, 1f,
            -1f, -1f, 1f, 0f, 0f, 1f, 0f, 0f,
            // Back face
            -1f, -1f, -1f, 0f, 0f, -1f, 1f, 0f,
            -1f, 1f, -1f, 0f, 0f, -1f, 1f, 1f,
            1f, 1f, -1f, 0f, 0f, -1f, 0f, 1f,
            1f, 1f, -1f, 0f, 0f, -1f, 0f, 1f,
            1f, -1f, -1f, 0f, 0f, -1f, 0f, 0f,
            -1f, -1f, -1f, 0f, 0f, -1f, 1f, 0f,
            // Left face
            -1f, 1f, 1f, -1f, 0f, 0f, 1f, 0f,
            -1f, 1f, -1f, -1f, 0f, 0f, 1f, 1f,
            -1f, -1f, -1f, -1f, 0f, 0f, 0f, 1f,
            -1f, -1f, -1f, -1f, 0f, 0f, 0f, 1f,
            -1f, -1f, 1f, -1f, 0f, 0f, 0f, 0f,
            -1f, 1f, 1f, -1f, 0f, 0f, 1f, 0f,
            // Right face
            1f, 1f, 1f, 1f, 0f, 0f, 0f, 0f,
            1f, -1f, -1f, 1f, 0f, 0f, 1f, 1f,
            1f, 1f, -1f, 1f, 0f, 0f, 1f, 0f,
            1f, -1f, -1f, 1f, 0f, 0f, 1f, 1f,
            1f, 1f, 1f, 1f, 0f, 0f, 0f, 0f,
            1f, -1f, 1f, 1f, 0f, 0f, 0f, 1f,
            // Top face
            -1f, 1f, -1f, 0f, 1f, 0f, 0f, 1f,
            -1f, 1f, 1f, 0f, 1f, 0f, 0f, 0f,
            1f, 1f, 1f, 0f, 1f, 0f, 1f, 0f,
            1f, 1f, 1f, 0f, 1f, 0f, 1f, 0f,
            1f, 1f, -1f, 0f, 1f, 0f, 1f, 1f,
            -1f, 1f, -1f, 0f, 1f, 0f, 0f, 1f,
            // Bottom face
            -1f, -1f, -1f, 0f, -1f, 0f, 1f, 1f,
            1f, -1f, 1f, 0f, -1f, 0f, 0f, 0f,
            -1f, -1f, 1f, 0f, -1f, 0f, 1f, 0f,
            1f, -1f, 1f, 0f, -1f, 0f, 0f, 0f,
            -1f, -1f, -1f, 0f, -1f, 0f, 1f, 1f,
            1f, -1f, -1f, 0f, -1f, 0f, 0f, 1f,
        };

        int vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        GL.BindVertexArray(0);

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Matrix4.CreateTranslation(0f, 0f, -5f);
        Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), aspect, 0.1f, 100f);
        GL.UseProgram(shader);
        GL.UniformMatrix4(GL.GetUniformLocation(shader, "model"), false, ref model);
        GL.UniformMatrix4(GL.GetUniformLocation(shader, "view"), false, ref view);
        GL.UniformMatrix4(GL.GetUniformLocation(shader, "projection"), false, ref proj);
        GL.UseProgram(0);

        int vertexCount = vertices.Length / 8;
        return new OpenGLHandle(new OpenGLMandatoryHandles(vao, vbo, 0, shader, vertexCount));
    }

    public OpenGLHandle LoadSphere(string vertShaderPath, string fragShaderPath, float aspect, int sectorCount = 36,
        int stackCount = 18)
    {
        string vertSrc = File.ReadAllText(vertShaderPath);
        string fragSrc = File.ReadAllText(fragShaderPath);
        int shader = GLHelper.CreateShaderProgram(vertSrc, fragSrc);

        // Build vertices in a grid (each vertex: pos, normal, tex)
        List<float> vertices = new List<float>();
        float radius = 1f;
        float pi = MathF.PI, twoPi = 2f * pi;
        for (int i = 0; i <= stackCount; ++i)
        {
            float stackAngle = pi / 2 - i * (pi / stackCount); // from pi/2 to -pi/2
            float xy = radius * MathF.Cos(stackAngle);
            float z = radius * MathF.Sin(stackAngle);
            for (int j = 0; j <= sectorCount; ++j)
            {
                float sectorAngle = j * twoPi / sectorCount;
                float x = xy * MathF.Cos(sectorAngle);
                float y = xy * MathF.Sin(sectorAngle);
                // Normalize for normal
                Vector3 norm = new Vector3(x, y, z).Normalized();
                float s = (float)j / sectorCount;
                float t = (float)i / stackCount;
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);
                vertices.Add(norm.X);
                vertices.Add(norm.Y);
                vertices.Add(norm.Z);
                vertices.Add(s);
                vertices.Add(t);
            }
        }

        // Build triangle indices (quad split into two triangles)
        List<int> indices = new List<int>();
        int rowCount = sectorCount + 1;
        for (int i = 0; i < stackCount; i++)
        {
            for (int j = 0; j < sectorCount; j++)
            {
                int first = i * rowCount + j;
                int second = first + rowCount;
                indices.Add(first);
                indices.Add(second);
                indices.Add(first + 1);

                indices.Add(first + 1);
                indices.Add(second);
                indices.Add(second + 1);
            }
        }

        // Assemble final vertex array
        float[] finalVertices = new float[indices.Count * 8];
        for (int i = 0; i < indices.Count; i++)
        {
            int idx = indices[i];
            finalVertices[i * 8 + 0] = vertices[idx * 8 + 0];
            finalVertices[i * 8 + 1] = vertices[idx * 8 + 1];
            finalVertices[i * 8 + 2] = vertices[idx * 8 + 2];
            finalVertices[i * 8 + 3] = vertices[idx * 8 + 3];
            finalVertices[i * 8 + 4] = vertices[idx * 8 + 4];
            finalVertices[i * 8 + 5] = vertices[idx * 8 + 5];
            finalVertices[i * 8 + 6] = vertices[idx * 8 + 6];
            finalVertices[i * 8 + 7] = vertices[idx * 8 + 7];
        }

        int vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, finalVertices.Length * sizeof(float), finalVertices,
            BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        GL.BindVertexArray(0);

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Matrix4.CreateTranslation(0f, 0f, -3f);
        Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), aspect, 0.1f, 100f);
        GL.UseProgram(shader);
        GL.UniformMatrix4(GL.GetUniformLocation(shader, "model"), false, ref model);
        GL.UniformMatrix4(GL.GetUniformLocation(shader, "view"), false, ref view);
        GL.UniformMatrix4(GL.GetUniformLocation(shader, "projection"), false, ref proj);
        GL.UseProgram(0);

        int vertexCount = finalVertices.Length / 8;
        return new OpenGLHandle(new OpenGLMandatoryHandles(vao, vbo, 0, shader, vertexCount));
    }


    public void DrawModelGL(OpenGLHandle glHandle, Matrix4 modelMatrix, CameraGL3D camera, nint texturePointer)
    {
        // Update transformation matrices.
        GL.UseProgram(glHandle.Handles.Shader);
        int modelLoc = GL.GetUniformLocation(glHandle.Handles.Shader, "model");
        int viewLoc = GL.GetUniformLocation(glHandle.Handles.Shader, "view");
        int projLoc = GL.GetUniformLocation(glHandle.Handles.Shader, "projection");

        Matrix4 camView = camera.View;
        Matrix4 proj = camera.Projection;
        GL.UniformMatrix4(modelLoc, false, ref modelMatrix);
        GL.UniformMatrix4(viewLoc, false, ref camView);
        GL.UniformMatrix4(projLoc, false, ref proj);

        // Set lighting uniforms.
        Vector3 lightDir = new Vector3(1, 1, 1);
        Vector3 lightColor = new Vector3(1.0f, 1.0f, 1.0f);
        Vector3 ambientColor = new Vector3(0.2f, 0.2f, 0.2f);
        GL.Uniform3(GL.GetUniformLocation(glHandle.Handles.Shader, "lightDir"), ref lightDir);
        GL.Uniform3(GL.GetUniformLocation(glHandle.Handles.Shader, "lightColor"), ref lightColor);
        GL.Uniform3(GL.GetUniformLocation(glHandle.Handles.Shader, "ambientColor"), ref ambientColor);

        // Bind texture to TextureUnit 0.
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, (int)texturePointer);
        int texLoc = GL.GetUniformLocation(glHandle.Handles.Shader, "Texture");
        GL.Uniform1(texLoc, 0);

        GL.BindVertexArray(glHandle.Handles.Vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, glHandle.Handles.VertexCount);
        GL.BindVertexArray(0);

        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.UseProgram(0);
    }

    private static void AppendVertex(string faceVertex, List<Vector3> positions, List<Vector2> texCoords,
        List<Vector3> normals, List<float> finalVertices)
    {
        // Expected format: "v/vt/vn" or "v//vn"
        string[] indices = faceVertex.Split('/');
        int posIndex = int.Parse(indices[0]) - 1;
        int texIndex = (indices.Length > 1 && !string.IsNullOrEmpty(indices[1])) ? int.Parse(indices[1]) - 1 : -1;
        int normIndex = (indices.Length > 2 && !string.IsNullOrEmpty(indices[2])) ? int.Parse(indices[2]) - 1 : -1;

        Vector3 pos = positions[posIndex];
        Vector2 tex = (texIndex >= 0 && texIndex < texCoords.Count) ? texCoords[texIndex] : new Vector2(0f, 0f);
        Vector3 norm = (normIndex >= 0 && normIndex < normals.Count) ? normals[normIndex] : new Vector3(0f, 0f, 0f);

        finalVertices.Add(pos.X);
        finalVertices.Add(pos.Y);
        finalVertices.Add(pos.Z);
        finalVertices.Add(norm.X);
        finalVertices.Add(norm.Y);
        finalVertices.Add(norm.Z);
        finalVertices.Add(tex.X);
        finalVertices.Add(tex.Y);
    }
}