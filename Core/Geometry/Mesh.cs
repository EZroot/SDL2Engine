namespace SDL2Engine.Core.Geometry;

public class Mesh
{
    public int Vao { get; }
    public int Vbo { get; }
    public int Ebo { get; }
    public int VertexCount { get; }

    public Mesh(int vao, int vbo, int ebo, int vertexCount)
    {
        Vao = vao;
        Vbo = vbo;
        Ebo = ebo;
        VertexCount = vertexCount;
    }
}