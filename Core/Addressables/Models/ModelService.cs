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

        // parse positions, texture coordinates, and normals
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
                    // (assumes convex polygon)
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

        // (each vertex consists of 8 floats).
        int vertexCount = vertices.Length / 8;
        
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
        Matrix4 view = Matrix4.CreateTranslation(0f, 0f, 0f);
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), aspect, 0.1f, 100f);
        GL.UseProgram(shaderProgram);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref model);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), false, ref view);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);
        GL.UseProgram(0);

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

public OpenGLHandle LoadSphere(string vertShaderPath, string fragShaderPath, float aspect, int sectorCount = 36, int stackCount = 18)
{
    string vertSrc = File.ReadAllText(vertShaderPath);
    string fragSrc = File.ReadAllText(fragShaderPath);
    int shader = GLHelper.CreateShaderProgram(vertSrc, fragSrc);

    List<float> vertices = new List<float>();
    float radius = 1f;
    float pi = MathF.PI, twoPi = 2f * pi;
    for (int i = 0; i <= stackCount; ++i)
    {
        float stackAngle = pi / 2 - i * (pi / stackCount);
        float xy = radius * MathF.Cos(stackAngle);
        float z = radius * MathF.Sin(stackAngle);
        for (int j = 0; j <= sectorCount; ++j)
        {
            float sectorAngle = j * twoPi / sectorCount;
            float x = xy * MathF.Cos(sectorAngle);
            float y = xy * MathF.Sin(sectorAngle);
            Vector3 norm = new Vector3(x, y, z).Normalized();
            float s = (float)j / sectorCount;
            float t = (float)i / stackCount;
            vertices.Add(x); vertices.Add(y); vertices.Add(z);
            vertices.Add(norm.X); vertices.Add(norm.Y); vertices.Add(norm.Z);
            vertices.Add(s); vertices.Add(t);
        }
    }

    List<int> indices = new List<int>();
    int rowCount = sectorCount + 1;
    for (int i = 0; i < stackCount; i++)
    {
        for (int j = 0; j < sectorCount; j++)
        {
            int first = i * rowCount + j;
            int second = first + rowCount;
            indices.Add(first); indices.Add(second); indices.Add(first + 1);
            indices.Add(first + 1); indices.Add(second); indices.Add(second + 1);
        }
    }

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
    GL.BufferData(BufferTarget.ArrayBuffer, finalVertices.Length * sizeof(float), finalVertices, BufferUsageHint.StaticDraw);

    // Position
    GL.EnableVertexAttribArray(0);
    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
    // Normal
    GL.EnableVertexAttribArray(1);
    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
    // TexCoord
    GL.EnableVertexAttribArray(2);
    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
    GL.BindVertexArray(0);

    // Set default transformation uniforms.
    Matrix4 model = Matrix4.Identity;
    Matrix4 view = Matrix4.CreateTranslation(0f, 0f, -3f);
    Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), aspect, 0.1f, 100f);
    GL.UseProgram(shader);
    GL.UniformMatrix4(GL.GetUniformLocation(shader, "model"), false, ref model);
    GL.UniformMatrix4(GL.GetUniformLocation(shader, "view"), false, ref view);
    GL.UniformMatrix4(GL.GetUniformLocation(shader, "projection"), false, ref proj);
    int lsLoc = GL.GetUniformLocation(shader, "lightSpaceMatrix");
    if (lsLoc != -1)
    {
        Matrix4 identity = Matrix4.Identity;
        GL.UniformMatrix4(lsLoc, false, ref identity);
    }
    int shadowLoc = GL.GetUniformLocation(shader, "shadowMap");
    if (shadowLoc != -1)
        GL.Uniform1(shadowLoc, 1);
    GL.UseProgram(0);

    int vertexCount = finalVertices.Length / 8;
    return new OpenGLHandle(new OpenGLMandatoryHandles(vao, vbo, 0, shader, vertexCount));
}

public void DrawModelGL(
    OpenGLHandle glHandle,
    Matrix4 modelMatrix,
    CameraGL3D camera,
    nint diffuseTexPtr,
    Matrix4 lightSpaceMatrix,
    nint shadowMapPtr,
    Vector3 lightDir,
    Vector3 lightColor,
    Vector3 ambientColor)
{
    GL.UseProgram(glHandle.Handles.Shader);

    // --- Set Light Uniforms ---
    int loc = GL.GetUniformLocation(glHandle.Handles.Shader, "lightDir");
    if (loc != -1) GL.Uniform3(loc, ref lightDir);

    loc = GL.GetUniformLocation(glHandle.Handles.Shader, "lightColor");
    if (loc != -1) GL.Uniform3(loc, ref lightColor);

    loc = GL.GetUniformLocation(glHandle.Handles.Shader, "ambientColor");
    if (loc != -1) GL.Uniform3(loc, ref ambientColor);

    var camView = camera.View;
    var camProj = camera.Projection;

    loc = GL.GetUniformLocation(glHandle.Handles.Shader, "model");
    if (loc != -1) GL.UniformMatrix4(loc, false, ref modelMatrix);

    loc = GL.GetUniformLocation(glHandle.Handles.Shader, "view");
    if (loc != -1) GL.UniformMatrix4(loc, false, ref camView);

    loc = GL.GetUniformLocation(glHandle.Handles.Shader, "projection");
    if (loc != -1) GL.UniformMatrix4(loc, false, ref camProj);

    loc = GL.GetUniformLocation(glHandle.Handles.Shader, "lightSpaceMatrix");
    if (loc != -1) GL.UniformMatrix4(loc, false, ref lightSpaceMatrix);

    GL.ActiveTexture(TextureUnit.Texture0);
    GL.BindTexture(TextureTarget.Texture2D, (int)diffuseTexPtr);
    loc = GL.GetUniformLocation(glHandle.Handles.Shader, "diffuseTexture");
    if (loc != -1) GL.Uniform1(loc, 0);

    GL.ActiveTexture(TextureUnit.Texture1);
    GL.BindTexture(TextureTarget.Texture2D, (int)shadowMapPtr);
    loc = GL.GetUniformLocation(glHandle.Handles.Shader, "shadowMap");
    if (loc != -1) GL.Uniform1(loc, 1);

    GL.BindVertexArray(glHandle.Handles.Vao);
    GL.DrawArrays(PrimitiveType.Triangles, 0, glHandle.Handles.VertexCount);

    GL.BindVertexArray(0);
    GL.ActiveTexture(TextureUnit.Texture1);
    GL.BindTexture(TextureTarget.Texture2D, 0);
    GL.ActiveTexture(TextureUnit.Texture0);
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