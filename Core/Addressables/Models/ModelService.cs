using System.Globalization;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SDL2Engine.Core.Addressables.Models.Interfaces;
using SDL2Engine.Core.Cameras;
using SDL2Engine.Core.Geometry;
using SDL2Engine.Core.Utils;

public class ModelService : IModelService
{
    /// <summary>
    /// Loads a .Obj 3d model
    /// </summary>
    /// <param name="path"></param>
    /// <param name="vertShaderPath"></param>
    /// <param name="fragShaderPath"></param>
    /// <param name="aspect"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public OpenGLHandle Load3DModel(string path, string vertShaderPath, string fragShaderPath, float aspect)
    {
        // Load and compile shaders.
        string vertSrc = FileHelper.ReadFileContents(vertShaderPath);
        string fragSrc = FileHelper.ReadFileContents(fragShaderPath);
        int shaderProgram = GLHelper.CreateShaderProgram(vertSrc, fragSrc);

        var positions = new List<Vector3>();
        var texCoords = new List<Vector2>();
        var normals = new List<Vector3>();
        var finalVerts = new List<float>();
        var culture = CultureInfo.InvariantCulture;

        foreach (string line in File.ReadLines(path))
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed[0] == '#')
                continue;

            string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            switch (parts[0])
            {
                case "v":
                    if (parts.Length < 4) continue;
                    positions.Add(new Vector3(
                        float.Parse(parts[1], culture),
                        float.Parse(parts[2], culture),
                        float.Parse(parts[3], culture)));
                    break;
                case "vt":
                    if (parts.Length < 3) continue;
                    float u = float.Parse(parts[1], culture);
                    float v = 1f - float.Parse(parts[2], culture);
                    texCoords.Add(new Vector2(u, v));
                    break;
                case "vn":
                    if (parts.Length < 4) continue;
                    normals.Add(new Vector3(
                        float.Parse(parts[1], culture),
                        float.Parse(parts[2], culture),
                        float.Parse(parts[3], culture)));
                    break;
                case "f":
                    // Assumes convex polygon faces.
                    int count = parts.Length - 1;
                    for (int i = 1; i < count - 1; i++)
                    {
                        AppendVertex(parts[1], positions, texCoords, normals, finalVerts);
                        AppendVertex(parts[i + 1], positions, texCoords, normals, finalVerts);
                        AppendVertex(parts[i + 2], positions, texCoords, normals, finalVerts);
                    }

                    break;
            }
        }

        var vertices = finalVerts.ToArray();
        if (vertices.Length == 0)
            throw new Exception("No vertices loaded from model file.");

        int vertexCount = vertices.Length / 8;

        // Generate and bind buffers.
        int vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        const int stride = 8 * sizeof(float);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.BindVertexArray(0);

        // Set default transforms.
        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Matrix4.Identity;
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(90f), aspect, 0.1f, 100f);

        // Set uniforms.
        // GL.UseProgram(shaderProgram);
        // GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref model);
        // GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), false, ref view);
        // GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);

        // GL.UseProgram(0);

        return new OpenGLHandle(new OpenGLMandatoryHandles(vao, vbo, 0, shaderProgram, vertexCount));
    }

    public Mesh LoadModel(string path)
    {
        var positions = new List<Vector3>();
        var texCoords = new List<Vector2>();
        var normals = new List<Vector3>();
        var finalVerts = new List<float>();
        var culture = CultureInfo.InvariantCulture;

        foreach (string line in File.ReadLines(path))
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                continue;

            string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            switch (parts[0])
            {
                case "v":
                    // Vertex position
                    if (parts.Length < 4) continue;
                    positions.Add(new Vector3(
                        float.Parse(parts[1], culture),
                        float.Parse(parts[2], culture),
                        float.Parse(parts[3], culture)));
                    break;

                case "vt":
                    // Texture coordinate
                    if (parts.Length < 3) continue;
                    float u = float.Parse(parts[1], culture);
                    float v = float.Parse(parts[2], culture);
                    // Some OBJ files store 0 at top-left. Flip if needed:
                    v = 1f - v;
                    texCoords.Add(new Vector2(u, v));
                    break;

                case "vn":
                    // Normal
                    if (parts.Length < 4) continue;
                    normals.Add(new Vector3(
                        float.Parse(parts[1], culture),
                        float.Parse(parts[2], culture),
                        float.Parse(parts[3], culture)));
                    break;

                case "f":
                    // Face, potentially "f v1 v2 v3 ..." or "f v1/t1/n1 v2/t2/n2 ..."
                    // We assume convex or triangular fan. E.g.  "f 1/1/1 2/2/2 3/3/3"
                    // We'll triangulate if there's > 3 vertices in one face line.
                    int count = parts.Length - 1; // number of face-vertices in this line
                    for (int i = 1; i < count - 1; i++)
                    {
                        // Build triangles [1, i+1, i+2]
                        AppendVertex(parts[1], positions, texCoords, normals, finalVerts);
                        AppendVertex(parts[i + 1], positions, texCoords, normals, finalVerts);
                        AppendVertex(parts[i + 2], positions, texCoords, normals, finalVerts);
                    }

                    break;
            }
        }

        float[] vertices = finalVerts.ToArray();
        if (vertices.Length == 0)
            throw new Exception($"No vertices loaded from OBJ file '{path}'.");

        // 3 (pos) + 3 (normal) + 2 (texCoord) = 8 floats per vertex
        int vertexCount = vertices.Length / 8;

        // Create and bind buffers
        int vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer,
            vertices.Length * sizeof(float),
            vertices,
            BufferUsageHint.StaticDraw);

        // Interleaved layout: [pos.x, pos.y, pos.z, norm.x, norm.y, norm.z, uv.x, uv.y]
        const int stride = 8 * sizeof(float);

        // Position attribute (location=0)
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

        // Normal attribute (location=1)
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

        // UV attribute (location=2)
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

        GL.BindVertexArray(0);

        // Return your mesh
        return new Mesh(vao, vbo, 0, vertexCount);
    }

    public Mesh CreateQuad()
    {
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
        int vertexCount = vertices.Length / 8;
        return new Mesh(vao, vbo, 0, vertexCount);
    }

    public Mesh CreateSphere()
    {
        var stackCount = 18;
        var sectorCount = 36;
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
        int vertexCount = finalVertices.Length / 8;
        return new Mesh(vao, vbo, 0, vertexCount);
    }

    public Mesh CreateArrowMesh()
    {
        float[] arrowVerts = GenerateBetterArrowGeometry();

        int arrowVao = GL.GenVertexArray();
        int arrowVbo = GL.GenBuffer();
        GL.BindVertexArray(arrowVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, arrowVbo);

        // 6 floats per vertex: 3 for position + 3 for normal
        int stride = 6 * sizeof(float);

        GL.BufferData(BufferTarget.ArrayBuffer,
            arrowVerts.Length * sizeof(float),
            arrowVerts,
            BufferUsageHint.StaticDraw);

        // Position attribute
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float,
            false, stride, 0);

        // Normal attribute
        GL.EnableVertexAttribArray(1);
        // offset by 3 floats for the normal
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float,
            false, stride, 3 * sizeof(float));

        GL.BindVertexArray(0);

        // Each vertex is 6 floats -> arrowVerts.Length / 6 is total vertex count
        int arrowVertCount = arrowVerts.Length / 6;

        return new Mesh(arrowVao, arrowVbo, 0, arrowVertCount);
    }


    public OpenGLHandle Create3DArrow(string vertShaderPath, string fragShaderPath)
    {
        string vertShaderSrc = FileHelper.ReadFileContents(vertShaderPath);
        string fragShaderSrc = FileHelper.ReadFileContents(fragShaderPath);

        var arrowShader = GLHelper.CreateShaderProgram(vertShaderSrc, fragShaderSrc);
        float[] arrowVerts = GenerateBetterArrowGeometry();

        int arrowVao = GL.GenVertexArray();
        int arrowVbo = GL.GenBuffer();
        GL.BindVertexArray(arrowVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, arrowVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, arrowVerts.Length * sizeof(float),
            arrowVerts, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

        GL.BindVertexArray(0);

        int arrowVertCount = arrowVerts.Length / 3;

        return new OpenGLHandle(new OpenGLMandatoryHandles(arrowVao, arrowVbo, 0, arrowShader, arrowVertCount));
    }

    public OpenGLHandle CreateQuad(string vertShaderPath, string fragShaderPath, float aspect)
    {
        string vertSrc = FileHelper.ReadFileContents(vertShaderPath);
        string fragSrc = FileHelper.ReadFileContents(fragShaderPath);
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

    public OpenGLHandle CreateCube(string vertShaderPath, string fragShaderPath, float aspect)
    {
        string vertSrc = FileHelper.ReadFileContents(vertShaderPath);
        string fragSrc = FileHelper.ReadFileContents(fragShaderPath);
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

    public OpenGLHandle CreateSphere(string vertShaderPath, string fragShaderPath, float aspect, int sectorCount = 36,
        int stackCount = 18)
    {
        string vertSrc = FileHelper.ReadFileContents(vertShaderPath);
        string fragSrc = FileHelper.ReadFileContents(fragShaderPath);
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

    public OpenGLHandle CreateFullscreenQuad(string vertShaderPath, string fragShaderPath)
    {
        string vertSrc = FileHelper.ReadFileContents(vertShaderPath);
        string fragSrc = FileHelper.ReadFileContents(fragShaderPath);
        int shader = GLHelper.CreateShaderProgram(vertSrc, fragSrc);

        int vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        float[] vertices = new float[]
        {
            // First triangle.
            -1.0f, 1.0f, 0.0f, 1.0f,
            -1.0f, -1.0f, 0.0f, 0.0f,
            1.0f, -1.0f, 1.0f, 0.0f,

            // Second triangle.
            -1.0f, 1.0f, 0.0f, 1.0f,
            1.0f, -1.0f, 1.0f, 0.0f,
            1.0f, 1.0f, 1.0f, 1.0f,
        };

        // upload vertex data
        GL.BufferData(BufferTarget.ArrayBuffer,
            vertices.Length * sizeof(float),
            vertices,
            BufferUsageHint.StaticDraw);

        int stride = 4 * sizeof(float);
        // Position attribute (location = 0): 2 floats, offset 0.
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        // TexCoord attribute (location = 1): 2 floats, offset 2 * sizeof(float).
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);

        var mandatoryHandles = new OpenGLMandatoryHandles(vao, vbo, 0, shader, 6);
        return new OpenGLHandle(mandatoryHandles);
    }

    public void DrawArrow(OpenGLHandle arrowHandle, CameraGL3D cam, Vector3 position, Vector3 dirNormalized)
    {
        var arrowShader = arrowHandle.Handles.Shader;
        // Arrow is modeled along +Z
        Vector3 forward = -dirNormalized;
        Vector3 zAxis = Vector3.UnitZ;
        // Cross/dot for orientation.
        Vector3 axis = Vector3.Cross(zAxis, forward);
        float dot = MathF.Max(-1f, MathF.Min(1f, Vector3.Dot(zAxis, forward)));
        float angle = MathF.Acos(dot);

        Quaternion orientation = Quaternion.FromAxisAngle(axis.Normalized(), angle);

        // Scale, then translate to light position
        Matrix4 arrowModel =
            Matrix4.CreateFromQuaternion(orientation) *
            Matrix4.CreateScale(5f) *
            Matrix4.CreateTranslation(position);

        // Use unlit arrow shader
        GL.UseProgram(arrowShader);

        // Send transforms
        int modelLoc = GL.GetUniformLocation(arrowShader, "uModel");
        int viewLoc = GL.GetUniformLocation(arrowShader, "uView");
        int projLoc = GL.GetUniformLocation(arrowShader, "uProjection");

        GL.UniformMatrix4(modelLoc, false, ref arrowModel);

        Matrix4 view = cam.View;
        GL.UniformMatrix4(viewLoc, false, ref view);

        Matrix4 proj = cam.Projection;
        GL.UniformMatrix4(projLoc, false, ref proj);

        // Optional arrow color
        int colorLoc = GL.GetUniformLocation(arrowShader, "uColor");
        if (colorLoc != -1)
        {
            Vector3 arrowColor = new Vector3(1f, 0f, 0f);
            GL.Uniform3(colorLoc, ref arrowColor);
        }

        // Draw arrow geometry
        GL.BindVertexArray(arrowHandle.Handles.Vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, arrowHandle.Handles.VertexCount);
        GL.BindVertexArray(0);

        GL.UseProgram(0);
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

    private float[] GenerateBetterArrowGeometry(
        float shaftRadius = 0.2f,
        float shaftLength = 5.7f,
        float tipRadius = 0.6f,
        float tipLength = 1.3f,
        int segments = 16)
    {
        List<float> verts = new List<float>();

        float tipZStart = shaftLength;
        float tipZEnd = shaftLength + tipLength;

        Vector3 shaftTopCenter = new Vector3(0, 0, shaftLength);
        Vector3 tipApex = new Vector3(0, 0, tipZEnd);

        // 1) CYLINDER SIDE FACES
        // ----------------------
        // We'll create quads around the circumference, each made of 2 triangles:
        //   (p0, p2, p1) and (p1, p2, p3)
        for (int i = 0; i < segments; i++)
        {
            float theta = 2f * MathF.PI * (i / (float)segments);
            float nextTheta = 2f * MathF.PI * ((i + 1) % segments / (float)segments);

            // base circle points (z=0)
            Vector3 p0 = new Vector3(
                shaftRadius * MathF.Cos(theta),
                shaftRadius * MathF.Sin(theta),
                0f);

            Vector3 p1 = new Vector3(
                shaftRadius * MathF.Cos(nextTheta),
                shaftRadius * MathF.Sin(nextTheta),
                0f);

            // top circle points (z=shaftLength)
            Vector3 p2 = p0 + new Vector3(0, 0, shaftLength);
            Vector3 p3 = p1 + new Vector3(0, 0, shaftLength);

            // First triangle: (p0, p2, p1)
            AddFace(verts, p0, p2, p1);

            // Second triangle: (p1, p2, p3)
            AddFace(verts, p1, p2, p3);
        }

        // 2) SHAFT TOP DISK
        // -----------------
        // Triangle fan for the top circle at z=shaftLength (normal = +Z)
        // If you actually want a “closed bottom” at z=0, do a similar fan with normal = -Z.
        for (int i = 0; i < segments; i++)
        {
            float theta = 2f * MathF.PI * (i / (float)segments);
            float nextTheta = 2f * MathF.PI * ((i + 1) % segments / (float)segments);

            Vector3 p0 = new Vector3(
                shaftRadius * MathF.Cos(theta),
                shaftRadius * MathF.Sin(theta),
                shaftLength);

            Vector3 p1 = new Vector3(
                shaftRadius * MathF.Cos(nextTheta),
                shaftRadius * MathF.Sin(nextTheta),
                shaftLength);

            // Triangle (shaftTopCenter, p0, p1)
            // The normal for this disk is +Z
            AddFace(verts, shaftTopCenter, p0, p1, overrideNormal: Vector3.UnitZ);
        }

        // 3) CONE TIP
        // -----------
        // A ring base at z=shaftLength, apex at z=shaftLength + tipLength
        // Triangles (b0, apex, b1)
        for (int i = 0; i < segments; i++)
        {
            float theta = 2f * MathF.PI * (i / (float)segments);
            float nextTheta = 2f * MathF.PI * ((i + 1) % segments / (float)segments);

            // base ring circle
            Vector3 b0 = new Vector3(
                tipRadius * MathF.Cos(theta),
                tipRadius * MathF.Sin(theta),
                tipZStart);

            Vector3 b1 = new Vector3(
                tipRadius * MathF.Cos(nextTheta),
                tipRadius * MathF.Sin(nextTheta),
                tipZStart);

            // Single triangle
            AddFace(verts, b0, tipApex, b1);
        }

        return verts.ToArray();
    }

    /// <summary>
    ///  Parses a single "face vertex" token (e.g. "1/2/3" or "1//3") from OBJ,
    ///  then appends the corresponding position, normal, and texCoord
    ///  to the finalVerts list in the order [pos.x, pos.y, pos.z,
    ///                                      norm.x, norm.y, norm.z,
    ///                                      tex.x,  tex.y].
    /// </summary>
    private void AppendVertex(
        string faceVertex,
        List<Vector3> positions,
        List<Vector2> texCoords,
        List<Vector3> normals,
        List<float> finalVerts)
    {
        // faceVertex is typically something like "vIndex/tIndex/nIndex"
        // or "vIndex//nIndex". Example: "12/5/9" -> positions[11], texCoords[4], normals[8]
        // (OBJ indices are 1-based)
        string[] indices = faceVertex.Split('/');

        // Positions are required
        int posIndex = int.Parse(indices[0]) - 1;
        Vector3 pos = positions[posIndex];

        // Texture coords might be missing
        Vector2 tex = Vector2.Zero;
        if (indices.Length > 1 && !string.IsNullOrEmpty(indices[1]))
        {
            int texIndex = int.Parse(indices[1]) - 1;
            if (texIndex >= 0 && texIndex < texCoords.Count)
                tex = texCoords[texIndex];
        }

        // Normals might be missing
        Vector3 norm = Vector3.Zero;
        if (indices.Length > 2 && !string.IsNullOrEmpty(indices[2]))
        {
            int normIndex = int.Parse(indices[2]) - 1;
            if (normIndex >= 0 && normIndex < normals.Count)
                norm = normals[normIndex];
        }

        // Append to finalVerts: 3 floats pos + 3 floats norm + 2 floats uv
        finalVerts.Add(pos.X);
        finalVerts.Add(pos.Y);
        finalVerts.Add(pos.Z);

        finalVerts.Add(norm.X);
        finalVerts.Add(norm.Y);
        finalVerts.Add(norm.Z);

        finalVerts.Add(tex.X);
        finalVerts.Add(tex.Y);
    }

    /// <summary>
    ///  Adds the 3 vertices of a triangle to the verts list,
    ///  computing a face normal from (v2 - v0) x (v1 - v0).
    ///  Or, if overrideNormal is non-null, uses that normal instead.
    /// </summary>
    private void AddFace(List<float> verts,
        Vector3 v0, Vector3 v1, Vector3 v2,
        Vector3? overrideNormal = null)
    {
        Vector3 faceNormal;
        if (overrideNormal.HasValue)
        {
            faceNormal = overrideNormal.Value;
        }
        else
        {
            // standard face normal
            Vector3 side1 = v1 - v0;
            Vector3 side2 = v2 - v0;
            faceNormal = Vector3.Cross(side1, side2).Normalized();
        }

        // Each triangle has 3 vertices
        // Position (3 floats) + Normal (3 floats) = 6 floats per vertex
        AppendVertex(verts, v0, faceNormal);
        AppendVertex(verts, v1, faceNormal);
        AppendVertex(verts, v2, faceNormal);
    }

    private void AppendVertex(List<float> verts, Vector3 pos, Vector3 norm)
    {
        verts.Add(pos.X);
        verts.Add(pos.Y);
        verts.Add(pos.Z);
        verts.Add(norm.X);
        verts.Add(norm.Y);
        verts.Add(norm.Z);
    }
}